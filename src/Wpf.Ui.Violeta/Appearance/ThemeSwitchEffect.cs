using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Appearance;

/// <summary>
/// Provides an attached property that plays a circular-ripple transition animation when a
/// theme-switching <see cref="ComboBox"/> changes its selection.
/// </summary>
/// <remarks>
/// Usage — add to the theme <see cref="ComboBox"/> in XAML:
/// <code>vio:ThemeSwitchEffect.IsEnabled="True"</code>
/// The ComboBox must have a <c>SelectedIndex</c> bound (one-way or two-way) to an
/// <c>ApplicationTheme</c>-valued integer property whose setter calls
/// <see cref="ThemeManager.Apply"/>, or a <c>SelectionChanged</c> handler that applies the
/// theme synchronously.  No other code-behind changes are required.
/// </remarks>
public static class ThemeSwitchEffect
{
    private const double RippleSpeed = 3200d;
    private const string OpaqueBackgroundKey = "MainWindowBackgroundNoTransparent";
    private static readonly Duration BackgroundFadeDuration = new(TimeSpan.FromMilliseconds(200));

    // -- Public attached property --------------------------------------------

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ThemeSwitchEffect),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(IsEnabledProperty, value);

    // -- Private per-instance storage ----------------------------------------

    private static readonly DependencyProperty SnapshotProperty =
        DependencyProperty.RegisterAttached(
            "ThemeSwitchEffect_Snapshot",
            typeof(BitmapSource),
            typeof(ThemeSwitchEffect),
            new PropertyMetadata(null));

    private static readonly DependencyProperty CaptureTokenProperty =
        DependencyProperty.RegisterAttached(
            "ThemeSwitchEffect_CaptureToken",
            typeof(object),
            typeof(ThemeSwitchEffect),
            new PropertyMetadata(null));

    private static readonly DependencyProperty OldIndexProperty =
        DependencyProperty.RegisterAttached(
            "ThemeSwitchEffect_OldIndex",
            typeof(int),
            typeof(ThemeSwitchEffect),
            new PropertyMetadata(-1));

    // -- Callback -------------------------------------------------------------

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            comboBox.DropDownOpened += OnDropDownOpened;
            comboBox.DropDownClosed += OnDropDownClosed;
        }
        else
        {
            comboBox.DropDownOpened -= OnDropDownOpened;
            comboBox.DropDownClosed -= OnDropDownClosed;
        }
    }

    // The snapshot must show the old theme, so it is taken while the drop-down is open.
    // Loaded priority runs after the popup has rendered but before pending input, so the
    // capture neither stalls the opening frame nor loses the race against a quick click.
    private static void OnDropDownOpened(object? sender, EventArgs e)
    {
        ComboBox comboBox = (ComboBox)sender!;
        int oldIndex = comboBox.SelectedIndex;
        object token = new();

        comboBox.SetValue(OldIndexProperty, oldIndex);
        comboBox.SetValue(SnapshotProperty, null);
        comboBox.SetValue(CaptureTokenProperty, token);

        _ = comboBox.Dispatcher.InvokeAsync(() =>
        {
            if (!ReferenceEquals(comboBox.GetValue(CaptureTokenProperty), token)
                || !comboBox.IsDropDownOpen
                || comboBox.SelectedIndex != oldIndex)
            {
                return;
            }

            if (Window.GetWindow(comboBox) is { Content: FrameworkElement rootVisual } window)
            {
                comboBox.SetValue(SnapshotProperty, TryCapture(window, rootVisual));
            }
        }, DispatcherPriority.Loaded);
    }

    // By the time DropDownClosed fires, the selection handler has already applied the new
    // theme synchronously, but nothing has been rendered yet.  The adorner is added here with
    // Diameter = 0 so the first new-theme frame is fully covered by the old-theme snapshot,
    // and an opaque new-theme background is placed under the content so the revealed area
    // never shows the DWM backdrop catching up asynchronously.
    private static void OnDropDownClosed(object? sender, EventArgs e)
    {
        ComboBox comboBox = (ComboBox)sender!;
        BitmapSource? snapshot = (BitmapSource?)comboBox.GetValue(SnapshotProperty);
        int oldIndex = (int)comboBox.GetValue(OldIndexProperty);

        comboBox.ClearValue(SnapshotProperty);
        comboBox.ClearValue(CaptureTokenProperty);

        if (snapshot is null || oldIndex == comboBox.SelectedIndex)
        {
            return;
        }

        if (Window.GetWindow(comboBox) is not { Content: FrameworkElement rootVisual } window
            || AdornerLayer.GetAdornerLayer(rootVisual) is not AdornerLayer adornerLayer)
        {
            return;
        }

        Point center = comboBox.TranslatePoint(
            new Point(comboBox.ActualWidth / 2d, comboBox.ActualHeight / 2d),
            rootVisual);

        ThemeSwitchRippleAdorner ripple = new(rootVisual)
        {
            Center = center,
            OuterBrush = new ImageBrush(snapshot),
        };

        SolidColorBrush? cover = null;
        Brush? restored = null;

        // ShellWindow re-applies its backdrop (and overwrites Background) from a Normal-priority
        // operation queued by ThemeManager.Changed; queue the cover after it, still before render.
        _ = window.Dispatcher.InvokeAsync(() =>
        {
            restored = window.Background;
            cover = new SolidColorBrush(ResolveOpaqueBackground(window));
            window.SetCurrentValue(Control.BackgroundProperty, cover);
        }, DispatcherPriority.Normal);

        ripple.Completed += (_, _) =>
        {
            adornerLayer.Remove(ripple);
            RestoreBackground(window, cover, restored);
        };

        adornerLayer.Add(ripple);

        // The first frame after a theme switch is expensive; starting the clock now would
        // let that frame swallow the beginning of the ripple.
        AfterFrames(2, () => ripple.Play(RippleSpeed, new SineEase { EasingMode = EasingMode.EaseOut }));
    }

    private static BitmapSource? TryCapture(Window window, FrameworkElement rootVisual)
    {
        try
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(window);
            int width = ToPixels(window.ActualWidth, dpi.DpiScaleX);
            int height = ToPixels(window.ActualHeight, dpi.DpiScaleY);

            if (width <= 0 || height <= 0)
            {
                return null;
            }

            RenderTargetBitmap bitmap = new(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);

            // Translucent backdrops (Mica, Acrylic) are composed by DWM and never reach the
            // bitmap, so the old theme's opaque background is painted underneath first.
            DrawingVisual background = new();
            using (DrawingContext drawingContext = background.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    new SolidColorBrush(ResolveOpaqueBackground(window)),
                    null,
                    new Rect(0d, 0d, window.ActualWidth, window.ActualHeight));
            }

            bitmap.Render(background);
            bitmap.Render(window);

            // Rendering the content root directly would apply its VisualOffset twice (for
            // example the maximized-window padding), so the window is rendered and cropped.
            Rect bounds = rootVisual.TransformToAncestor(window)
                .TransformBounds(new Rect(rootVisual.RenderSize));
            int x = Math.Max(0, ToPixels(bounds.X, dpi.DpiScaleX));
            int y = Math.Max(0, ToPixels(bounds.Y, dpi.DpiScaleY));
            Int32Rect crop = new(
                x,
                y,
                Math.Min(width - x, ToPixels(bounds.Width, dpi.DpiScaleX)),
                Math.Min(height - y, ToPixels(bounds.Height, dpi.DpiScaleY)));

            if (crop.Width <= 0 || crop.Height <= 0)
            {
                return null;
            }

            // CroppedBitmap crops lazily on first render; copy now so the covering frame is cheap.
            WriteableBitmap snapshot = new(new CroppedBitmap(bitmap, crop));
            snapshot.Freeze();
            return snapshot;
        }
        catch
        {
            return null;
        }
    }

    private static Color ResolveOpaqueBackground(Window window)
    {
        if (window.TryFindResource(OpaqueBackgroundKey) is SolidColorBrush { Color.A: 255 } resource)
        {
            return resource.Color;
        }

        return window.Background is SolidColorBrush { Color.A: 255 } background
            ? background.Color
            : Colors.White;
    }

    private static void RestoreBackground(Window window, SolidColorBrush? cover, Brush? restored)
    {
        // Someone else (e.g. another theme switch) replaced the cover; leave their value alone.
        if (cover is null || !ReferenceEquals(window.Background, cover))
        {
            return;
        }

        if (restored is SolidColorBrush { Color.A: 255 })
        {
            window.SetCurrentValue(Control.BackgroundProperty, restored);
            return;
        }

        // The backdrop is translucent: fade the cover out instead of popping to it.
        DoubleAnimation fade = new(0d, BackgroundFadeDuration);
        fade.Completed += (_, _) =>
        {
            if (ReferenceEquals(window.Background, cover))
            {
                window.SetCurrentValue(Control.BackgroundProperty, restored);
            }
        };

        cover.BeginAnimation(Brush.OpacityProperty, fade);
    }

    private static int ToPixels(double dips, double scale) => (int)Math.Round(dips * scale);

    private static void AfterFrames(int frames, Action action)
    {
        int remaining = frames;
        TimeSpan lastFrameTime = TimeSpan.MinValue;

        void OnRendering(object? sender, EventArgs e)
        {
            // Rendering can be raised more than once per frame.
            TimeSpan frameTime = ((RenderingEventArgs)e).RenderingTime;
            if (frameTime == lastFrameTime)
            {
                return;
            }

            lastFrameTime = frameTime;

            if (--remaining > 0)
            {
                return;
            }

            CompositionTarget.Rendering -= OnRendering;
            action();
        }

        CompositionTarget.Rendering += OnRendering;
    }
}

