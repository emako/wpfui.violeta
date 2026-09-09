using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Wpf.Ui.Violeta.Win32;

internal static class GdiPlus
{
    private static readonly object _syncRoot = new();
    private static bool _initialized;

    public const int Ok = 0;
    public const int PropertyTagFrameDelay = 0x5100;
    public static readonly Guid FrameDimensionTime = new("6aedbd6d-3fb5-418a-83a6-7f45229dc872");

    public static void EnsureInitialized()
    {
        if (_initialized)
            return;

        lock (_syncRoot)
        {
            if (_initialized)
                return;

            var startupInput = new GdiplusStartupInput
            {
                GdiplusVersion = 1,
                DebugEventCallback = IntPtr.Zero,
                SuppressBackgroundThread = false,
                SuppressExternalCodecs = false,
            };

            int status = GdiplusStartup(out nint token, ref startupInput, IntPtr.Zero);
            if (status != 0)
                throw new InvalidOperationException($"GdiplusStartup failed with status code {status}.");

            _initialized = true;
        }
    }

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipCreateBitmapFromStream(IStream stream, out nint bitmap);

    [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    public static extern int GdipCreateBitmapFromFile(string filename, out nint bitmap);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipCreateHBITMAPFromBitmap(nint bitmap, out nint hbmReturn, uint background);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipGetImageWidth(nint image, out uint width);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipGetImageHeight(nint image, out uint height);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipBitmapLockBits(nint bitmap, ref GpRect rect, uint flags, int format, ref BitmapData lockedBitmapData);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipBitmapUnlockBits(nint bitmap, ref BitmapData lockedBitmapData);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipDisposeImage(nint image);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipImageForceValidation(nint image);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipImageGetFrameDimensionsCount(nint image, out int count);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipImageGetFrameDimensionsList(nint image, nint dimensionIds, int count);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipImageGetFrameCount(nint image, ref Guid dimensionId, out int count);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipImageSelectActiveFrame(nint image, ref Guid dimensionId, int frameIndex);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipGetPropertyItemSize(nint image, int propId, out uint size);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    public static extern int GdipGetPropertyItem(nint image, int propId, uint propSize, nint buffer);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdiplusStartup(out nint token, ref GdiplusStartupInput input, nint output);

    [StructLayout(LayoutKind.Sequential)]
    private struct GdiplusStartupInput
    {
        public uint GdiplusVersion;
        public nint DebugEventCallback;

        [MarshalAs(UnmanagedType.Bool)]
        public bool SuppressBackgroundThread;

        [MarshalAs(UnmanagedType.Bool)]
        public bool SuppressExternalCodecs;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GpRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapData
    {
        public uint Width;
        public uint Height;
        public int Stride;
        public int PixelFormat;
        public nint Scan0;
        public nint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PropertyItem
    {
        public int Id;
        public int Length;
        public short Type;
        public nint Value;
    }

    [Flags]
    public enum ImageLockMode : uint
    {
        Read = 0x0001,
        Write = 0x0002,
        UserInputBuffer = 0x0004,
    }

    public const int PixelFormat32bppArgb = 0x26200A;
}
