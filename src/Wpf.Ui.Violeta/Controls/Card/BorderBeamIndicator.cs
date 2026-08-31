using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Renders an animated gradient beam traveling along a rounded-rectangle border ring.
/// </summary>
public class BorderBeamIndicator : FrameworkElement
{
    private const double MaxBeamColorStopPercent = 0.7;

    private long _animationStartTimestamp;
    private double _progress;
    private bool _isRendering;

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

        // Stroke along the ring centerline so the beam follows rounded corners continuously
        // instead of revealing a rotated square through a ring mask (which breaks at corners).
        var inset = lineWidth * 0.5;
        var strokeRect = new Rect(
            outer.X + inset,
            outer.Y + inset,
            Math.Max(0, outer.Width - lineWidth),
            Math.Max(0, outer.Height - lineWidth));

        if (strokeRect.Width <= 0 || strokeRect.Height <= 0)
        {
            return;
        }

        var strokeRadius = new CornerRadius(
            Math.Max(0, cornerRadius.TopLeft - inset),
            Math.Max(0, cornerRadius.TopRight - inset),
            Math.Max(0, cornerRadius.BottomRight - inset),
            Math.Max(0, cornerRadius.BottomLeft - inset));

        var path = new RoundedRectPath(strokeRect, strokeRadius);
        if (path.TotalLength <= 0)
        {
            return;
        }

        var beamSize = Math.Max(8, BeamSize);
        var beamCount = Math.Max(1, Count);

        for (var index = 0; index < beamCount; index++)
        {
            var phase = (_progress - (double)index / beamCount) % 1.0;
            if (phase < 0)
            {
                phase += 1.0;
            }

            DrawBeamAlongPath(
                drawingContext,
                path,
                headDistance: phase * path.TotalLength,
                beamLength: beamSize,
                strokeWidth: lineWidth);
        }
    }

    private void DrawBeamAlongPath(
        DrawingContext drawingContext,
        RoundedRectPath path,
        double headDistance,
        double beamLength,
        double strokeWidth)
    {
        var totalLength = path.TotalLength;
        if (totalLength <= 0 || beamLength <= 0)
        {
            return;
        }

        // Keep a visible gap so the beam never covers the entire perimeter.
        beamLength = Math.Min(beamLength, totalLength * 0.85);

        var sampleCount = Math.Max(48, (int)Math.Ceiling(beamLength));
        var points = new Point[sampleCount + 1];
        for (var i = 0; i <= sampleCount; i++)
        {
            // t = 0 at the opaque head, t = 1 at the transparent tail.
            var t = (double)i / sampleCount;
            var distance = NormalizeDistance(headDistance - t * beamLength, totalLength);
            points[i] = path.GetPointAndAngle(distance).Point;
        }

        // Draw short bands so the gradient follows the path through corners
        // (a single Absolute LinearGradientBrush would cut across the chord).
        const int bandCount = 16;
        for (var band = 0; band < bandCount; band++)
        {
            var t0 = (double)band / bandCount;
            var t1 = (double)(band + 1) / bandCount;
            var color = SampleBeamColor((t0 + t1) * 0.5);
            if (color.A == 0)
            {
                continue;
            }

            var startIndex = (int)Math.Round(t0 * sampleCount);
            var endIndex = Math.Max(startIndex + 1, (int)Math.Round(t1 * sampleCount));
            endIndex = Math.Min(endIndex, sampleCount);

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(points[startIndex], false, false);
                for (var i = startIndex + 1; i <= endIndex; i++)
                {
                    ctx.LineTo(points[i], true, true);
                }
            }

            geometry.Freeze();

            var brush = new SolidColorBrush(color);
            brush.Freeze();
            var pen = new Pen(brush, strokeWidth)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round,
            };
            pen.Freeze();

            drawingContext.DrawGeometry(null, pen, geometry);
        }
    }

    private Color SampleBeamColor(double t)
    {
        t = Clamp01(t);
        var beamColor = BeamColor;
        var highlightColor = BeamHighlightColor;
        var transparent = Color.FromArgb(0, highlightColor.R, highlightColor.G, highlightColor.B);

        if (t <= MaxBeamColorStopPercent)
        {
            var local = MaxBeamColorStopPercent <= 0 ? 0 : t / MaxBeamColorStopPercent;
            return LerpColor(beamColor, highlightColor, local);
        }

        var fade = (t - MaxBeamColorStopPercent) / (1.0 - MaxBeamColorStopPercent);
        return LerpColor(highlightColor, transparent, fade);
    }

    private static Color LerpColor(Color from, Color to, double t)
    {
        t = Clamp01(t);
        return Color.FromArgb(
            (byte)(from.A + (to.A - from.A) * t),
            (byte)(from.R + (to.R - from.R) * t),
            (byte)(from.G + (to.G - from.G) * t),
            (byte)(from.B + (to.B - from.B) * t));
    }

    private static double Clamp01(double value) => value < 0 ? 0 : value > 1 ? 1 : value;

    private static double NormalizeDistance(double distance, double totalLength)
    {
        if (totalLength <= 0)
        {
            return 0;
        }

        distance %= totalLength;
        if (distance < 0)
        {
            distance += totalLength;
        }

        return distance;
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
        var indicator = (BorderBeamIndicator)d;
        if (e.Property == DurationProperty && indicator._isRendering && e.OldValue is TimeSpan oldDuration)
        {
            // Keep the current phase when duration changes so the beam does not jump.
            var oldSeconds = Math.Max(0.1, oldDuration.TotalSeconds);
            var newSeconds = Math.Max(0.1, indicator.Duration.TotalSeconds);
            var elapsed = (Stopwatch.GetTimestamp() - indicator._animationStartTimestamp) / (double)Stopwatch.Frequency;
            var phase = (elapsed % oldSeconds) / oldSeconds;
            indicator._animationStartTimestamp =
                Stopwatch.GetTimestamp() - (long)(phase * newSeconds * Stopwatch.Frequency);
        }

        indicator.UpdateAnimationState();
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (!IsActive)
        {
            return;
        }

        var durationSeconds = Math.Max(0.1, Duration.TotalSeconds);
        var elapsed = (Stopwatch.GetTimestamp() - _animationStartTimestamp) / (double)Stopwatch.Frequency;
        var nextProgress = (elapsed % durationSeconds) / durationSeconds;

        // Skip no-op frames (e.g. duplicate Rendering callbacks) to avoid redundant redraws.
        if (Math.Abs(nextProgress - _progress) < 1e-9)
        {
            return;
        }

        _progress = nextProgress;
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
        if (_isRendering)
        {
            return;
        }

        // Resume from the last known phase instead of snapping back to the origin.
        var durationSeconds = Math.Max(0.1, Duration.TotalSeconds);
        _animationStartTimestamp =
            Stopwatch.GetTimestamp() - (long)(_progress * durationSeconds * Stopwatch.Frequency);
        CompositionTarget.Rendering += OnRendering;
        _isRendering = true;
    }

    private void StopAnimation()
    {
        if (!_isRendering)
        {
            return;
        }

        CompositionTarget.Rendering -= OnRendering;
        _isRendering = false;
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
