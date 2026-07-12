using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Appearance;

internal class ThemeSwitchRippleAdorner : Adorner
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
