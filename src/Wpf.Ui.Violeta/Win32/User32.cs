using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Wpf.Ui.Violeta.Win32;

internal static class User32
{
    [SecurityCritical]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern nint GetDC(nint hWnd);

    [SecurityCritical]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport("gdi32.dll", SetLastError = false, ExactSpelling = true)]
    public static extern int GetDeviceCaps(nint hdc, DeviceCap nIndex);

    [DllImport("user32.dll", SetLastError = false, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool BringWindowToTop(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint MB_GetString(uint wBtn);

    [Flags]
    public enum DialogBoxCommand : uint
    {
        IDOK = 0,
        IDCANCEL = 1,
        IDABORT = 2,
        IDRETRY = 3,
        IDIGNORE = 4,
        IDYES = 5,
        IDNO = 6,
        IDCLOSE = 7,
        IDHELP = 8,
        IDTRYAGAIN = 9,
        IDCONTINUE = 10,
    }

    public enum DeviceCap
    {
        LOGPIXELSX = 88,
        LOGPIXELSY = 90,
    }
}
