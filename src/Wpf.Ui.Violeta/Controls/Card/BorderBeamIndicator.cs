using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Renders an animated gradient beam traveling along a rounded-rectangle border ring.
/// </summary>
public class BorderBeamIndicator : FrameworkElement
{
    private const double MaxBeamColorStopPercent = 0.7;

    private readonly DispatcherTimer _timer;
    private DateTime _animationStart = DateTime.UtcNow;
    private double _progress;

    static BorderBeamIndicator()
    {
        IsHitTestVisibleProperty.OverrideMetadata(
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(false));

        ClipToBoundsProperty.OverrideMetadata(
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(false));
    }

    public BorderBeamIndicator()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0),
        };
        _timer.Tick += OnTimerTick;

        Loaded += (_, _) => UpdateAnimationState();
        Unloaded += (_, _) => StopAnimation();
        IsVisibleChanged += (_, _) => UpdateAnimationState();
    }

    #region IsActive

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(true, OnAnimationPropertyChanged));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    #endregion IsActive

    #region Duration

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(TimeSpan),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(TimeSpan.FromSeconds(6), OnAnimationPropertyChanged));

    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    #endregion Duration

    #region BeamSize

    public static readonly DependencyProperty BeamSizeProperty =
        DependencyProperty.Register(
            nameof(BeamSize),
            typeof(double),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                100.0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                CoercePositive));

    public double BeamSize
    {
        get => (double)GetValue(BeamSizeProperty);
        set => SetValue(BeamSizeProperty, value);
    }

    #endregion BeamSize

    #region LineWidth

    public static readonly DependencyProperty LineWidthProperty =
        DependencyProperty.Register(
            nameof(LineWidth),
            typeof(double),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                1.0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                CoercePositive));

    public double LineWidth
    {
        get => (double)GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    #endregion LineWidth

    #region Outset

    public static readonly DependencyProperty OutsetProperty =
        DependencyProperty.Register(
            nameof(Outset),
            typeof(double),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                double.NaN,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Distance the beam layer extends beyond the host edge. When unset, the host border thickness is used.
    /// </summary>
    public double Outset
    {
        get => (double)GetValue(OutsetProperty);
        set => SetValue(OutsetProperty, value);
    }

    #endregion Outset

    #region Count

    public static readonly DependencyProperty CountProperty =
        DependencyProperty.Register(
            nameof(Count),
            typeof(int),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                1,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                CoerceCount));

    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    #endregion Count

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                new CornerRadius(0),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius

    #region HostBorderThickness

    public static readonly DependencyProperty HostBorderThicknessProperty =
        DependencyProperty.Register(
            nameof(HostBorderThickness),
            typeof(Thickness),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                new Thickness(0),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public Thickness HostBorderThickness
    {
        get => (Thickness)GetValue(HostBorderThicknessProperty);
        set => SetValue(HostBorderThicknessProperty, value);
    }

    #endregion HostBorderThickness

    #region BeamColor

    public static readonly DependencyProperty BeamColorProperty =
        DependencyProperty.Register(
            nameof(BeamColor),
            typeof(Color),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                Colors.DodgerBlue,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public Color BeamColor
    {
        get => (Color)GetValue(BeamColorProperty);
        set => SetValue(BeamColorProperty, value);
    }

    #endregion BeamColor

    #region BeamHighlightColor

    public static readonly DependencyProperty BeamHighlightColorProperty =
        DependencyProperty.Register(
            nameof(BeamHighlightColor),
            typeof(Color),
            typeof(BorderBeamIndicator),
            new FrameworkPropertyMetadata(
                Colors.DeepSkyBlue,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public Color BeamHighlightColor
    {
        get => (Color)GetValue(BeamHighlightColorProperty);
        set => SetValue(BeamHighlightColorProperty, value);
    }

    #endregion BeamHighlightColor

    protected override int VisualChildrenCount => 0;

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (!IsActive || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var outset = ResolveOutset();
        var outer = new Rect(
            -outset,
            -outset,
            ActualWidth + outset * 2,
            ActualHeight + outset * 2);

        var cornerRadius = ClampCornerRadius(outer, CornerRadius);
        var lineWidth = Math.Max(0.5, LineWidth);
        var ring = CreateRingGeometry(outer, cornerRadius, lineWidth);
        if (ring is null)
        {
            return;
        }

        drawingContext.PushClip(ring);

        var path = new RoundedRectPath(outer, cornerRadius);
        if (path.TotalLength <= 0)
        {
            drawingContext.Pop();
            return;
        }

        var beamSize = Math.Max(8, BeamSize);
        var durationSeconds = Math.Max(0.1, Duration.TotalSeconds);
        var beamCount = Math.Max(1, Count);

        for (var index = 0; index < beamCount; index++)
        {
            var phase = (_progress - (double)index / beamCount) % 1.0;
            if (phase < 0)
            {
                phase += 1.0;
            }

            var distance = phase * path.TotalLength;
            var (point, angle) = path.GetPointAndAngle(distance);
            DrawBeam(drawingContext, point, angle, beamSize);
        }

        drawingContext.Pop();

        _ = durationSeconds;
    }

    private void DrawBeam(DrawingContext drawingContext, Point anchor, double angleDegrees, double beamSize)
    {
        var gradient = new LinearGradientBrush
        {
            StartPoint = new Point(1, 0.5),
            EndPoint = new Point(0, 0.5),
            MappingMode = BrushMappingMode.RelativeToBoundingBox,
            GradientStops =
            {
                new GradientStop(BeamColor, 0),
                new GradientStop(BeamHighlightColor, MaxBeamColorStopPercent),
                new GradientStop(Color.FromArgb(0, BeamHighlightColor.R, BeamHighlightColor.G, BeamHighlightColor.B), 1),
            },
        };
        gradient.Freeze();

        drawingContext.PushTransform(new TranslateTransform(anchor.X, anchor.Y));
        drawingContext.PushTransform(new RotateTransform(angleDegrees));
        drawingContext.DrawRectangle(
            gradient,
            null,
            new Rect(-beamSize * 0.9, -beamSize * 0.5, beamSize, beamSize));
        drawingContext.Pop();
        drawingContext.Pop();
    }

    private double ResolveOutset()
    {
        if (!double.IsNaN(Outset))
        {
            return Math.Max(0, Outset);
        }

        var thickness = HostBorderThickness;
        return Math.Max(thickness.Left, Math.Max(thickness.Top, Math.Max(thickness.Right, thickness.Bottom)));
    }

    private static Geometry? CreateRingGeometry(Rect outer, CornerRadius cornerRadius, double lineWidth)
    {
        if (outer.Width <= 0 || outer.Height <= 0)
        {
            return null;
        }

        var inner = new Rect(
            outer.X + lineWidth,
            outer.Y + lineWidth,
            Math.Max(0, outer.Width - lineWidth * 2),
            Math.Max(0, outer.Height - lineWidth * 2));

        if (inner.Width <= 0 || inner.Height <= 0)
        {
            return CreateRoundedRectGeometry(outer, cornerRadius);
        }

        var innerRadius = new CornerRadius(
            Math.Max(0, cornerRadius.TopLeft - lineWidth),
            Math.Max(0, cornerRadius.TopRight - lineWidth),
            Math.Max(0, cornerRadius.BottomRight - lineWidth),
            Math.Max(0, cornerRadius.BottomLeft - lineWidth));

        var geometry = new CombinedGeometry(
            GeometryCombineMode.Exclude,
            CreateRoundedRectGeometry(outer, cornerRadius),
            CreateRoundedRectGeometry(inner, innerRadius));
        geometry.Freeze();
        return geometry;
    }

    private static Geometry CreateRoundedRectGeometry(Rect rect, CornerRadius cornerRadius)
    {
        var geometry = new PathGeometry();
        var figure = new PathFigure { IsClosed = true };

        var topLeft = Math.Min(cornerRadius.TopLeft, Math.Min(rect.Width, rect.Height) * 0.5);
        var topRight = Math.Min(cornerRadius.TopRight, Math.Min(rect.Width, rect.Height) * 0.5);
        var bottomRight = Math.Min(cornerRadius.BottomRight, Math.Min(rect.Width, rect.Height) * 0.5);
        var bottomLeft = Math.Min(cornerRadius.BottomLeft, Math.Min(rect.Width, rect.Height) * 0.5);

        figure.StartPoint = new Point(rect.Left + topLeft, rect.Top);

        figure.Segments.Add(new LineSegment(new Point(rect.Right - topRight, rect.Top), true));
        if (topRight > 0)
        {
            figure.Segments.Add(new ArcSegment(
                new Point(rect.Right, rect.Top + topRight),
                new Size(topRight, topRight),
                0,
                false,
                SweepDirection.Clockwise,
                true));
        }

        figure.Segments.Add(new LineSegment(new Point(rect.Right, rect.Bottom - bottomRight), true));
        if (bottomRight > 0)
        {
            figure.Segments.Add(new ArcSegment(
                new Point(rect.Right - bottomRight, rect.Bottom),
                new Size(bottomRight, bottomRight),
                0,
                false,
                SweepDirection.Clockwise,
                true));
        }

        figure.Segments.Add(new LineSegment(new Point(rect.Left + bottomLeft, rect.Bottom), true));
        if (bottomLeft > 0)
        {
            figure.Segments.Add(new ArcSegment(
                new Point(rect.Left, rect.Bottom - bottomLeft),
                new Size(bottomLeft, bottomLeft),
                0,
                false,
                SweepDirection.Clockwise,
                true));
        }

        figure.Segments.Add(new LineSegment(new Point(rect.Left, rect.Top + topLeft), true));
        if (topLeft > 0)
        {
            figure.Segments.Add(new ArcSegment(
                new Point(rect.Left + topLeft, rect.Top),
                new Size(topLeft, topLeft),
                0,
                false,
                SweepDirection.Clockwise,
                true));
        }

        geometry.Figures.Add(figure);
        geometry.Freeze();
        return geometry;
    }

    private static CornerRadius ClampCornerRadius(Rect rect, CornerRadius cornerRadius)
    {
        var maxRadius = Math.Min(rect.Width, rect.Height) * 0.5;
        return new CornerRadius(
            Math.Min(cornerRadius.TopLeft, maxRadius),
            Math.Min(cornerRadius.TopRight, maxRadius),
            Math.Min(cornerRadius.BottomRight, maxRadius),
            Math.Min(cornerRadius.BottomLeft, maxRadius));
    }

    private static object CoercePositive(DependencyObject d, object baseValue)
    {
        var value = (double)baseValue;
        return value > 0 ? value : 1.0;
    }

    private static object CoerceCount(DependencyObject d, object baseValue)
    {
        var value = (int)baseValue;
        return value >= 1 ? value : 1;
    }

    private static void OnAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BorderBeamIndicator)d).UpdateAnimationState();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!IsActive || Duration.TotalSeconds <= 0)
        {
            return;
        }

        var elapsed = (DateTime.UtcNow - _animationStart).TotalSeconds;
        _progress = (elapsed % Duration.TotalSeconds) / Duration.TotalSeconds;
        InvalidateVisual();
    }

    private void UpdateAnimationState()
    {
        if (IsLoaded && IsActive && IsVisible && Duration.TotalSeconds > 0)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        _animationStart = DateTime.UtcNow;
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    private void StopAnimation()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    private sealed class RoundedRectPath
    {
        private readonly double _topLeft;
        private readonly double _topRight;
        private readonly double _bottomRight;
        private readonly double _bottomLeft;
        private readonly double _left;
        private readonly double _top;
        private readonly double _right;
        private readonly double _bottom;
        private readonly double _topEdge;
        private readonly double _rightEdge;
        private readonly double _bottomEdge;
        private readonly double _leftEdge;
        private readonly double _topRightArc;
        private readonly double _bottomRightArc;
        private readonly double _bottomLeftArc;
        private readonly double _topLeftArc;

        public RoundedRectPath(Rect rect, CornerRadius cornerRadius)
        {
            _left = rect.Left;
            _top = rect.Top;
            _right = rect.Right;
            _bottom = rect.Bottom;

            var maxRadius = Math.Min(rect.Width, rect.Height) * 0.5;
            _topLeft = Math.Min(cornerRadius.TopLeft, maxRadius);
            _topRight = Math.Min(cornerRadius.TopRight, maxRadius);
            _bottomRight = Math.Min(cornerRadius.BottomRight, maxRadius);
            _bottomLeft = Math.Min(cornerRadius.BottomLeft, maxRadius);

            _topEdge = Math.Max(0, rect.Width - _topLeft - _topRight);
            _rightEdge = Math.Max(0, rect.Height - _topRight - _bottomRight);
            _bottomEdge = Math.Max(0, rect.Width - _bottomRight - _bottomLeft);
            _leftEdge = Math.Max(0, rect.Height - _bottomLeft - _topLeft);

            _topRightArc = _topRight * Math.PI * 0.5;
            _bottomRightArc = _bottomRight * Math.PI * 0.5;
            _bottomLeftArc = _bottomLeft * Math.PI * 0.5;
            _topLeftArc = _topLeft * Math.PI * 0.5;

            TotalLength = _topEdge + _rightEdge + _bottomEdge + _leftEdge
                + _topRightArc + _bottomRightArc + _bottomLeftArc + _topLeftArc;
        }

        public double TotalLength { get; }

        public (Point Point, double AngleDegrees) GetPointAndAngle(double distance)
        {
            if (TotalLength <= 0)
            {
                return (new Point(_left, _top), 0);
            }

            distance %= TotalLength;
            if (distance < 0)
            {
                distance += TotalLength;
            }

            if (distance <= _topEdge)
            {
                return (new Point(_left + _topLeft + distance, _top), 0);
            }

            distance -= _topEdge;

            if (distance <= _topRightArc)
            {
                return GetArcPoint(
                    centerX: _right - _topRight,
                    centerY: _top + _topRight,
                    radius: _topRight,
                    startAngle: -Math.PI * 0.5,
                    distance,
                    angleOffsetDegrees: 90);
            }

            distance -= _topRightArc;

            if (distance <= _rightEdge)
            {
                return (new Point(_right, _top + _topRight + distance), 90);
            }

            distance -= _rightEdge;

            if (distance <= _bottomRightArc)
            {
                return GetArcPoint(
                    centerX: _right - _bottomRight,
                    centerY: _bottom - _bottomRight,
                    radius: _bottomRight,
                    startAngle: 0,
                    distance,
                    angleOffsetDegrees: 90);
            }

            distance -= _bottomRightArc;

            if (distance <= _bottomEdge)
            {
                return (new Point(_right - _bottomRight - distance, _bottom), 180);
            }

            distance -= _bottomEdge;

            if (distance <= _bottomLeftArc)
            {
                return GetArcPoint(
                    centerX: _left + _bottomLeft,
                    centerY: _bottom - _bottomLeft,
                    radius: _bottomLeft,
                    startAngle: Math.PI * 0.5,
                    distance,
                    angleOffsetDegrees: 90);
            }

            distance -= _bottomLeftArc;

            if (distance <= _leftEdge)
            {
                return (new Point(_left, _bottom - _bottomLeft - distance), 270);
            }

            distance -= _leftEdge;

            return GetArcPoint(
                centerX: _left + _topLeft,
                centerY: _top + _topLeft,
                radius: _topLeft,
                startAngle: Math.PI,
                distance,
                angleOffsetDegrees: 90);
        }

        private static (Point Point, double AngleDegrees) GetArcPoint(
            double centerX,
            double centerY,
            double radius,
            double startAngle,
            double distance,
            double angleOffsetDegrees)
        {
            if (radius <= 0)
            {
                return (new Point(centerX, centerY), RadToDeg(startAngle) + angleOffsetDegrees);
            }

            var angle = startAngle + distance / radius;
            var point = new Point(
                centerX + radius * Math.Cos(angle),
                centerY + radius * Math.Sin(angle));
            return (point, RadToDeg(angle) + angleOffsetDegrees);
        }

        private static double RadToDeg(double radians) => radians * 180.0 / Math.PI;
    }
}
