using Microsoft.Win32.SafeHandles;
using System;
using System.IO;

namespace Wpf.Ui.Violeta.Win32;

public class Win32Icon : IDisposable
{
    private readonly SafeHIconHandle _handle;

    public SafeHIconHandle SafeHandle => _handle;

    public nint Handle => TryGetHandle(out nint handle) ? handle : IntPtr.Zero;

    public Win32Icon(Stream stream)
    {
        _ = stream ?? throw new ArgumentNullException(nameof(stream));

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        byte[] bytes = ms.ToArray();

        nint hIcon = CreateIconHandleFromBytes(bytes);
        _handle = new SafeHIconHandle(hIcon);
    }

    public Win32Icon(byte[] iconBytes)
    {
        _ = iconBytes ?? throw new ArgumentNullException(nameof(iconBytes));
        if (iconBytes.Length == 0)
            throw new ArgumentException("Icon bytes cannot be empty.", nameof(iconBytes));

        byte[] bytes = (byte[])iconBytes.Clone();
        nint hIcon = CreateIconHandleFromBytes(bytes);
        _handle = new SafeHIconHandle(hIcon);
    }

    private static nint CreateIconHandleFromBytes(byte[] bytes)
    {
        if (bytes.Length == 0)
            throw new InvalidDataException("Icon stream is empty.");

        if (bytes.Length < 6)
            throw new InvalidDataException("Stream too short for ICONDIR.");

        ushort reserved = BitConverter.ToUInt16(bytes, 0);
        ushort type = BitConverter.ToUInt16(bytes, 2);
        ushort count = BitConverter.ToUInt16(bytes, 4);

        if (reserved != 0 || type != 1 || count == 0)
            throw new InvalidDataException("Invalid ICO header.");

        int entryOffset = 6;
        if (bytes.Length < entryOffset + 16)
            throw new InvalidDataException("Stream too short for ICONDIRENTRY.");

        uint imageSize = BitConverter.ToUInt32(bytes, entryOffset + 8);
        uint imageOffset = BitConverter.ToUInt32(bytes, entryOffset + 12);

        if (imageOffset + imageSize > bytes.Length)
            throw new InvalidDataException("Icon image out of bounds.");

        nint hIcon = User32.CreateIconFromResourceEx(
            ref bytes[imageOffset],
            imageSize,
            true,
            0x00030000,
            0,
            0,
            0);

        if (hIcon == IntPtr.Zero)
            throw new InvalidOperationException("CreateIconFromResourceEx failed.");

        return hIcon;
    }

    internal bool TryCreateMenuBitmap(out nint hBitmap, out bool shouldDisposeBitmap)
    {
        hBitmap = IntPtr.Zero;
        shouldDisposeBitmap = false;

        if (!TryGetHandle(out nint hIcon))
            return false;

        if (!User32.GetIconInfo(hIcon, out User32.ICONINFO iconInfo))
            return false;

        nint selectedBitmap = iconInfo.hbmColor != IntPtr.Zero ? iconInfo.hbmColor : iconInfo.hbmMask;
        if (selectedBitmap == IntPtr.Zero)
            return false;

        nint unusedBitmap = selectedBitmap == iconInfo.hbmColor ? iconInfo.hbmMask : iconInfo.hbmColor;
        if (unusedBitmap != IntPtr.Zero)
            _ = Gdi32.DeleteObject(unusedBitmap);

        hBitmap = selectedBitmap;
        shouldDisposeBitmap = true;
        return true;
    }

    private bool TryGetHandle(out nint handle)
    {
        handle = IntPtr.Zero;

        if (_handle.IsClosed || _handle.IsInvalid)
            return false;

        handle = _handle.DangerousGetHandle();
        return handle != IntPtr.Zero;
    }

    public void Dispose()
    {
        _handle.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class SafeHIconHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeHIconHandle()
        : base(true)
    {
    }

    public SafeHIconHandle(nint preexistingHandle, bool ownsHandle = true)
        : base(ownsHandle)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        return User32.DestroyIcon(handle) != 0;
    }
}
