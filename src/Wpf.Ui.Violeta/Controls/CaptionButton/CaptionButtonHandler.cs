using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Non-client caption-button interaction.
/// <para>
/// <see cref="WM_NCHITTEST"/> only reports HT codes to Windows (Snap Layouts, system semantics).
/// Hover is updated on <see cref="WM_NCMOUSEMOVE"/> and cleared on <see cref="WM_NCMOUSELEAVE"/>,
/// so DWM close hot-tracking probes that re-enter NCHITTEST cannot clear custom hover.
/// </para>
/// </summary>
public sealed class CaptionButtonHandler : IDisposable
{
    public CaptionButtonHandler(HwndSource hwndSource)
    {
        _hwndSource = hwndSource;
        _hwndSource.AddHook(OnHwndSourceMessage);
    }

    public void Add(CaptionButton? button)
    {
        if (button is null)
        {
            return;
        }

        _buttons.Add(button);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        HoveredButton = null;
        PressedButton = null;
        _hwndSource.RemoveHook(OnHwndSourceMessage);
    }

    private nint OnHwndSourceMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_NCHITTEST:
                {
                    // Hit-test only: answer Windows with HTxxx. Do not touch hover here —
                    // DWM may re-query NCHITTEST at unrelated points after HTCLOSE.
                    CaptionButton? hit = HitTestCaptionButton(lParam);
                    if (hit is null)
                    {
                        break;
                    }

                    handled = true;
                    return (nint)hit.Kind;
                }

            case WM_NCMOUSEMOVE:
                {
                    CaptionButton? hit = HitTestCaptionButton(lParam);
                    HoveredButton = hit is { IsEnabled: true } ? hit : null;
                    EnsureNonClientMouseTracking(hwnd);
                    break;
                }

            case WM_NCLBUTTONDOWN:
                {
                    CaptionButton? hit = HitTestCaptionButton(lParam);
                    if (hit is null)
                    {
                        PressedButton = null;
                        break;
                    }

                    if (hit.IsEnabled)
                    {
                        PressedButton = hit;
                    }

                    handled = true;
                    break;
                }

            case WM_NCLBUTTONUP:
                {
                    CaptionButton? hit = HitTestCaptionButton(lParam);
                    if (hit is null)
                    {
                        PressedButton = null;
                        break;
                    }

                    if (hit.IsEnabled && hit == PressedButton)
                    {
                        hit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }

                    PressedButton = null;
                    handled = true;
                    break;
                }

            case WM_NCMOUSELEAVE:
                {
                    _trackingNonClientMouse = false;
                    HoveredButton = null;
                    PressedButton = null;
                    break;
                }
        }

        return 0;
    }

    private void EnsureNonClientMouseTracking(nint hwnd)
    {
        if (_trackingNonClientMouse)
        {
            return;
        }

        TRACKMOUSEEVENT tme = new()
        {
            cbSize = Marshal.SizeOf<TRACKMOUSEEVENT>(),
            dwFlags = TME_LEAVE | TME_NONCLIENT,
            hwndTrack = hwnd,
            dwHoverTime = 0,
        };

        if (TrackMouseEvent(ref tme))
        {
            _trackingNonClientMouse = true;
        }
    }

    private CaptionButton? HitTestCaptionButton(nint lParam)
    {
        if (_buttons.Count == 0)
        {
            return null;
        }

        Point pointerScreenPosition = new(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));

        foreach (CaptionButton button in _buttons)
        {
            if (!button.IsVisible || !button.IsLoaded || button.ActualWidth <= 0 || button.ActualHeight <= 0)
            {
                continue;
            }

            Point local;
            try
            {
                local = button.PointFromScreen(pointerScreenPosition);
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            if (local.X >= 0 && local.Y >= 0 && local.X < button.ActualWidth && local.Y < button.ActualHeight)
            {
                return button;
            }
        }

        return null;
    }

    private static int GET_X_LPARAM(nint lParam) => unchecked((short)(lParam & 0xFFFF));

    private static int GET_Y_LPARAM(nint lParam) => unchecked((short)((lParam >> 16) & 0xFFFF));

    private readonly HwndSource _hwndSource;
    private readonly HashSet<CaptionButton> _buttons = [];
    private bool _disposed;
    private bool _trackingNonClientMouse;

    private CaptionButton? HoveredButton
    {
        get;
        set
        {
            field?.IsMouseOverInTitleBar = false;
            field = value;
            field?.IsMouseOverInTitleBar = true;
        }
    }

    private CaptionButton? PressedButton
    {
        get;
        set
        {
            field?.IsPressedInTitleBar = false;
            field = value;
            field?.IsPressedInTitleBar = true;
        }
    }

    private const int WM_NCHITTEST = 0x0084;
    private const int WM_NCMOUSEMOVE = 0x00A0;
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int WM_NCLBUTTONUP = 0x00A2;
    private const int WM_NCMOUSELEAVE = 0x02A2;

    private const uint TME_LEAVE = 0x00000002;
    private const uint TME_NONCLIENT = 0x00000010;

    [StructLayout(LayoutKind.Sequential)]
    private struct TRACKMOUSEEVENT
    {
        public int cbSize;
        public uint dwFlags;
        public nint hwndTrack;
        public uint dwHoverTime;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);
}
