using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Resources.Localization;
using Wpf.Ui.Violeta.Threading;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

public static class PendingBox
{
    public static IPendingHandler Show(string? message = null, string? title = null, bool isShowCancel = false, bool closeOnCanceled = true)
    {
        return Show(null, message, title, isShowCancel, closeOnCanceled);
    }

    public static IPendingHandler Show(Window? owner, string? message, string? title = null, bool isShowCancel = false, bool closeOnCanceled = true)
    {
        return ShowCore(owner, message, title, isShowCancel, closeOnCanceled);
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static IPendingHandler ShowCore(Window? owner, string? message, string? title, bool isShowCancel, bool closeOnCanceled = true)
    {
        owner
            ??= Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;

        Dispatcher dispatcher = owner?.Dispatcher ?? Application.Current.Dispatcher;

        return dispatcher.Invoke(() =>
        {
            PendingBoxDialog dialog = new()
            {
                Owner = owner,
                Title = title ?? string.Empty,
                Message = message ?? (SH.Loading + "..."),
                IsShowCancel = isShowCancel,
                WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
            };
            PendingHandler pending = new(dialog)
            {
                CloseOnCanceled = closeOnCanceled,
            };

            dialog.Closing += (_, e) =>
            {
                if (!e.Cancel)
                {
                    _ = User32.SetForegroundWindow(new WindowInteropHelper(owner).Handle);
                }
            };
            dialog.Closed += (s, e) =>
            {
                pending.RaiseClosedEvent(s, e);
                _ = User32.EnableWindow(new WindowInteropHelper(owner).Handle, true);
                _ = User32.SetForegroundWindow(new WindowInteropHelper(owner).Handle);
            };
            dialog.Canceled += (s, e) =>
            {
                pending.RaiseCanceledEvent(s, e);
                _ = User32.EnableWindow(new WindowInteropHelper(owner).Handle, true);
            };

            _ = User32.EnableWindow(new WindowInteropHelper(owner).Handle, false);
            dialog.Show();
            return pending;
        });
    }

    [Obsolete("Under development")]
    public static STAThread<IPendingHandler> ShowAsync(string? message = null, string? title = null, bool isShowCancel = false, bool closeOnCanceled = true)
    {
        return ShowAsync(null, message, title, isShowCancel, closeOnCanceled);
    }

    [Obsolete("Under development")]
    public static STAThread<IPendingHandler> ShowAsync(Window? owner, string? message, string? title = null, bool isShowCancel = false, bool closeOnCanceled = true)
    {
        owner
            ??= Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;

        STAThread<IPendingHandler> sta = new(sta =>
        {
            sta.Result = ShowCoreAsync(owner, message, title, isShowCancel, closeOnCanceled);
            sta.Result.Show();
        });
        sta.Start();
        return sta;
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static IPendingHandler ShowCoreAsync(Window? owner, string? message, string? title, bool isShowCancel, bool closeOnCanceled = true)
    {
        Point? center = null;

        try
        {
            owner?.Dispatcher.Invoke(() =>
            {
                RECT rect = new();
                _ = User32.GetWindowRect(new WindowInteropHelper(owner).Handle, ref rect);
                POINT p = new(rect.Left + (int)((rect.Right - rect.Left) / 2d), rect.Top + (int)((rect.Bottom - rect.Top) / 2d));
                center = new(p.X, p.Y);
            });
        }
        catch
        {
            ///
        }

        PendingBoxDialog dialog = new()
        {
            Title = title ?? string.Empty,
            Message = message ?? (SH.Loading + "..."),
            IsShowCancel = isShowCancel,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };
        PendingHandlerAsync pending = new(dialog);

        dialog.Closing += (_, e) =>
        {
            if (!e.Cancel)
            {
                _ = User32.SetForegroundWindow(new WindowInteropHelper(owner).Handle);
            }
        };
        dialog.Closed += (s, e) =>
        {
            pending.RaiseClosedEvent(s, e);
            pending.Close();
        };
        dialog.Canceled += (s, e) =>
        {
            pending.RaiseCanceledEvent(s, e);
            pending.Close();
        };

        if (center != null)
        {
            dialog.Loaded += (_, _) =>
            {
                try
                {
                    LayoutHelper.MoveWindowCenter(dialog, (Point)center);
                }
                catch
                {
                    ///
                }
            };
        }
        return pending;
    }
}

file static class LayoutHelper
{
    public static void MoveWindowCenter(Window window, Point center)
    {
        if (window == null)
        {
            return;
        }

        nint hwnd = new WindowInteropHelper(window).Handle;

        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        RECT rect = default;
        _ = User32.GetWindowRect(hwnd, ref rect);

        nint monitorHandle = User32.MonitorFromWindow(hwnd, User32.MonitorDefaultTo.Nearest);
        User32.MONITORINFO monitorInfo = new()
        {
            cbSize = Marshal.SizeOf<User32.MONITORINFO>()
        };
        _ = User32.GetMonitorInfo(monitorHandle, ref monitorInfo);
        RECT screen = monitorInfo.rcMonitor;

        (int x, int y) = ((int)center.X, (int)center.Y);
        (int w, int h) = (rect.Right - rect.Left, rect.Bottom - rect.Top);

        (x, y) = ((int)(x - w / 2d), (int)(y - h / 2d));

        if (x + w > screen.Right)
        {
            x = screen.Right - w;
        }
        else if (x - w < screen.Left)
        {
            x = screen.Left + w;
        }

        if (y + h > screen.Bottom)
        {
            y = screen.Bottom - h;
        }
        else if (y - h < screen.Top)
        {
            y = screen.Top + h;
        }

        _ = User32.MoveWindow(hwnd, x, y, w, h, false);
    }
}
