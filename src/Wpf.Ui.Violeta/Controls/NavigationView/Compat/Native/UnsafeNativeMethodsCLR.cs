using System;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal partial class UnsafeNativeMethods
{
    [DllImport("user32.dll", EntryPoint = "SetWindowPos", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

    [DllImport("user32.dll", EntryPoint = "ClientToScreen", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
    private static extern int IntClientToScreen(HandleRef hWnd, [In, Out] POINT pt);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern IntPtr GetActiveWindow();
}