internal sealed class ThemeSwitchRippleAdorner : Adorner
{
    private readonly UIElement _adornedElement;
    private RectangleGeometry? _cachedRectangleGeometry;
    private EllipseGeometry? _cachedEllipseGeometry;
    private CombinedGeometry? _cachedOuterGeometry;

    public ThemeSwitchRippleAdorner(UIElement adornedElement) : base(adornedElement)
    {
        _adornedElement = adornedElement;
        IsHitTestVisible = false;
    }

    public Point Center
    {
        get => (Point)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public Brush? OuterBrush
    {
        get => (Brush?)GetValue(OuterBrushProperty);
        set => SetValue(OuterBrushProperty, value);
    }

    public double Diameter
    {
        get => (double)GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public event EventHandler? Completed;

    public void Play(double speed, IEasingFunction? easingFunction)
    {
        Point center = Center;
        Size renderSize = _adornedElement.RenderSize;

        Vector d1 = new(center.X, center.Y);
        Vector d2 = new(renderSize.Width - center.X, center.Y);
        Vector d3 = new(center.X, renderSize.Height - center.Y);
        Vector d4 = new(renderSize.Width - center.X, renderSize.Height - center.Y);

        double maxRadiusSquared = Math.Max(
            Math.Max(d1.LengthSquared, d2.LengthSquared),
            Math.Max(d3.LengthSquared, d4.LengthSquared));

        double maxDiameter = Math.Sqrt(maxRadiusSquared) * 2.0;
        double fromDiameter = Diameter > maxDiameter ? 0.0 : Diameter;
        double durationSeconds = (maxDiameter - fromDiameter) / speed;

        DoubleAnimation animation = new()
        {
            From = fromDiameter,
            To = maxDiameter,
            Duration = new Duration(TimeSpan.FromSeconds(durationSeconds)),
            EasingFunction = easingFunction,
        };

        animation.Completed += (_, _) =>
        {
            BeginAnimation(DiameterProperty, null);
            Diameter = maxDiameter;
            Completed?.Invoke(this, EventArgs.Empty);
        };

        BeginAnimation(DiameterProperty, animation);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        Rect wholeRect = new(0, 0, ActualWidth, ActualHeight);
        Point center = Center;
        double radius = Diameter / 2.0;

        RectangleGeometry rectangleGeometry = _cachedRectangleGeometry ??= new RectangleGeometry();
        EllipseGeometry ellipseGeometry = _cachedEllipseGeometry ??= new EllipseGeometry();
        CombinedGeometry outerGeometry = _cachedOuterGeometry ??= new CombinedGeometry();

        rectangleGeometry.Rect = wholeRect;
        ellipseGeometry.Center = center;
        ellipseGeometry.RadiusX = radius;
        ellipseGeometry.RadiusY = radius;

        outerGeometry.Geometry1 = rectangleGeometry;
        outerGeometry.Geometry2 = ellipseGeometry;
        outerGeometry.GeometryCombineMode = GeometryCombineMode.Exclude;

        if (OuterBrush is not null)
        {
            drawingContext.PushClip(outerGeometry);
            drawingContext.DrawRectangle(OuterBrush, null, wholeRect);
            drawingContext.Pop();
        }
    }

    public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
        nameof(Center), typeof(Point), typeof(ThemeSwitchRippleAdorner),
        new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
        nameof(Diameter), typeof(double), typeof(ThemeSwitchRippleAdorner),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OuterBrushProperty = DependencyProperty.Register(
        nameof(OuterBrush), typeof(Brush), typeof(ThemeSwitchRippleAdorner),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
}
