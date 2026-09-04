using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

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
/// <see cref="ThemeManager.Apply"/>.  No code-behind changes are required.
/// </remarks>
public static class ThemeSwitchEffect
{
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
            typeof(RenderTargetBitmap),
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

    // Capture a screenshot of the current window content BEFORE the user picks a new theme.
    // DropDownOpened fires before any selection change, so the snapshot always shows the
    // old theme.
    private static void OnDropDownOpened(object? sender, EventArgs e)
    {
        var comboBox = (ComboBox)sender!;

        comboBox.SetValue(OldIndexProperty, comboBox.SelectedIndex);

        var window = Window.GetWindow(comboBox);
        if (window?.Content is not UIElement rootVisual)
        {
            comboBox.SetValue(SnapshotProperty, null);
            return;
        }

        try
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(rootVisual);
            RenderTargetBitmap snapshot = new(
                Math.Max(1, (int)(rootVisual.RenderSize.Width * dpi.DpiScaleX)),
                Math.Max(1, (int)(rootVisual.RenderSize.Height * dpi.DpiScaleY)),
                dpi.PixelsPerInchX,
                dpi.PixelsPerInchY,
                PixelFormats.Pbgra32);

            snapshot.Render(rootVisual);
            comboBox.SetValue(SnapshotProperty, snapshot);
        }
        catch
        {
            comboBox.SetValue(SnapshotProperty, null);
        }
    }

    // By the time DropDownClosed fires, the TwoWay binding has already pushed the new
    // SelectedIndex to the VM and ThemeManager.Apply() has updated the ResourceDictionaries.
    // We add the adorner synchronously here with Diameter = 0, so the very first rendered
    // frame already shows the old-theme snapshot covering the window.  The animation then
    // grows the transparent circle to reveal the new theme underneath.
    private static void OnDropDownClosed(object? sender, EventArgs e)
    {
        var comboBox = (ComboBox)sender!;
        var snapshot = (RenderTargetBitmap?)comboBox.GetValue(SnapshotProperty);
        int oldIndex = (int)comboBox.GetValue(OldIndexProperty);

        // Nothing to animate: no snapshot or user cancelled without changing selection.
        if (snapshot is null || oldIndex == comboBox.SelectedIndex)
        {
            return;
        }

        comboBox.SetValue(SnapshotProperty, null);

        var window = Window.GetWindow(comboBox);
        if (window?.Content is not UIElement rootVisual)
        {
            return;
        }

        var adornerLayer = AdornerLayer.GetAdornerLayer(rootVisual);
        if (adornerLayer is null)
        {
            return;
        }

        // Center of the ripple = center of the ComboBox, in rootVisual's coordinate space.
        Point center = comboBox.TranslatePoint(
            new Point(comboBox.ActualWidth / 2.0, comboBox.ActualHeight / 2.0),
            rootVisual);

        ThemeSwitchRippleAdorner ripple = new(rootVisual)
        {
            Center = center,
            OuterBrush = new ImageBrush(snapshot),
        };

        ripple.Completed += (_, _) => adornerLayer.Remove(ripple);
        adornerLayer.Add(ripple);
        ripple.Play(speed: 3200, new SineEase { EasingMode = EasingMode.EaseOut });
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
