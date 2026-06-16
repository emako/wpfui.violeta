using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Manages a Win32 tray icon and its context menu for WPF applications.
/// </summary>
public partial class TrayIconHost : IDisposable
{
    private readonly nint hWnd = IntPtr.Zero;
    private readonly User32.WndProcDelegate wndProcDelegate = null!;
    private Shell32.NotifyIconData notifyIconData = default;
    private readonly int id = default;
    private static int nextId = 0;
    private readonly uint taskbarCreatedMessageId = 0;

    public TrayThemeMode ThemeMode
    {
        get => field;
        set => SetThemeMode(field = value);
    } = TrayThemeMode.None;

    public string ToolTipText
    {
        get => notifyIconData.szTip;
        set
        {
            notifyIconData.szTip = value;
            notifyIconData.uFlags |= (int)Shell32.NotifyIconFlags.NIF_TIP;
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
        }
    }

    public nint Icon
    {
        get => notifyIconData.hIcon;
        set
        {
            if (notifyIconData.hIcon != IntPtr.Zero)
                _ = User32.DestroyIcon(notifyIconData.hIcon);

            notifyIconData.hIcon = User32.CopyIcon(value);
            notifyIconData.uFlags |= (int)Shell32.NotifyIconFlags.NIF_ICON;
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
        }
    }

