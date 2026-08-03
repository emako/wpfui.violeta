using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

namespace Wpf.Ui.Violeta.Win32;

internal static class User32
{
    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;
    public const int WS_SYSMENU = 0x00080000;
    public const int WS_MINIMIZEBOX = 0x00020000;
    public const int WS_MAXIMIZEBOX = 0x00010000;
    public const int WS_EX_NOACTIVATE = 0x08000000;

    public const nint HWND_TOPMOST = -1;
    public const nint HWND_NOTOPMOST = -2;
    public const nint HWND_TOP = 0;
    public const nint HWND_BOTTOM = 1;

    [DllImport("user32.dll")]
    public static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

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

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CreateIconFromResourceEx(
        ref byte pbIconBits,
        uint cbIconBits,
        bool fIcon,
        uint dwVersion,
        int cxDesired,
        int cyDesired,
        uint uFlags);

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

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool MoveWindow(nint hWnd, int x, int y, int width, int height, bool repaint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(nint hwnd, ref RECT rect);

    [DllImport("user32.dll")]
    public static extern nint MonitorFromWindow(nint hwnd, MonitorDefaultTo dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CopyIcon(nint hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetIconInfo(nint hIcon, out ICONINFO piconinfo);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CreatePopupMenu();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyMenu(nint hMenu);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool AppendMenu(nint hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool AppendMenu(nint hMenu, uint uFlags, nint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SetMenuItemInfo(nint hMenu, uint uItem, bool fByPosition, ref MENUITEMINFO lpmii);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool GetMenuItemInfo(nint hMenu, uint uItem, bool fByPosition, ref MENUITEMINFO lpmii);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint TrackPopupMenuEx(nint hMenu, uint uFlags, int x, int y, nint hWnd, nint lpTPMParams);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern nint CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport("user32.dll")]
    public static extern nint DefWindowProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern uint RegisterWindowMessage(string lpString);

    public delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    public static extern int SetWindowCompositionAttribute(nint hwnd,
        ref WindowCompositionAttributeData data);

    [DllImport("user32.dll")]
    public static extern bool GetWindowPlacement(nint hWnd, ref WINDOWPLACEMENT lpwndpl);

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
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
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

    public enum MonitorDefaultTo : uint
    {
        Null = 0,
        Primary = 1,
        Nearest = 2,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASS
    {
        public uint style;
        public nint lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MENUITEMINFO
    {
        public uint cbSize;
        public uint fMask;
        public uint fType;
        public uint fState;
        public uint wID;
        public nint hSubMenu;
        public nint hbmpChecked;
        public nint hbmpUnchecked;
        public nint dwItemData;
        public string dwTypeData;
        public uint cch;
        public nint hbmpItem;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool fIcon;

        public uint xHotspot;
        public uint yHotspot;
        public nint hbmMask;
        public nint hbmColor;
    }

    /// <summary>Contains information about the placement of a window on the screen.</summary>
    /// <remarks>
    /// <para>If the window is a top-level window that does not have the <b>WS_EX_TOOLWINDOW</b> window style, then the coordinates represented by the following members are in workspace coordinates: <b>ptMinPosition</b>, <b>ptMaxPosition</b>, and <b>rcNormalPosition</b>. Otherwise, these members are in screen coordinates. Workspace coordinates differ from screen coordinates in that they take the locations and sizes of application toolbars (including the taskbar) into account. Workspace coordinate (0,0) is the upper-left corner of the workspace area, the area of the screen not being used by application toolbars. The coordinates used in a <b>WINDOWPLACEMENT</b> structure should be used only by the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowplacement">GetWindowPlacement</a> and <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-setwindowplacement">SetWindowPlacement</a> functions. Passing workspace coordinates to functions which expect screen coordinates (such as <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-setwindowpos">SetWindowPos</a>) will result in the window appearing in the wrong location. For example, if the taskbar is at the top of the screen, saving window coordinates using <b>GetWindowPlacement</b> and restoring them using <b>SetWindowPos</b> causes the window to appear to "creep" up the screen.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-windowplacement#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    internal partial struct WINDOWPLACEMENT
    {
        /// <summary>
        /// <para>Type: <b>UINT</b> The length of the structure, in bytes. Before calling the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowplacement">GetWindowPlacement</a> or <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-setwindowplacement">SetWindowPlacement</a> functions, set this member to <c>sizeof(WINDOWPLACEMENT)</c>.</para>
        /// <para><a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-getwindowplacement">GetWindowPlacement</a> and <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-setwindowplacement">SetWindowPlacement</a> fail if this member is not set correctly.</para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-windowplacement#members">Read more on docs.microsoft.com</see>.</para>
        /// </summary>
        internal uint length;

        /// <summary>Type: <b>UINT</b></summary>
        internal WINDOWPLACEMENT_FLAGS flags;

        /// <summary>
        /// <para>Type: <b>UINT</b> The current show state of the window. It can be any of the values that can be specified in the <i>nCmdShow</i> parameter for the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-showwindow">ShowWindow</a> function.</para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-windowplacement#members">Read more on docs.microsoft.com</see>.</para>
        /// </summary>
        internal SHOW_WINDOW_CMD showCmd;

        /// <summary>
        /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The coordinates of the window's upper-left corner when the window is minimized.</para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-windowplacement#members">Read more on docs.microsoft.com</see>.</para>
        /// </summary>
        internal POINT ptMinPosition;

        /// <summary>
        /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The coordinates of the window's upper-left corner when the window is maximized.</para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-windowplacement#members">Read more on docs.microsoft.com</see>.</para>
        /// </summary>
        internal POINT ptMaxPosition;

        /// <summary>
        /// <para>Type: <b><a href="https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-rect">RECT</a></b> The window's coordinates when the window is in the restored position.</para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-windowplacement#members">Read more on docs.microsoft.com</see>.</para>
        /// </summary>
        internal RECT rcNormalPosition;
    }

    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
    public enum WindowMessage
    {
        WM_NULL = 0x0000,
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_MOVE = 0x0003,
        WM_SIZE = 0x0005,
        WM_ACTIVATE = 0x0006,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008,
        WM_ENABLE = 0x000A,
        WM_SETREDRAW = 0x000B,
        WM_SETTEXT = 0x000C,
        WM_GETTEXT = 0x000D,
        WM_GETTEXTLENGTH = 0x000E,
        WM_PAINT = 0x000F,
        WM_CLOSE = 0x0010,
        WM_QUERYENDSESSION = 0x0011,
        WM_QUIT = 0x0012,
        WM_QUERYOPEN = 0x0013,
        WM_ERASEBKGND = 0x0014,
        WM_SYSCOLORCHANGE = 0x0015,
        WM_ENDSESSION = 0x0016,
        WM_SHOWWINDOW = 0x0018,
        WM_CTLCOLOR = 0x0019,
        WM_WININICHANGE = 0x001A,
        WM_SETTINGCHANGE = 0x001A,
        WM_DEVMODECHANGE = 0x001B,
        WM_ACTIVATEAPP = 0x001C,
        WM_FONTCHANGE = 0x001D,
        WM_TIMECHANGE = 0x001E,
        WM_CANCELMODE = 0x001F,
        WM_SETCURSOR = 0x0020,
        WM_MOUSEACTIVATE = 0x0021,
        WM_CHILDACTIVATE = 0x0022,
        WM_QUEUESYNC = 0x0023,
        WM_GETMINMAXINFO = 0x0024,
        WM_PAINTICON = 0x0026,
        WM_ICONERASEBKGND = 0x0027,
        WM_NEXTDLGCTL = 0x0028,
        WM_SPOOLERSTATUS = 0x002A,
        WM_DRAWITEM = 0x002B,
        WM_MEASUREITEM = 0x002C,
        WM_DELETEITEM = 0x002D,
        WM_VKEYTOITEM = 0x002E,
        WM_CHARTOITEM = 0x002F,
        WM_SETFONT = 0x0030,
        WM_GETFONT = 0x0031,
        WM_SETHOTKEY = 0x0032,
        WM_GETHOTKEY = 0x0033,
        WM_QUERYDRAGICON = 0x0037,
        WM_COMPAREITEM = 0x0039,
        WM_GETOBJECT = 0x003D,
        WM_COMPACTING = 0x0041,
        WM_COMMNOTIFY = 0x0044,
        WM_WINDOWPOSCHANGING = 0x0046,
        WM_WINDOWPOSCHANGED = 0x0047,
        WM_POWER = 0x0048,
        WM_COPYDATA = 0x004A,
        WM_CANCELJOURNAL = 0x004B,
        WM_NOTIFY = 0x004E,
        WM_INPUTLANGCHANGEREQUEST = 0x0050,
        WM_INPUTLANGCHANGE = 0x0051,
        WM_TCARD = 0x0052,
        WM_HELP = 0x0053,
        WM_USERCHANGED = 0x0054,
        WM_NOTIFYFORMAT = 0x0055,
        WM_CONTEXTMENU = 0x007B,
        WM_STYLECHANGING = 0x007C,
        WM_STYLECHANGED = 0x007D,
        WM_DISPLAYCHANGE = 0x007E,
        WM_GETICON = 0x007F,
        WM_SETICON = 0x0080,
        WM_NCCREATE = 0x0081,
        WM_NCDESTROY = 0x0082,
        WM_NCCALCSIZE = 0x0083,
        WM_NCHITTEST = 0x0084,
        WM_NCPAINT = 0x0085,
        WM_NCACTIVATE = 0x0086,
        WM_GETDLGCODE = 0x0087,
        WM_SYNCPAINT = 0x0088,
        WM_NCMOUSEMOVE = 0x00A0,
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_NCLBUTTONUP = 0x00A2,
        WM_NCLBUTTONDBLCLK = 0x00A3,
        WM_NCRBUTTONDOWN = 0x00A4,
        WM_NCRBUTTONUP = 0x00A5,
        WM_NCRBUTTONDBLCLK = 0x00A6,
        WM_NCMBUTTONDOWN = 0x00A7,
        WM_NCMBUTTONUP = 0x00A8,
        WM_NCMBUTTONDBLCLK = 0x00A9,
        WM_NCMOUSELEAVE = 0x02A2,
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_CHAR = 0x0102,
        WM_DEADCHAR = 0x0103,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105,
        WM_SYSCHAR = 0x0106,
        WM_SYSDEADCHAR = 0x0107,
        WM_KEYLAST = 0x0108,
        WM_IME_STARTCOMPOSITION = 0x010D,
        WM_IME_ENDCOMPOSITION = 0x010E,
        WM_IME_COMPOSITION = 0x010F,
        WM_IME_KEYLAST = 0x010F,
        WM_INITDIALOG = 0x0110,
        WM_COMMAND = 0x0111,
        WM_SYSCOMMAND = 0x0112,
        WM_TIMER = 0x0113,
        WM_HSCROLL = 0x0114,
        WM_VSCROLL = 0x0115,
        WM_INITMENU = 0x0116,
        WM_INITMENUPOPUP = 0x0117,
        WM_MENUSELECT = 0x011F,
        WM_MENUCHAR = 0x0120,
        WM_ENTERIDLE = 0x0121,
        WM_MENURBUTTONUP = 0x0122,
        WM_MENUDRAG = 0x0123,
        WM_MENUGETOBJECT = 0x0124,
        WM_UNINITMENUPOPUP = 0x0125,
        WM_MENUCOMMAND = 0x0126,
        WM_CTLCOLORWinMsgBOX = 0x0132,
        WM_CTLCOLOREDIT = 0x0133,
        WM_CTLCOLORLISTBOX = 0x0134,
        WM_CTLCOLORBTN = 0x0135,
        WM_CTLCOLORDLG = 0x0136,
        WM_CTLCOLORSCROLLBAR = 0x0137,
        WM_CTLCOLORSTATIC = 0x0138,
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MBUTTONDBLCLK = 0x0209,
        WM_MOUSEWHEEL = 0x020A,
        WM_PARENTNOTIFY = 0x0210,
        WM_ENTERMENULOOP = 0x0211,
        WM_EXITMENULOOP = 0x0212,
        WM_NEXTMENU = 0x0213,
        WM_SIZING = 0x0214,
        WM_CAPTURECHANGED = 0x0215,
        WM_MOVING = 0x0216,
        WM_DEVICECHANGE = 0x0219,
        WM_MDICREATE = 0x0220,
        WM_MDIDESTROY = 0x0221,
        WM_MDIACTIVATE = 0x0222,
        WM_MDIRESTORE = 0x0223,
        WM_MDINEXT = 0x0224,
        WM_MDIMAXIMIZE = 0x0225,
        WM_MDITILE = 0x0226,
        WM_MDICASCADE = 0x0227,
        WM_MDIICONARRANGE = 0x0228,
        WM_MDIGETACTIVE = 0x0229,
        WM_MDISETMENU = 0x0230,
        WM_ENTERSIZEMOVE = 0x0231,
        WM_EXITSIZEMOVE = 0x0232,
        WM_DROPFILES = 0x0233,
        WM_MDIREFRESHMENU = 0x0234,
        WM_IME_SETCONTEXT = 0x0281,
        WM_IME_NOTIFY = 0x0282,
        WM_IME_CONTROL = 0x0283,
        WM_IME_COMPOSITIONFULL = 0x0284,
        WM_IME_SELECT = 0x0285,
        WM_IME_CHAR = 0x0286,
        WM_IME_REQUEST = 0x0288,
        WM_IME_KEYDOWN = 0x0290,
        WM_IME_KEYUP = 0x0291,
        WM_MOUSEHOVER = 0x02A1,
        WM_MOUSELEAVE = 0x02A3,
        WM_DPICHANGED = 0x02E0,
        WM_CUT = 0x0300,
        WM_COPY = 0x0301,
        WM_PASTE = 0x0302,
        WM_CLEAR = 0x0303,
        WM_UNDO = 0x0304,
        WM_RENDERFORMAT = 0x0305,
        WM_RENDERALLFORMATS = 0x0306,
        WM_DESTROYCLIPBOARD = 0x0307,
        WM_DRAWCLIPBOARD = 0x0308,
        WM_PAINTCLIPBOARD = 0x0309,
        WM_VSCROLLCLIPBOARD = 0x030A,
        WM_SIZECLIPBOARD = 0x030B,
        WM_ASKCBFORMATNAME = 0x030C,
        WM_CHANGECBCHAIN = 0x030D,
        WM_HSCROLLCLIPBOARD = 0x030E,
        WM_QUERYNEWPALETTE = 0x030F,
        WM_PALETTEISCHANGING = 0x0310,
        WM_PALETTECHANGED = 0x0311,
        WM_HOTKEY = 0x0312,
        WM_SYSMENU = 0x0313,
        WM_PRINT = 0x0317,
        WM_PRINTCLIENT = 0x0318,
        WM_THEMECHANGED = 0x031A,
        WM_HANDHELDFIRST = 0x0358,
        WM_HANDHELDLAST = 0x035F,
        WM_AFXFIRST = 0x0360,
        WM_AFXLAST = 0x037F,
        WM_PENWINFIRST = 0x0380,
        WM_PENWINLAST = 0x038F,
        WM_APP = 0x8000,
        WM_USER = 0x0400,
        WM_NOTIFYICON_BALLOONSHOW = 0x402,
        WM_NOTIFYICON_BALLOONHIDE = 0x403,
        WM_NOTIFYICON_BALLOONTIMEOUT = 0x404,
        WM_NOTIFYICON_BALLOONUSERCLICK = 0x405,
        WM_TRAYICON = WM_USER + 0x0001,
        WM_REFLECT = WM_USER + 0x1c00,
    }

    [Flags]
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
    public enum SHOW_WINDOW_CMD : uint
    {
        SW_FORCEMINIMIZE = 0x0000000B,
        SW_HIDE = 0x00000000,
        SW_MAXIMIZE = 0x00000003,
        SW_MINIMIZE = 0x00000006,
        SW_RESTORE = 0x00000009,
        SW_SHOW = 0x00000005,
        SW_SHOWDEFAULT = 0x0000000A,
        SW_SHOWMAXIMIZED = 0x00000003,
        SW_SHOWMINIMIZED = 0x00000002,
        SW_SHOWMINNOACTIVE = 0x00000007,
        SW_SHOWNA = 0x00000008,
        SW_SHOWNOACTIVATE = 0x00000004,
        SW_SHOWNORMAL = 0x00000001,
        SW_NORMAL = 0x00000001,
        SW_MAX = 0x0000000B,
        SW_PARENTCLOSING = 0x00000001,
        SW_OTHERZOOM = 0x00000002,
        SW_PARENTOPENING = 0x00000003,
        SW_OTHERUNZOOM = 0x00000004,
        SW_SCROLLCHILDREN = 0x00000001,
        SW_INVALIDATE = 0x00000002,
        SW_ERASE = 0x00000004,
        SW_SMOOTHSCROLL = 0x00000010,
    }

    [Flags]
    public enum WINDOWPLACEMENT_FLAGS : uint
    {
        WPF_ASYNCWINDOWPLACEMENT = 0x00000004,
        WPF_RESTORETOMAXIMIZED = 0x00000002,
        WPF_SETMINPOSITION = 0x00000001,
    }

    [Flags]
    public enum MenuFlags : uint
    {
        MF_STRING = 0x0000,
        MF_DISABLED = 0x0002,
        MF_CHECKED = 0x0008,
        MF_GRAYED = 0x0001,
        MF_POPUP = 0x0010,
        MF_SEPARATOR = 0x0800,
        MF_OWNERDRAW = 0x0100,
    }

    [Flags]
    public enum MenuItemMask : uint
    {
        MIIM_STATE = 0x00000001,
        MIIM_ID = 0x00000002,
        MIIM_SUBMENU = 0x00000004,
        MIIM_CHECKMARKS = 0x00000008,
        MIIM_TYPE = 0x00000010,
        MIIM_DATA = 0x00000020,
        MIIM_STRING = 0x00000040,
        MIIM_BITMAP = 0x00000080,
        MIIM_FTYPE = 0x00000100,
    }

    [Flags]
    public enum MenuItemType : uint
    {
        MFT_STRING = 0x00000000,
        MFT_BITMAP = 0x00000004,
        MFT_MENUBARBREAK = 0x00000020,
        MFT_MENUBREAK = 0x00000040,
        MFT_OWNERDRAW = 0x00000100,
        MFT_RADIOCHECK = 0x00000200,
        MFT_SEPARATOR = 0x00000800,
        MFT_RIGHTORDER = 0x00002000,
        MFT_RIGHTJUSTIFY = 0x00004000,
    }

    [Flags]
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
    public enum MenuItemState : uint
    {
        MFS_ENABLED = 0x00000000,
        MFS_UNCHECKED = 0x00000000,
        MFS_UNHILITE = 0x00000000,
        MFS_CHECKED = 0x00000008,
        MFS_DISABLED = 0x00000003,
        MFS_GRAYED = 0x00000003,
        MFS_HILITE = 0x00000080,
        MFS_DEFAULT = 0x00001000,
    }

    [Flags]
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
    public enum TrackPopupMenuFlags : uint
    {
        TPM_LEFTBUTTON = 0u,
        TPM_RIGHTBUTTON = 2u,
        TPM_LEFTALIGN = 0u,
        TPM_CENTERALIGN = 4u,
        TPM_RIGHTALIGN = 8u,
        TPM_TOPALIGN = 0u,
        TPM_VCENTERALIGN = 0x10u,
        TPM_BOTTOMALIGN = 0x20u,
        TPM_HORIZONTAL = 0u,
        TPM_VERTICAL = 0x40u,
        TPM_NONOTIFY = 0x80u,
        TPM_RETURNCMD = 0x100u,
        TPM_RECURSE = 1u,
        TPM_HORPOSANIMATION = 0x400u,
        TPM_HORNEGANIMATION = 0x800u,
        TPM_VERPOSANIMATION = 0x1000u,
        TPM_VERNEGANIMATION = 0x2000u,
        TPM_NOANIMATION = 0x4000u,
        TPM_LAYOUTRTL = 0x8000u,
        TPM_WORKAREA = 0x10000u,
    }

    public enum MONITOR_FROM_FLAGS : uint
    {
        MONITOR_DEFAULTTONEAREST = 2U,
        MONITOR_DEFAULTTONULL = 0U,
        MONITOR_DEFAULTTOPRIMARY = 1U,
    }

    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
    public enum SYSTEM_METRICS_INDEX : uint
    {
        SM_ARRANGE = 56U,
        SM_CLEANBOOT = 67U,
        SM_CMONITORS = 80U,
        SM_CMOUSEBUTTONS = 43U,
        SM_CONVERTIBLESLATEMODE = 8195U,
        SM_CXBORDER = 5U,
        SM_CXCURSOR = 13U,
        SM_CXDLGFRAME = 7U,
        SM_CXDOUBLECLK = 36U,
        SM_CXDRAG = 68U,
        SM_CXEDGE = 45U,
        SM_CXFIXEDFRAME = 7U,
        SM_CXFOCUSBORDER = 83U,
        SM_CXFRAME = 32U,
        SM_CXFULLSCREEN = 16U,
        SM_CXHSCROLL = 21U,
        SM_CXHTHUMB = 10U,
        SM_CXICON = 11U,
        SM_CXICONSPACING = 38U,
        SM_CXMAXIMIZED = 61U,
        SM_CXMAXTRACK = 59U,
        SM_CXMENUCHECK = 71U,
        SM_CXMENUSIZE = 54U,
        SM_CXMIN = 28U,
        SM_CXMINIMIZED = 57U,
        SM_CXMINSPACING = 47U,
        SM_CXMINTRACK = 34U,
        SM_CXPADDEDBORDER = 92U,
        SM_CXSCREEN = 0U,
        SM_CXSIZE = 30U,
        SM_CXSIZEFRAME = 32U,
        SM_CXSMICON = 49U,
        SM_CXSMSIZE = 52U,
        SM_CXVIRTUALSCREEN = 78U,
        SM_CXVSCROLL = 2U,
        SM_CYBORDER = 6U,
        SM_CYCAPTION = 4U,
        SM_CYCURSOR = 14U,
        SM_CYDLGFRAME = 8U,
        SM_CYDOUBLECLK = 37U,
        SM_CYDRAG = 69U,
        SM_CYEDGE = 46U,
        SM_CYFIXEDFRAME = 8U,
        SM_CYFOCUSBORDER = 84U,
        SM_CYFRAME = 33U,
        SM_CYFULLSCREEN = 17U,
        SM_CYHSCROLL = 3U,
        SM_CYICON = 12U,
        SM_CYICONSPACING = 39U,
        SM_CYKANJIWINDOW = 18U,
        SM_CYMAXIMIZED = 62U,
        SM_CYMAXTRACK = 60U,
        SM_CYMENU = 15U,
        SM_CYMENUCHECK = 72U,
        SM_CYMENUSIZE = 55U,
        SM_CYMIN = 29U,
        SM_CYMINIMIZED = 58U,
        SM_CYMINSPACING = 48U,
        SM_CYMINTRACK = 35U,
        SM_CYSCREEN = 1U,
        SM_CYSIZE = 31U,
        SM_CYSIZEFRAME = 33U,
        SM_CYSMCAPTION = 51U,
        SM_CYSMICON = 50U,
        SM_CYSMSIZE = 53U,
        SM_CYVIRTUALSCREEN = 79U,
        SM_CYVSCROLL = 20U,
        SM_CYVTHUMB = 9U,
        SM_DBCSENABLED = 42U,
        SM_DEBUG = 22U,
        SM_DIGITIZER = 94U,
        SM_IMMENABLED = 82U,
        SM_MAXIMUMTOUCHES = 95U,
        SM_MEDIACENTER = 87U,
        SM_MENUDROPALIGNMENT = 40U,
        SM_MIDEASTENABLED = 74U,
        SM_MOUSEPRESENT = 19U,
        SM_MOUSEHORIZONTALWHEELPRESENT = 91U,
        SM_MOUSEWHEELPRESENT = 75U,
        SM_NETWORK = 63U,
        SM_PENWINDOWS = 41U,
        SM_REMOTECONTROL = 8193U,
        SM_REMOTESESSION = 4096U,
        SM_SAMEDISPLAYFORMAT = 81U,
        SM_SECURE = 44U,
        SM_SERVERR2 = 89U,
        SM_SHOWSOUNDS = 70U,
        SM_SHUTTINGDOWN = 8192U,
        SM_SLOWMACHINE = 73U,
        SM_STARTER = 88U,
        SM_SWAPBUTTON = 23U,
        SM_SYSTEMDOCKED = 8196U,
        SM_TABLETPC = 86U,
        SM_XVIRTUALSCREEN = 76U,
        SM_YVIRTUALSCREEN = 77U,
    }
}
