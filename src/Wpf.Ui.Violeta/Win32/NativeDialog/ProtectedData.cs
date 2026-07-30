using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Wpf.Ui.Violeta.Win32.NativeDialog;

internal unsafe static class ProtectedData
{
    public static byte[] Protect(byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
    {
        CheckPlatformSupport();
        _ = userData ?? throw new ArgumentNullException(nameof(userData));
        TryProtectOrUnprotect(userData, optionalEntropy, scope, true, out _, out byte[] result, true, default);
        return result;
    }

    public static byte[] Protect(ReadOnlySpan<byte> userData, DataProtectionScope scope, ReadOnlySpan<byte> optionalEntropy = default)
    {
        CheckPlatformSupport();
        TryProtectOrUnprotect(userData, optionalEntropy, scope, true, out _, out byte[] result, true, default);
        return result;
    }

    public static bool TryProtect(ReadOnlySpan<byte> userData, DataProtectionScope scope, Span<byte> destination, out int bytesWritten, ReadOnlySpan<byte> optionalEntropy = default)
    {
        CheckPlatformSupport();
        return TryProtectOrUnprotect(userData, optionalEntropy, scope, true, out bytesWritten, out _, false, destination);
    }

    public static int Protect(ReadOnlySpan<byte> userData, DataProtectionScope scope, Span<byte> destination, ReadOnlySpan<byte> optionalEntropy = default)
    {
        CheckPlatformSupport();
        if (!TryProtectOrUnprotect(userData, optionalEntropy, scope, true, out int result, out _, false, destination))
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
        return result;
    }

    public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
    {
        CheckPlatformSupport();
        _ = encryptedData ?? throw new ArgumentNullException(nameof(encryptedData));
        TryProtectOrUnprotect(encryptedData, optionalEntropy, scope, false, out _, out byte[] result, true, default);
        return result;
    }

    public static byte[] Unprotect(ReadOnlySpan<byte> encryptedData, DataProtectionScope scope, ReadOnlySpan<byte> optionalEntropy = default)
    {
        CheckPlatformSupport();
        TryProtectOrUnprotect(encryptedData, optionalEntropy, scope, false, out _, out byte[] result, true, default);
        return result;
    }

    public static bool TryUnprotect(ReadOnlySpan<byte> encryptedData, DataProtectionScope scope, Span<byte> destination, out int bytesWritten, ReadOnlySpan<byte> optionalEntropy = default)
    {
        CheckPlatformSupport();
        return TryProtectOrUnprotect(encryptedData, optionalEntropy, scope, false, out bytesWritten, out _, false, destination);
    }

    public static int Unprotect(ReadOnlySpan<byte> encryptedData, DataProtectionScope scope, Span<byte> destination, ReadOnlySpan<byte> optionalEntropy = default)
    {
        CheckPlatformSupport();
        if (!TryProtectOrUnprotect(encryptedData, optionalEntropy, scope, false, out int result, out _, false, destination))
        {
            throw new ArgumentException("Destination is too short.", nameof(destination));
        }
        return result;
    }

    private unsafe static bool TryProtectOrUnprotect(ReadOnlySpan<byte> inputData, ReadOnlySpan<byte> optionalEntropy, DataProtectionScope scope, bool protect, out int bytesWritten, out byte[] outputData, bool allocateArray, Span<byte> outputSpan = default)
    {
        fixed (byte* pinnableReference = inputData.IsEmpty ? s_nonEmpty : inputData.ToArray())
        {
            byte* handle = pinnableReference;
            fixed (byte* pinnableReference2 = optionalEntropy.ToArray())
            {
                byte* handle2 = pinnableReference2;
                Crypt32.DATA_BLOB data_BLOB = new((nint)handle, (uint)inputData.Length);
                Crypt32.DATA_BLOB data_BLOB2 = default;
                if (!optionalEntropy.IsEmpty)
                {
                    data_BLOB2 = new Crypt32.DATA_BLOB((nint)handle2, (uint)optionalEntropy.Length);
                }
                Crypt32.CryptProtectDataFlags cryptProtectDataFlags = Crypt32.CryptProtectDataFlags.CRYPTPROTECT_UI_FORBIDDEN;
                if (scope == DataProtectionScope.LocalMachine)
                {
                    cryptProtectDataFlags |= Crypt32.CryptProtectDataFlags.CRYPTPROTECT_LOCAL_MACHINE;
                }
                Crypt32.DATA_BLOB data_BLOB3 = default;
                Span<byte> span = default;
                bool result;
                try
                {
                    if (!(protect ? Crypt32.CryptProtectData(data_BLOB, null!, ref data_BLOB2, IntPtr.Zero, IntPtr.Zero, cryptProtectDataFlags, out data_BLOB3) : Crypt32.CryptUnprotectData(data_BLOB, IntPtr.Zero, ref data_BLOB2, IntPtr.Zero, IntPtr.Zero, cryptProtectDataFlags, out data_BLOB3)))
                    {
                        int lastPInvokeError = Marshal.GetLastWin32Error();
                        if (protect && ErrorMayBeCausedByUnloadedProfile(lastPInvokeError))
                        {
                            throw new CryptographicException("The data protection operation was unsuccessful. This may have been caused by not having the user profile loaded for the current thread's user context, which may be the case when the thread is impersonating.");
                        }
                        throw new CryptographicException(Kernel32.GetMessage(lastPInvokeError));
                    }
                    else
                    {
                        if (data_BLOB3.pbData == IntPtr.Zero)
                        {
                            throw new OutOfMemoryException();
                        }
                        int cbData = (int)data_BLOB3.cbData;
                        span = new Span<byte>((byte*)data_BLOB3.pbData, cbData);
                        if (allocateArray)
                        {
                            outputData = span.ToArray();
                            bytesWritten = cbData;
                            result = true;
                        }
                        else if (data_BLOB3.cbData > (ulong)((long)outputSpan.Length))
                        {
                            bytesWritten = 0;
                            outputData = null!;
                            result = false;
                        }
                        else
                        {
                            span.CopyTo(outputSpan);
                            bytesWritten = cbData;
                            outputData = null!;
                            result = true;
                        }
                    }
                }
                finally
                {
                    if (data_BLOB3.pbData != IntPtr.Zero)
                    {
                        span.Clear();
                        Marshal.FreeHGlobal(data_BLOB3.pbData);
                    }
                }
                return result;
            }
        }
    }

    private static bool ErrorMayBeCausedByUnloadedProfile(int errorCode)
    {
        return errorCode == -2147024894 || errorCode == 2;
    }

    private static void CheckPlatformSupport()
    {
        // Pass
    }

    private static readonly byte[] s_nonEmpty = new byte[1];
}

file unsafe static class Crypt32
{
    [return: MarshalAs(UnmanagedType.Bool)]
    internal unsafe static bool CryptProtectData(in DATA_BLOB pDataIn, string szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, Crypt32.CryptProtectDataFlags dwFlags, out Crypt32.DATA_BLOB pDataOut)
    {
        pDataOut = default;
        int num;
        int lastSystemError;
        fixed (DATA_BLOB* ptr = &pDataOut)
        {
            DATA_BLOB* _pDataOut_native = ptr;
            fixed (DATA_BLOB* ptr2 = &pOptionalEntropy)
            {
                DATA_BLOB* _pOptionalEntropy_native = ptr2;
                fixed (char* pinnableReference = szDataDescr)
                {
                    void* _szDataDescr_native = pinnableReference;
                    fixed (DATA_BLOB* ptr3 = &pDataIn)
                    {
                        DATA_BLOB* _pDataIn_native = ptr3;
                        Kernel32.SetLastError(0);
                        num = CryptProtectData(_pDataIn_native, (ushort*)_szDataDescr_native, _pOptionalEntropy_native, pvReserved, pPromptStruct, dwFlags, _pDataOut_native);
                        lastSystemError = Marshal.GetLastWin32Error();
                    }
                }
            }
        }
        bool result = num != 0;
        Kernel32.SetLastError((uint)lastSystemError);
        return result;
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    internal static bool CryptUnprotectData(in DATA_BLOB pDataIn, nint ppszDataDescr, ref DATA_BLOB pOptionalEntropy, nint pvReserved, nint pPromptStruct, CryptProtectDataFlags dwFlags, out DATA_BLOB pDataOut)
    {
        pDataOut = default;
        int num;
        int lastSystemError;
        fixed (DATA_BLOB* ptr = &pDataOut)
        {
            DATA_BLOB* _pDataOut_native = ptr;
            fixed (DATA_BLOB* ptr2 = &pOptionalEntropy)
            {
                DATA_BLOB* _pOptionalEntropy_native = ptr2;
                fixed (DATA_BLOB* ptr3 = &pDataIn)
                {
                    DATA_BLOB* _pDataIn_native = ptr3;
                    Kernel32.SetLastError(0);
                    num = CryptUnprotectData(_pDataIn_native, ppszDataDescr, _pOptionalEntropy_native, pvReserved, pPromptStruct, dwFlags, _pDataOut_native);
                    lastSystemError = Marshal.GetLastWin32Error();
                }
            }
        }
        bool result = num != 0;
        Kernel32.SetLastError((uint)lastSystemError);
        return result;
    }

    [DllImport("crypt32.dll", EntryPoint = "CryptProtectData", ExactSpelling = true)]
    internal extern unsafe static int CryptProtectData(DATA_BLOB* __pDataIn_native, ushort* __szDataDescr_native, DATA_BLOB* __pOptionalEntropy_native, nint __pvReserved_native, nint __pPromptStruct_native, CryptProtectDataFlags __dwFlags_native, DATA_BLOB* __pDataOut_native);

    [DllImport("crypt32.dll", EntryPoint = "CryptUnprotectData", ExactSpelling = true)]
    internal extern unsafe static int CryptUnprotectData(DATA_BLOB* __pDataIn_native, nint __ppszDataDescr_native, DATA_BLOB* __pOptionalEntropy_native, nint __pvReserved_native, nint __pPromptStruct_native, CryptProtectDataFlags __dwFlags_native, DATA_BLOB* __pDataOut_native);

    [Flags]
    internal enum CryptProtectDataFlags
    {
        CRYPTPROTECT_UI_FORBIDDEN = 1,

        CRYPTPROTECT_LOCAL_MACHINE = 4,

        CRYPTPROTECT_CRED_SYNC = 8,

        CRYPTPROTECT_AUDIT = 16,

        CRYPTPROTECT_NO_RECOVERY = 32,

        CRYPTPROTECT_VERIFY_PROTECTION = 64
    }

    internal struct DATA_BLOB
    {
        internal DATA_BLOB(nint handle, uint size)
        {
            cbData = size;
            pbData = handle;
        }

        internal readonly byte[] ToByteArray()
        {
            if (cbData == 0U)
            {
                return [];
            }
            byte[] array = new byte[cbData];
            Marshal.Copy(pbData, array, 0, (int)cbData);
            return array;
        }

        internal readonly ReadOnlySpan<byte> DangerousAsSpan()
        {
            return new ReadOnlySpan<byte>((byte*)pbData, (int)cbData);
        }

        internal uint cbData;

        internal nint pbData;
    }
}

file static class Kernel32
{
    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;
    private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int FormatMessage(
        uint dwFlags,
        nint lpSource,
        int dwMessageId,
        uint dwLanguageId,
        StringBuilder lpBuffer,
        int nSize,
        nint arguments);

    public static string GetMessage(int errorCode)
    {
        var buffer = new StringBuilder(1024);

        int length = FormatMessage(
            FORMAT_MESSAGE_FROM_SYSTEM |
            FORMAT_MESSAGE_IGNORE_INSERTS,
            IntPtr.Zero,
            errorCode,
            0,
            buffer,
            buffer.Capacity,
            IntPtr.Zero);

        if (length == 0)
        {
            return $"Unknown error 0x{errorCode:X8}";
        }

        return buffer.ToString().Trim();
    }

    [DllImport("kernel32.dll")]
    public static extern void SetLastError(uint dwErrCode);
}

public enum DataProtectionScope
{
    CurrentUser,
    LocalMachine,
}
