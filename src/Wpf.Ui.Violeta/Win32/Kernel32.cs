using System;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

internal static class Kernel32
{
    private const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
    private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

    [DllImport("kernel32.dll")]
    public static extern int GetLastError();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint FormatMessage(
        uint dwFlags,
        nint lpSource,
        uint dwMessageId,
        uint dwLanguageId,
        nint lpBuffer,
        uint nSize,
        nint Arguments
    );

    public static string GetErrorMessage(int errorCode)
    {
        if (errorCode == 0)
            return string.Empty;

        uint bufSize = 512;
        nint buffer = Marshal.AllocHGlobal((int)bufSize);

        try
        {
            uint ret = FormatMessage(
                FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                IntPtr.Zero,
                (uint)errorCode,
                0, // 0 = Use the system's default language
                buffer,
                bufSize,
                IntPtr.Zero
            );

            if (ret == 0)
                return $"Unknown error code:{errorCode}";

            string? msg = Marshal.PtrToStringUni(buffer);
            // Remove trailing newline and carriage return
            return msg?.TrimEnd('\r', '\n') ?? $"Unknown error code:{errorCode}";
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
