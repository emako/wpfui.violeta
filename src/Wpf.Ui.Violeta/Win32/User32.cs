using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Wpf.Ui.Violeta.Win32;

internal static class User32
{
    public const int GWL_STYLE = -16;
    public const int WS_SYSMENU = 0x00080000;

    [SecurityCritical]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern nint GetDC(nint hWnd);

    [SecurityCritical]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport("user32.dll", SetLastError = false, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool BringWindowToTop(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint MB_GetString(uint wBtn);

    [DllImport("user32.dll")]
    public static extern int DestroyIcon(nint hIcon);

    [DllImport("user32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern nint GetActiveWindow();

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, SET_WINDOW_POS_FLAGS uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool EnableWindow(nint hWnd, bool bEnable);

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

    [Flags]
    internal enum SET_WINDOW_POS_FLAGS : uint
    {
        SWP_ASYNCWINDOWPOS = 0x00004000,
        SWP_DEFERERASE = 0x00002000,
        SWP_DRAWFRAME = 0x00000020,
        SWP_FRAMECHANGED = 0x00000020,
        SWP_HIDEWINDOW = 0x00000080,
        SWP_NOACTIVATE = 0x00000010,
        SWP_NOCOPYBITS = 0x00000100,
        SWP_NOMOVE = 0x00000002,
        SWP_NOOWNERZORDER = 0x00000200,
        SWP_NOREDRAW = 0x00000008,
        SWP_NOREPOSITION = 0x00000200,
        SWP_NOSENDCHANGING = 0x00000400,
        SWP_NOSIZE = 0x00000001,
        SWP_NOZORDER = 0x00000004,
        SWP_SHOWWINDOW = 0x00000040,
    }
}
