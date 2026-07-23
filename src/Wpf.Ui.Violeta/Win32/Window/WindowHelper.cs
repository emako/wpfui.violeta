using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace Wpf.Ui.Violeta.Win32;

public static class WindowHelper
{
    public static void ShowWithoutTransition(this Window window)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandleSafe();
        DwmApi.SetTransitionsForceDisabled(hwnd, disabled: true);
        window.Show();
        DwmApi.SetTransitionsForceDisabled(hwnd, disabled: false);
    }

    public static void BringToFront(this Window window, bool keep)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandleSafe();
        keep |= window.Topmost;

        User32.SetWindowPos(hwnd, User32.HWND_TOPMOST, 0, 0, 0, 0,
            User32.SET_WINDOW_POS_FLAGS.SWP_NOMOVE | User32.SET_WINDOW_POS_FLAGS.SWP_NOSIZE | User32.SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);

        if (!keep)
            User32.SetWindowPos(hwnd, User32.HWND_NOTOPMOST, 0, 0, 0, 0,
                User32.SET_WINDOW_POS_FLAGS.SWP_NOMOVE | User32.SET_WINDOW_POS_FLAGS.SWP_NOSIZE | User32.SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    }

    public static void SetNoactivate(this Window window)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandleSafe();
        _ = User32.SetWindowLong(hwnd, User32.GWL_EXSTYLE,
            User32.GetWindowLong(hwnd, User32.GWL_EXSTYLE) |
            User32.WS_EX_NOACTIVATE);
    }

    public static Rect GetWindowRectInPixel(this Window window)
    {
        var handle = new WindowInteropHelper(window).EnsureHandleSafe();

        RECT nRect = default;
        User32.GetWindowRect(handle, ref nRect);

        return new Rect(new Point(nRect.Left, nRect.Top), new Point(nRect.Right, nRect.Bottom));
    }
}

file static class WindowInteropHelperExtension
{
    extension(WindowInteropHelper windowInteropHelper)
    {
        public nint EnsureHandleSafe()
        {
            try
            {
                return windowInteropHelper?.EnsureHandle() ?? IntPtr.Zero;
            }
            catch (Exception e)
            {
                // Returning 0 is fine, since this error usually only occurs when the window is already closed or being disposed.
                Debug.WriteLine(e.ToString());
                return IntPtr.Zero;
            }
        }
    }
}
