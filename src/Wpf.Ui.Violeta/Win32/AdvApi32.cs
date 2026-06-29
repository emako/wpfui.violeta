using System;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

internal static class AdvApi32
{
    public static readonly IntPtr HKEY_CURRENT_USER = new(unchecked((int)0x80000001));

    public const uint RRF_RT_REG_DWORD = 0x00000010;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int RegGetValue(nint hKey, string lpSubKey, string lpValue, uint dwFlags, nint pdwType, out uint pvData, ref uint pcbData);
}
