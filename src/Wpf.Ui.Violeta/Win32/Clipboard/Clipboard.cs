using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// This class is designed to avoid the "Cannot open clipboard" error when copying text to the clipboard. (CLIPBRD_E_CANT_OPEN)
/// The source can be found here: https://zhuanlan.zhihu.com/p/388316834 | https://stackoverflow.com/questions/5707996/clipboard-operations-throwing-exception
/// </summary>
[SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible")]
public static class Clipboard
{
    /// <summary>
    /// Set text to clipboard
    /// </summary>
    /// <param name="text">Text to set</param>
    public static void SetText(string text)
    {
        if (!OpenClipboard(IntPtr.Zero))
        {
            SetText(text);
            return;
        }
        EmptyClipboard();
        SetClipboardData((int)ClipboardFormat.CF_UNICODETEXT, Marshal.StringToHGlobalUni(text));
        CloseClipboard();
    }

    public static string GetText(int format)
    {
        string? value = string.Empty;

        OpenClipboard(IntPtr.Zero);
        if (IsClipboardFormatAvailable(format))
        {
            nint ptr = GetClipboardData(format);

            if (ptr != IntPtr.Zero)
            {
                value = Marshal.PtrToStringUni(ptr) ?? string.Empty;
            }
        }
        CloseClipboard();
        return value;
    }

    [DllImport("User32")]
    public static extern bool OpenClipboard(nint hWndNewOwner);

    [DllImport("User32")]
    public static extern bool CloseClipboard();

    [DllImport("User32")]
    public static extern bool EmptyClipboard();

    [DllImport("User32")]
    public static extern bool IsClipboardFormatAvailable(int format);

    [DllImport("User32")]
    public static extern nint GetClipboardData(int uFormat);

    [DllImport("User32", CharSet = CharSet.Unicode)]
    public static extern nint SetClipboardData(int uFormat, nint hMem);
}
