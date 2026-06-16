using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Wpf.Ui.Violeta.Win32;

public class Win32Image : IDisposable
{
    private readonly byte[] _imageBytes;
    private readonly SafeHBitmapHandle _handle;

    public SafeHBitmapHandle SafeHandle => _handle;

    public nint Handle => TryGetHandle(out nint handle) ? handle : IntPtr.Zero;

    public bool ShowAsMonochrome { get; set; } = false;

    public TrayThemeMode ThemeMode { get; set; } = TrayThemeMode.System;

    public Win32Image(Stream stream)
    {
        _ = stream ?? throw new ArgumentNullException(nameof(stream));

        using MemoryStream ms = new();
        stream.CopyTo(ms);
        _imageBytes = ms.ToArray();

        nint hBitmap = CreateHBitmapFromBytes(_imageBytes);
        _handle = new SafeHBitmapHandle(hBitmap);
    }

    public Win32Image(byte[] imageBytes)
    {
        _ = imageBytes ?? throw new ArgumentNullException(nameof(imageBytes));
        if (imageBytes.Length == 0)
            throw new ArgumentException("Image bytes cannot be empty.", nameof(imageBytes));

        _imageBytes = (byte[])imageBytes.Clone();
        nint hBitmap = CreateHBitmapFromBytes(_imageBytes);
        _handle = new SafeHBitmapHandle(hBitmap);
    }

    private static nint CreateHBitmapFromBytes(byte[] imageBytes)
    {
        if (imageBytes.Length == 0)
            throw new InvalidDataException("Image stream is empty.");

        GdiPlus.EnsureInitialized();

        IStream? imageStream = null;
        nint gpBitmap = IntPtr.Zero;

        try
        {
            int createStreamResult = Ole32.CreateStreamOnHGlobal(IntPtr.Zero, true, out imageStream);
            if (createStreamResult != 0 || imageStream is null)
                throw new InvalidOperationException($"CreateStreamOnHGlobal failed with HRESULT 0x{createStreamResult:X8}.");

            imageStream.Write(imageBytes, imageBytes.Length, IntPtr.Zero);
            imageStream.Seek(0, 0, IntPtr.Zero);

            int createBitmapResult = GdiPlus.GdipCreateBitmapFromStream(imageStream, out gpBitmap);
            if (createBitmapResult != 0 || gpBitmap == IntPtr.Zero)
                throw new InvalidDataException($"Image decode failed with GDI+ status code {createBitmapResult}.");

            int createHBitmapResult = GdiPlus.GdipCreateHBITMAPFromBitmap(gpBitmap, out nint hBitmap, 0);
            if (createHBitmapResult != 0 || hBitmap == IntPtr.Zero)
                throw new InvalidOperationException($"GdipCreateHBITMAPFromBitmap failed with status code {createHBitmapResult}.");

            return hBitmap;
        }
        finally
        {
            if (gpBitmap != IntPtr.Zero)
                _ = GdiPlus.GdipDisposeImage(gpBitmap);

            if (imageStream is not null)
                Marshal.ReleaseComObject(imageStream);
        }
    }

    internal bool TryCreateMenuBitmap(out nint hBitmap, out bool shouldDisposeBitmap)
    {
        hBitmap = IntPtr.Zero;
        shouldDisposeBitmap = false;

        if (!TryGetHandle(out nint nativeHandle))
            return false;

        if (!ShowAsMonochrome)
        {
            hBitmap = nativeHandle;
            return true;
        }

        return TryCreateMonochromeBitmap(out hBitmap, out shouldDisposeBitmap);
    }

