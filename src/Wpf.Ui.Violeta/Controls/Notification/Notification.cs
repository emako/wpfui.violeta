using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Static balloon-tip helper similar to <see cref="Toast"/>, backed by a transient
/// hidden <see cref="TrayIconHost"/> so callers do not need an application tray icon.
/// The host is created on demand and released after the tip ends.
/// </summary>
public static class Notification
{
    public const int DefaultTimeout = 5000;

    private const int DisposeGraceMilliseconds = 1000;

    private static readonly object _sync = new();
    private static TrayIconHost? _host;
    private static DispatcherTimer? _disposeTimer;
    private static bool _exitHooked;

    /// <summary>
    /// Optional tray icon handle used by the internal host. When unset, the current process icon is used.
    /// Copied on each show; ownership remains with the caller.
    /// </summary>
    public static nint Icon { get; set; }

    public static void Information(string title, string content, int timeout = DefaultTimeout)
        => Show(title, content, ToolTipIcon.Info, timeout);

    public static void Warning(string title, string content, int timeout = DefaultTimeout)
        => Show(title, content, ToolTipIcon.Warning, timeout);

    public static void Error(string title, string content, int timeout = DefaultTimeout)
        => Show(title, content, ToolTipIcon.Error, timeout);

    public static void Show(string title, string content, ToolTipIcon icon = ToolTipIcon.None, int timeout = DefaultTimeout)
    {
        if (string.IsNullOrEmpty(content))
            throw new ArgumentException("Notification content cannot be null or empty.", nameof(content));

        if (timeout < 0)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => ShowCore(title, content, icon, timeout));
            return;
        }

        ShowCore(title, content, icon, timeout);
    }

    private static void ShowCore(string title, string content, ToolTipIcon icon, int timeout)
    {
        TrayIconHost host = EnsureHost();
        host.ShowBalloonTip(timeout, title ?? string.Empty, content, icon);
        ScheduleRelease(timeout);
    }

    private static TrayIconHost EnsureHost()
    {
        lock (_sync)
        {
            if (_host is not null)
                return _host;

            TrayIconHost host = new()
            {
                ToolTipText = Application.Current?.MainWindow?.Title
                    ?? Process.GetCurrentProcess().ProcessName
                    ?? nameof(Notification),
            };

            nint iconHandle = Icon != IntPtr.Zero ? Icon : TryGetProcessIcon();
            if (iconHandle != IntPtr.Zero)
            {
                host.Icon = iconHandle;
                if (Icon == IntPtr.Zero)
                    _ = User32.DestroyIcon(iconHandle);
            }

            host.IsVisible = false;
            host.BalloonTipClosed += OnBalloonTipClosed;

            if (!_exitHooked && Application.Current is { } app)
            {
                app.Exit += OnApplicationExit;
                _exitHooked = true;
            }

            _host = host;
            return host;
        }
    }

    private static void ScheduleRelease(int timeout)
    {
        _disposeTimer ??= new DispatcherTimer(DispatcherPriority.Background);
        _disposeTimer.Tick -= OnDisposeTimerTick;
        _disposeTimer.Tick += OnDisposeTimerTick;
        _disposeTimer.Interval = TimeSpan.FromMilliseconds(timeout + DisposeGraceMilliseconds);
        _disposeTimer.Stop();
        _disposeTimer.Start();
    }

    private static void OnDisposeTimerTick(object? sender, EventArgs e)
        => ReleaseHost();

    private static void OnBalloonTipClosed(object? sender, EventArgs e)
        => ScheduleRelease(0);

    private static void OnApplicationExit(object sender, ExitEventArgs e)
        => ReleaseHost();

    private static void ReleaseHost()
    {
        lock (_sync)
        {
            if (_disposeTimer is not null)
            {
                _disposeTimer.Stop();
                _disposeTimer.Tick -= OnDisposeTimerTick;
                _disposeTimer = null;
            }

            if (_host is null)
                return;

            _host.BalloonTipClosed -= OnBalloonTipClosed;
            _host.Dispose();
            _host = null;
        }
    }

    private static nint TryGetProcessIcon()
    {
        try
        {
            string path = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (path.Length == 0)
                return IntPtr.Zero;

            nint[] large = new nint[1];
            nint[] small = new nint[1];
            _ = ExtractIconEx(path, 0, large, small, 1);

            nint chosen = small[0] != IntPtr.Zero ? small[0] : large[0];
            nint unused = chosen == small[0] ? large[0] : small[0];
            if (unused != IntPtr.Zero && unused != chosen)
                _ = User32.DestroyIcon(unused);

            return chosen;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(string lpszFile, int nIconIndex, nint[] phiconLarge, nint[] phiconSmall, uint nIcons);
}
