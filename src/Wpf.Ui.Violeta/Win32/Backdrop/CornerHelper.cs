using System;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

public static class CornerHelper
{
    public static bool SetWindowCorner(nint hwnd, WindowCornerPreference style)
    {
        if (hwnd == IntPtr.Zero)
            return false;

        if (Environment.OSVersion.Version < new Version(10, 0, 22000))
            return false;

        int cornerPreference = (int)style;
        return DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, Marshal.SizeOf<int>()) == HRESULT.S_OK;
    }
}