    public bool IsVisible
    {
        get => notifyIconData.dwState != (uint)Shell32.NotifyIconState.NIS_HIDDEN;
        set
        {
            notifyIconData.dwState = value ? 0 : (uint)Shell32.NotifyIconState.NIS_HIDDEN;
            notifyIconData.dwStateMask = (uint)(Shell32.NotifyIconState.NIS_HIDDEN | Shell32.NotifyIconState.NIS_SHAREDICON);
            notifyIconData.uFlags |= (int)Shell32.NotifyIconFlags.NIF_STATE;
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref notifyIconData);
        }
    }

    public string BalloonTipText
    {
        get => field;
        set
        {
            if (value != field)
                field = value;
        }
    } = string.Empty;

    public ToolTipIcon BalloonTipIcon
    {
        get => field;
        set
        {
            if ((int)value < 0 || (int)value > 3)
                throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ToolTipIcon));

            if (value != field)
                field = value;
        }
    }

    public string BalloonTipTitle
    {
        get => field;
        set
        {
            if (value != field)
                field = value;
        }
    } = string.Empty;

    public object? Tag { get; set; } = null;

    public TrayMenu Menu { get; set; } = null!;

    public TrayContextMenuHorizontalAlignment MenuHorizontalAlignment { get; set; } = TrayContextMenuHorizontalAlignment.Left;

    public TrayContextMenuVerticalAlignment MenuVerticalAlignment { get; set; } = TrayContextMenuVerticalAlignment.Top;

    public event EventHandler<EventArgs>? UserPreferenceChanged = null;
    public event EventHandler<EventArgs>? BalloonTipClicked = null;
    public event EventHandler<EventArgs>? BalloonTipClosed = null;
    public event EventHandler<EventArgs>? BalloonTipShown = null;
    public event EventHandler<EventArgs>? Click = null;
    public event EventHandler<EventArgs>? RightDown = null;
    public event EventHandler<EventArgs>? RightClick = null;
    public event EventHandler<EventArgs>? RightDoubleClick = null;
    public event EventHandler<EventArgs>? LeftDown = null;
    public event EventHandler<EventArgs>? LeftClick = null;
    public event EventHandler<EventArgs>? LeftDoubleClick = null;
    public event EventHandler<EventArgs>? MiddleDown = null;
    public event EventHandler<EventArgs>? MiddleClick = null;
    public event EventHandler<EventArgs>? MiddleDoubleClick = null;

    public TrayIconHost()
    {
        id = ++nextId;
        taskbarCreatedMessageId = User32.RegisterWindowMessage("TaskbarCreated");

        wndProcDelegate = new User32.WndProcDelegate(WndProc);

        User32.WNDCLASS wc = new()
        {
            lpszClassName = "TrayIconHostWindowClass",
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate)
        };
        User32.RegisterClass(ref wc);

        hWnd = User32.CreateWindowEx(
            0,
            "TrayIconHostWindowClass",
            "TrayIconHostWindow",
            0,
            0,
            0,
            0,
            0,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        notifyIconData = new Shell32.NotifyIconData()
        {
            cbSize = Marshal.SizeOf<Shell32.NotifyIconData>(),
            hWnd = hWnd,
            uID = id,
            uFlags = (int)(Shell32.NotifyIconFlags.NIF_ICON | Shell32.NotifyIconFlags.NIF_MESSAGE | Shell32.NotifyIconFlags.NIF_TIP),
            uCallbackMessage = (int)User32.WindowMessage.WM_TRAYICON,
            hIcon = IntPtr.Zero,
            szTip = null!,
        };

        _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_ADD, ref notifyIconData);
    }

    protected virtual nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == taskbarCreatedMessageId)
        {
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_ADD, ref notifyIconData);
            return IntPtr.Zero;
        }

        if (msg == (uint)User32.WindowMessage.WM_TRAYICON)
        {
            if ((int)wParam == id)
            {
                User32.WindowMessage mouseMsg = (User32.WindowMessage)lParam;

                switch (mouseMsg)
                {
                    case User32.WindowMessage.WM_QUERYENDSESSION:
                    case User32.WindowMessage.WM_ENDSESSION:
                        _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_DELETE, ref notifyIconData);
                        break;
                    case User32.WindowMessage.WM_LBUTTONDOWN:
                        LeftDown?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_LBUTTONUP:
                        LeftClick?.Invoke(this, EventArgs.Empty);
                        Click?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_LBUTTONDBLCLK:
                        LeftDoubleClick?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_RBUTTONDOWN:
                        RightDown?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_RBUTTONUP:
                        RightClick?.Invoke(this, EventArgs.Empty);
                        ShowContextMenu();
                        break;
                    case User32.WindowMessage.WM_RBUTTONDBLCLK:
                        RightDoubleClick?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_MBUTTONDOWN:
                        MiddleDown?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_MBUTTONUP:
                        MiddleClick?.Invoke(this, EventArgs.Empty);
                        Click?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_MBUTTONDBLCLK:
                        MiddleDoubleClick?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONSHOW:
                        BalloonTipShown?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONHIDE:
                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONTIMEOUT:
                        BalloonTipClosed?.Invoke(this, EventArgs.Empty);
                        break;
                    case User32.WindowMessage.WM_NOTIFYICON_BALLOONUSERCLICK:
                        BalloonTipClicked?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
        }
        else if (msg == (uint)User32.WindowMessage.WM_SETTINGCHANGE)
        {
            if (ThemeMode != TrayThemeMode.None)
            {
                string? area = Marshal.PtrToStringUni(lParam);
                if (string.Equals(area, "ImmersiveColorSet", StringComparison.Ordinal))
                {
                    if (ThemeMode == TrayThemeMode.System)
                        SetThemeMode(TrayThemeMode.System);

                    UserPreferenceChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        else if (msg == (uint)User32.WindowMessage.WM_THEMECHANGED)
        {
            if (ThemeMode != TrayThemeMode.None)
            {
                if (ThemeMode == TrayThemeMode.System)
                    SetThemeMode(TrayThemeMode.System);

                UserPreferenceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        return User32.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public virtual void ShowContextMenu()
    {
        Menu?.Open(hWnd, MenuHorizontalAlignment, MenuVerticalAlignment);
    }

    public virtual void ShowBalloonTip(int timeout)
    {
        ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
    }

    public virtual void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
    {
        if (timeout < 0)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        if (string.IsNullOrEmpty(tipText))
            throw new ArgumentException("NotifyIconEmptyOrNullTipText");

        if ((int)tipIcon < 0 || (int)tipIcon > 3)
            throw new InvalidEnumArgumentException(nameof(tipIcon), (int)tipIcon, typeof(ToolTipIcon));

        var balloonData = new Shell32.NotifyIconData()
        {
            cbSize = Marshal.SizeOf<Shell32.NotifyIconData>(),
            hWnd = hWnd,
            uID = id,
            uFlags = (int)Shell32.NotifyIconFlags.NIF_INFO,
            uTimeoutOrVersion = (uint)timeout,
            szInfoTitle = tipTitle,
            szInfo = tipText,
            dwInfoFlags = tipIcon switch
            {
                ToolTipIcon.Info => 1,
                ToolTipIcon.Warning => 2,
                ToolTipIcon.Error => 3,
                ToolTipIcon.None or _ => 0,
            },
        };

        _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_MODIFY, ref balloonData);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ = Shell32.Shell_NotifyIcon((int)Shell32.NOTIFY_COMMAND.NIM_DELETE, ref notifyIconData);

            if (notifyIconData.hIcon != IntPtr.Zero)
            {
                _ = User32.DestroyIcon(notifyIconData.hIcon);
                notifyIconData.hIcon = IntPtr.Zero;
            }

            if (hWnd != IntPtr.Zero)
            {
                _ = User32.DestroyWindow(hWnd);
            }
        }
    }

    ~TrayIconHost()
    {
        Dispose(false);
    }
}

/// <summary>
/// Defines a set of standardized icons that can be associated with a ToolTip.
/// </summary>
public enum ToolTipIcon
{
    None = 0x00,
    Info = 0x01,
    Warning = 0x02,
    Error = 0x03,
}
