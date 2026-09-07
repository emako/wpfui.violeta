using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Non-client hit-testing for caption buttons.
/// Returns HTMIN/HTMAX/HTHELP so Windows Snap Layouts and system caption semantics work.
/// Close is excluded: it uses <see cref="System.Windows.Shell.WindowChrome.IsHitTestVisibleInChromeProperty"/>
/// and client <c>IsMouseOver</c> to avoid HTCLOSE hot-track flicker.
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
                    CaptionButton? button = GetPointedButton(lParam);
                    if (button is null)
                    {
                        HoveredButton = null;
                        break;
                    }

                    // Close uses client hit-testing; returning HTCLOSE fights custom hover and flickers.
                    if (button.Kind == CaptionButtonKind.Close)
                    {
                        HoveredButton = null;
                        break;
                    }

                    if (button.IsEnabled)
                    {
                        if (PressedButton is not null && PressedButton != button)
                        {
                            PressedButton.IsMouseOverInTitleBar = false;
                            PressedButton.IsPressedInTitleBar = false;
                            break;
                        }

                        if (PressedButton == button)
                        {
                            PressedButton.IsPressedInTitleBar = true;
                        }

                        HoveredButton = button;
                    }

                    handled = true;
                    // HTMAXBUTTON (9) is required for Windows 11 Snap Layouts on the maximize button.
                    return (nint)button.Kind;
                }

            case WM_NCLBUTTONDOWN:
                {
                    CaptionButton? button = GetPointedButton(lParam);
                    if (button is null || button.Kind == CaptionButtonKind.Close)
                    {
                        PressedButton = null;
                        break;
                    }

                    if (button.IsEnabled)
                    {
                        PressedButton = button;
                    }

                    handled = true;
                    break;
                }

            case WM_NCLBUTTONUP:
                {
                    CaptionButton? button = GetPointedButton(lParam);
                    if (button is null || button.Kind == CaptionButtonKind.Close)
                    {
                        PressedButton = null;
                        break;
                    }

                    if (button.IsEnabled && button == PressedButton)
                    {
                        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }

                    PressedButton = null;
                    handled = true;
                    break;
                }

            case WM_NCMOUSELEAVE:
                {
                    HoveredButton = null;
                    PressedButton = null;
                    break;
                }
        }

        return 0;
    }

    private CaptionButton? GetPointedButton(nint lParam)
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
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int WM_NCLBUTTONUP = 0x00A2;
    private const int WM_NCMOUSELEAVE = 0x02A2;
}
