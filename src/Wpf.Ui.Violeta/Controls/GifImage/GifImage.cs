using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Wpf.Ui.Violeta.Controls.Primitives;
using Wpf.Ui.Violeta.Win32;
using Image = System.Windows.Controls.Image;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Displays an animated GIF as an <see cref="Wpf.Ui.Controls.IconElement"/>.
/// </summary>
public class GifImage : IconElementEx, IDisposable
{
    private static readonly TimeSpan MinFrameDelay = TimeSpan.FromMilliseconds(20);

    private Image? _image;
    private nint _nativeImage;
    private IStream? _nativeStream;
    private int[]? _frameDelays;
    private int _frameCount;
    private int _frameIndex;
    private Thread? _animationThread;
    private volatile bool _isAnimating;
    private bool _isLoaded;
    private bool _disposed;
    private readonly object _syncRoot = new();

    static GifImage()
    {
        VisibilityProperty.OverrideMetadata(
            typeof(GifImage),
            new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));
    }

    public GifImage()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public GifImage(string filename)
        : this()
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("File name cannot be null or empty.", nameof(filename));

        Uri = new Uri(Path.GetFullPath(filename), UriKind.Absolute);
    }

    public GifImage(Stream stream)
        : this()
    {
        _ = stream ?? throw new ArgumentNullException(nameof(stream));
        LoadFromStream(stream);
        StartAnimate();
    }

    /// <summary>
    /// Identifies the <see cref="Uri"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UriProperty =
        DependencyProperty.Register(
            nameof(Uri),
            typeof(Uri),
            typeof(GifImage),
            new FrameworkPropertyMetadata(null, OnUriChanged));

    /// <summary>
    /// Gets or sets the URI of the GIF to display.
    /// </summary>
    public Uri? Uri
    {
        get => (Uri?)GetValue(UriProperty);
        set => SetValue(UriProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Stretch"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StretchProperty =
        Image.StretchProperty.AddOwner(
            typeof(GifImage),
            new FrameworkPropertyMetadata(Stretch.Uniform, OnStretchChanged));

    /// <summary>
    /// Gets or sets how the GIF is stretched to fill the layout slot.
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    protected override UIElement InitializeChildren()
    {
        _image = new Image
        {
            Stretch = Stretch,
            SnapsToDevicePixels = true,
        };

        ApplyCurrentFrame();
        Children.Add(_image);
        return _image;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~GifImage() => Dispose(false);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        StopAnimate();
        DisposeNative();
        _disposed = true;
    }

    private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (GifImage)d;
        control.StopAnimate();
        control.DisposeNative();

        if (e.NewValue is Uri uri)
        {
            control.LoadFromUri(uri);
            control.StartAnimate();
        }
        else
        {
            control.ClearImageSource();
        }
    }

    private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (GifImage)d;
        if (control._image != null)
            control._image.Stretch = (Stretch)e.NewValue;
    }

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (GifImage)d;
        if (control._nativeImage == IntPtr.Zero)
            return;

        if ((Visibility)e.NewValue != Visibility.Visible)
        {
            control.StopAnimate();
        }
        else if (!control._isAnimating)
        {
            control.StartAnimate();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || _isLoaded)
            return;

        _isLoaded = true;

        if (Uri != null && _nativeImage == IntPtr.Zero)
        {
            LoadFromUri(Uri);
            StartAnimate();
        }
        else if (_nativeImage != IntPtr.Zero && !_isAnimating)
        {
            StartAnimate();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopAnimate();
        DisposeNative();
        ClearImageSource();
        _isLoaded = false;
    }

    private void LoadFromUri(Uri uri)
    {
        try
        {
            if (uri.IsAbsoluteUri && string.Equals(uri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                using var fileStream = File.OpenRead(uri.LocalPath);
                LoadFromStream(fileStream);
                return;
            }

            StreamResourceInfo? streamInfo;
            if (!uri.IsAbsoluteUri)
            {
                streamInfo = Application.GetContentStream(uri) ?? Application.GetResourceStream(uri);
            }
            else if (uri.GetLeftPart(UriPartial.Authority).Contains("siteoforigin", StringComparison.OrdinalIgnoreCase))
            {
                streamInfo = Application.GetRemoteStream(uri);
            }
            else
            {
                streamInfo = Application.GetContentStream(uri) ?? Application.GetResourceStream(uri);
            }

            if (streamInfo?.Stream is null)
                throw new FileNotFoundException("Resource not found.", uri.ToString());

            using (streamInfo.Stream)
            {
                LoadFromStream(streamInfo.Stream);
            }
        }
        catch
        {
            DisposeNative();
            ClearImageSource();
        }
    }

    private void LoadFromStream(Stream stream)
    {
        DisposeNative();

        GdiPlus.EnsureInitialized();

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        byte[] bytes = memoryStream.ToArray();
        if (bytes.Length == 0)
            return;

        int createStreamResult = Ole32.CreateStreamOnHGlobal(IntPtr.Zero, true, out IStream? imageStream);
        if (createStreamResult != 0 || imageStream is null)
            throw new InvalidOperationException($"CreateStreamOnHGlobal failed with HRESULT 0x{createStreamResult:X8}.");

        imageStream.Write(bytes, bytes.Length, IntPtr.Zero);
        imageStream.Seek(0, 0, IntPtr.Zero);

        int createBitmapResult = GdiPlus.GdipCreateBitmapFromStream(imageStream, out nint bitmap);
        if (createBitmapResult != GdiPlus.Ok || bitmap == IntPtr.Zero)
        {
            Marshal.ReleaseComObject(imageStream);
            throw new InvalidDataException($"Image decode failed with GDI+ status code {createBitmapResult}.");
        }

        int validation = GdiPlus.GdipImageForceValidation(bitmap);
        if (validation != GdiPlus.Ok)
        {
            _ = GdiPlus.GdipDisposeImage(bitmap);
            Marshal.ReleaseComObject(imageStream);
            throw new InvalidDataException($"Image validation failed with GDI+ status code {validation}.");
        }

        _nativeStream = imageStream;
        _nativeImage = bitmap;
        ReadAnimationMetadata();
        SelectActiveFrame(0);
        ApplyCurrentFrame();
    }

    private void ReadAnimationMetadata()
    {
        _frameCount = 1;
        _frameIndex = 0;
        _frameDelays = null;

        if (_nativeImage == IntPtr.Zero)
            return;

        Guid timeDimension = GdiPlus.FrameDimensionTime;
        if (GdiPlus.GdipImageGetFrameCount(_nativeImage, ref timeDimension, out int frameCount) != GdiPlus.Ok || frameCount <= 1)
            return;

        _frameCount = frameCount;
        _frameDelays = new int[frameCount];

        if (GdiPlus.GdipGetPropertyItemSize(_nativeImage, GdiPlus.PropertyTagFrameDelay, out uint size) != GdiPlus.Ok || size == 0)
            return;

        nint buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            if (GdiPlus.GdipGetPropertyItem(_nativeImage, GdiPlus.PropertyTagFrameDelay, size, buffer) != GdiPlus.Ok)
                return;

            var item = Marshal.PtrToStructure<GdiPlus.PropertyItem>(buffer);
            if (item.Value == IntPtr.Zero || item.Length < frameCount * 4)
                return;

            byte[] values = new byte[item.Length];
            Marshal.Copy(item.Value, values, 0, item.Length);

            for (int i = 0; i < frameCount; i++)
            {
                int delay = BitConverter.ToInt32(values, i * 4);
                _frameDelays[i] = delay;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private bool CanAnimate => _nativeImage != IntPtr.Zero && _frameCount > 1 && _frameDelays != null;

    private void StartAnimate()
    {
        if (_nativeImage == IntPtr.Zero || _isAnimating || Visibility != Visibility.Visible)
            return;

        ApplyCurrentFrame();

        if (!CanAnimate)
            return;

        _isAnimating = true;
        _animationThread = new Thread(AnimationLoop)
        {
            IsBackground = true,
            Name = "GifImageAnimator",
        };
        _animationThread.Start();
    }

    private void StopAnimate()
    {
        _isAnimating = false;
        _animationThread = null;
    }

    private void AnimationLoop()
    {
        while (_isAnimating)
        {
            int delayHundredths = 0;
            lock (_syncRoot)
            {
                if (!_isAnimating || _frameDelays is null || _frameCount <= 1)
                    break;

                delayHundredths = _frameDelays[_frameIndex];
            }

            TimeSpan delay = TimeSpan.FromMilliseconds(Math.Max(delayHundredths * 10, MinFrameDelay.TotalMilliseconds));
            Thread.Sleep(delay);

            if (!_isAnimating)
                break;

            lock (_syncRoot)
            {
                if (!_isAnimating || _nativeImage == IntPtr.Zero || _frameDelays is null)
                    break;

                _frameIndex = (_frameIndex + 1) % _frameCount;
                SelectActiveFrame(_frameIndex);
            }

            _ = Dispatcher.BeginInvoke(new Action(ApplyCurrentFrame));
        }
    }

    private void SelectActiveFrame(int frameIndex)
    {
        if (_nativeImage == IntPtr.Zero)
            return;

        Guid timeDimension = GdiPlus.FrameDimensionTime;
        _ = GdiPlus.GdipImageSelectActiveFrame(_nativeImage, ref timeDimension, frameIndex);
    }

    private void ApplyCurrentFrame()
    {
        if (_image is null)
            return;

        nint nativeImage;
        lock (_syncRoot)
        {
            nativeImage = _nativeImage;
        }

        if (nativeImage == IntPtr.Zero)
            return;

        nint handle = IntPtr.Zero;
        try
        {
            lock (_syncRoot)
            {
                if (_nativeImage == IntPtr.Zero)
                    return;

                int status = GdiPlus.GdipCreateHBITMAPFromBitmap(_nativeImage, out handle, 0);
                if (status != GdiPlus.Ok || handle == IntPtr.Zero)
                    return;
            }

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            _image.Source = source;
        }
        catch
        {
            // ignored
        }
        finally
        {
            if (handle != IntPtr.Zero)
                _ = Gdi32.DeleteObject(handle);
        }
    }

    private void ClearImageSource()
    {
        _image?.ClearValue(Image.SourceProperty);
    }

    private void DisposeNative()
    {
        lock (_syncRoot)
        {
            if (_nativeImage != IntPtr.Zero)
            {
                _ = GdiPlus.GdipDisposeImage(_nativeImage);
                _nativeImage = IntPtr.Zero;
            }

            if (_nativeStream is not null)
            {
                Marshal.ReleaseComObject(_nativeStream);
                _nativeStream = null;
            }

            _frameDelays = null;
            _frameCount = 0;
            _frameIndex = 0;
        }
    }
}
