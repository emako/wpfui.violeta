using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Shapes;

/// <summary>
/// Renders an arc shape supporting Arc, Ring, and Pie mode controlled by ArcThickness.
/// </summary>
public sealed class Arc : Shape
{
    public static readonly DependencyProperty StartAngleProperty =
        DependencyProperty.Register(
            nameof(StartAngle),
            typeof(double),
            typeof(Arc),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EndAngleProperty =
        DependencyProperty.Register(
            nameof(EndAngle),
            typeof(double),
            typeof(Arc),
            new FrameworkPropertyMetadata(90d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ArcThicknessProperty =
        DependencyProperty.Register(
            nameof(ArcThickness),
            typeof(double),
            typeof(Arc),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ArcThicknessUnitProperty =
        DependencyProperty.Register(
            nameof(ArcThicknessUnit),
            typeof(ArcUnitType),
            typeof(Arc),
            new FrameworkPropertyMetadata(ArcUnitType.Pixel, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Gets or sets the start angle.
    /// </summary>
    /// <value>The start angle in degrees. Zero degrees is pointing up.</value>
    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the end angle.
    /// </summary>
    /// <value>The end angle in degrees. Zero degrees is pointing up.</value>
    public double EndAngle
    {
        get => (double)GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }

    /// <summary>
    /// Gets or sets the arc thickness.
    /// </summary>
    /// <value>The arc thickness in pixels or percentage depending on "ArcThicknessUnit".</value>
    public double ArcThickness
    {
        get => (double)GetValue(ArcThicknessProperty);
        set => SetValue(ArcThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the arc thickness unit.
    /// </summary>
    /// <value>The arc thickness unit in pixels or percentage.</value>
    public ArcUnitType ArcThicknessUnit
    {
        get => (ArcUnitType)GetValue(ArcThicknessUnitProperty);
        set => SetValue(ArcThicknessUnitProperty, value);
    }

    protected override Geometry DefiningGeometry
    {
        get
        {
            Rect bounds = new(new Point(0, 0), RenderSize);
            if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return Geometry.Empty;
            }

            double start = NormalizeAngle(StartAngle);
            double end = NormalizeAngle(EndAngle);
            bool sameAngle = AreClose(StartAngle, EndAngle);
            if (end < start || sameAngle)
            {
                end += 360d;
            }

            double sweep = end - start;
            double relativeThickness = GetRelativeThickness(bounds);

            if (sameAngle)
            {
                return CreateZeroAngleGeometry(bounds, start, relativeThickness);
            }

            if (AreClose(sweep % 360d, 0d))
            {
                if (AreClose(relativeThickness, 0d) || AreClose(relativeThickness, 1d))
                {
                    return new EllipseGeometry(bounds);
                }

                return CreateFullRingGeometry(bounds, relativeThickness);
            }

            if (AreClose(relativeThickness, 1d))
            {
                return CreatePieGeometry(bounds, start, end);
            }

            if (AreClose(relativeThickness, 0d))
            {
                return CreateOpenArcGeometry(bounds, start, end);
            }

            return CreateRingArcGeometry(bounds, start, end, relativeThickness);
        }
    }

    private double GetRelativeThickness(Rect bounds)
    {
        double radius = Math.Min(bounds.Width / 2d, bounds.Height / 2d);
        if (radius <= 0)
        {
            return 0d;
        }

        double relative = ArcThicknessUnit == ArcUnitType.Pixel ? ArcThickness / radius : ArcThickness;
        return Math.Max(0d, Math.Min(1d, relative));
    }

    private static PathGeometry CreateOpenArcGeometry(Rect bounds, double start, double end)
    {
        PathFigure figure = new()
        {
            StartPoint = GetArcPoint(start, bounds),
            IsClosed = false,
            IsFilled = false
        };
        figure.Segments.Add(new ArcSegment
        {
            Point = GetArcPoint(end, bounds),
            Size = GetArcSize(bounds),
            IsLargeArc = end - start > 180d,
            SweepDirection = SweepDirection.Clockwise
        });

        PathGeometry geometry = new();
        geometry.Figures.Add(figure);
        return geometry;
    }

    private static PathGeometry CreatePieGeometry(Rect bounds, double start, double end)
    {
        PathFigure figure = new()
        {
            StartPoint = GetArcPoint(start, bounds),
            IsClosed = true,
            IsFilled = true
        };
        figure.Segments.Add(new ArcSegment
        {
            Point = GetArcPoint(end, bounds),
            Size = GetArcSize(bounds),
            IsLargeArc = end - start > 180d,
            SweepDirection = SweepDirection.Clockwise
        });
        figure.Segments.Add(new LineSegment { Point = GetCenter(bounds) });

        PathGeometry geometry = new();
        geometry.Figures.Add(figure);
        return geometry;
    }

    private static PathGeometry CreateRingArcGeometry(Rect bounds, double start, double end, double relativeThickness)
    {
        Rect innerBounds = ResizeRect(bounds, 1d - relativeThickness);
        PathFigure figure = new()
        {
            StartPoint = GetArcPoint(start, bounds),
            IsClosed = true,
            IsFilled = true
        };

        figure.Segments.Add(new ArcSegment
        {
            Point = GetArcPoint(end, bounds),
            Size = GetArcSize(bounds),
            IsLargeArc = end - start > 180d,
            SweepDirection = SweepDirection.Clockwise
        });
        figure.Segments.Add(new LineSegment { Point = GetArcPoint(end, innerBounds) });
        figure.Segments.Add(new ArcSegment
        {
            Point = GetArcPoint(start, innerBounds),
            Size = GetArcSize(innerBounds),
            IsLargeArc = end - start > 180d,
            SweepDirection = SweepDirection.Counterclockwise
        });

        PathGeometry geometry = new() { FillRule = FillRule.Nonzero };
        geometry.Figures.Add(figure);
        return geometry;
    }

    private static CombinedGeometry CreateFullRingGeometry(Rect bounds, double relativeThickness)
    {
        EllipseGeometry outer = new(bounds);
        EllipseGeometry inner = new(ResizeRect(bounds, 1d - relativeThickness));
        return new CombinedGeometry(GeometryCombineMode.Exclude, outer, inner);
    }

    private static LineGeometry CreateZeroAngleGeometry(Rect bounds, double angle, double relativeThickness)
    {
        Point start = GetArcPoint(angle, bounds);
        Point end = GetArcPoint(angle, ResizeRect(bounds, 1d - relativeThickness));
        return new LineGeometry(start, end);
    }

    private static Point GetArcPoint(double angle, Rect bounds)
    {
        double rad = angle * Math.PI / 180d;
        Point c = GetCenter(bounds);
        double rx = bounds.Width / 2d;
        double ry = bounds.Height / 2d;
        return new Point(
            c.X + rx * Math.Sin(rad),
            c.Y - ry * Math.Cos(rad));
    }

    private static Size GetArcSize(Rect bounds)
        => new(bounds.Width / 2d, bounds.Height / 2d);

    private static Point GetCenter(Rect bounds)
        => new(bounds.Left + bounds.Width / 2d, bounds.Top + bounds.Height / 2d);

    private static Rect ResizeRect(Rect bounds, double factor)
    {
        double width = bounds.Width * factor;
        double height = bounds.Height * factor;
        Point center = GetCenter(bounds);
        return new Rect(center.X - width / 2d, center.Y - height / 2d, width, height);
    }

    private static bool AreClose(double a, double b)
        => Math.Abs(a - b) < 0.0001d;

    private static double NormalizeAngle(double degree)
    {
        if (degree < 0d || degree > 360d)
        {
            degree %= 360d;
            if (degree < 0d)
            {
                degree += 360d;
            }
        }

        return degree;
    }
}

/// <summary>
/// Specifies the unit of thickness.
/// </summary>
public enum ArcUnitType
{
    /// <summary>
    /// Unit in pixels.
    /// </summary>
    Pixel,

    /// <summary>
    /// Unit in percentage relative to the bounding box.
    /// </summary>
    Percent,
}