    private bool TryCreateMonochromeBitmap(out nint hBitmap, out bool shouldDisposeBitmap)
    {
        hBitmap = IntPtr.Zero;
        shouldDisposeBitmap = false;

        GdiPlus.EnsureInitialized();

        IStream? imageStream = null;
        nint gpBitmap = IntPtr.Zero;

        try
        {
            int createStreamResult = Ole32.CreateStreamOnHGlobal(IntPtr.Zero, true, out imageStream);
            if (createStreamResult != 0 || imageStream is null)
                return false;

            imageStream.Write(_imageBytes, _imageBytes.Length, IntPtr.Zero);
            imageStream.Seek(0, 0, IntPtr.Zero);

            int createBitmapResult = GdiPlus.GdipCreateBitmapFromStream(imageStream, out gpBitmap);
            if (createBitmapResult != 0 || gpBitmap == IntPtr.Zero)
                return false;

            int widthResult = GdiPlus.GdipGetImageWidth(gpBitmap, out uint width);
            int heightResult = GdiPlus.GdipGetImageHeight(gpBitmap, out uint height);
            if (widthResult != 0 || heightResult != 0 || width == 0 || height == 0)
                return false;

            var rect = new GdiPlus.GpRect
            {
                X = 0,
                Y = 0,
                Width = (int)width,
                Height = (int)height,
            };

            var bitmapData = default(GdiPlus.BitmapData);
            int lockBitsResult = GdiPlus.GdipBitmapLockBits(
                gpBitmap,
                ref rect,
                (uint)(GdiPlus.ImageLockMode.Read | GdiPlus.ImageLockMode.Write),
                GdiPlus.PixelFormat32bppArgb,
                ref bitmapData);

            if (lockBitsResult != 0 || bitmapData.Scan0 == IntPtr.Zero)
                return false;

            try
            {
                ApplyMonochrome(bitmapData, (int)height, ResolveMonochromeArgb(ThemeMode));
            }
            finally
            {
                _ = GdiPlus.GdipBitmapUnlockBits(gpBitmap, ref bitmapData);
            }

            int createHBitmapResult = GdiPlus.GdipCreateHBITMAPFromBitmap(gpBitmap, out hBitmap, 0);
            if (createHBitmapResult != 0 || hBitmap == IntPtr.Zero)
                return false;

            shouldDisposeBitmap = true;
            return true;
        }
        finally
        {
            if (gpBitmap != IntPtr.Zero)
                _ = GdiPlus.GdipDisposeImage(gpBitmap);

            if (imageStream is not null)
                Marshal.ReleaseComObject(imageStream);
        }
    }

    private static void ApplyMonochrome(GdiPlus.BitmapData bitmapData, int height, uint monochromeArgb)
    {
        int stride = Math.Abs(bitmapData.Stride);
        byte[] pixels = new byte[stride * height];
        Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);

        byte blue = (byte)(monochromeArgb & 0xFF);
        byte green = (byte)((monochromeArgb >> 8) & 0xFF);
        byte red = (byte)((monochromeArgb >> 16) & 0xFF);

        for (int offset = 0; offset < pixels.Length; offset += 4)
        {
            byte alpha = pixels[offset + 3];
            if (alpha == 0)
                continue;

            pixels[offset] = blue;
            pixels[offset + 1] = green;
            pixels[offset + 2] = red;
        }

        Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
    }

    private static uint ResolveMonochromeArgb(TrayThemeMode themeMode)
    {
        TrayThemeMode effectiveTheme = themeMode;

        if (effectiveTheme == TrayThemeMode.System)
            effectiveTheme = OSThemeHelper.SystemUsesDarkTheme() ? TrayThemeMode.Dark : TrayThemeMode.Light;

        return effectiveTheme == TrayThemeMode.Dark ? 0xFFFFFFFFu : 0xFF000000u;
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

public sealed class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeHBitmapHandle()
        : base(true)
    {
    }

    public SafeHBitmapHandle(nint preexistingHandle, bool ownsHandle = true)
        : base(ownsHandle)
    {
        SetHandle(preexistingHandle);
    }

    protected override bool ReleaseHandle()
    {
        return Gdi32.DeleteObject(handle);
    }
}
