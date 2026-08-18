using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UiBorder = Wpf.Ui.Controls.Border;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// TextBox-style bottom accent (full-size <see cref="Border"/> with only bottom thickness)
/// clipped to the current progress window. The bar stays 1px track / 3px fill like ProgressBar;
/// corners come from the card radius on the full-size border, not from a short rectangle.
/// </summary>
public class CardProgressIndicator : FrameworkElement
{
    private const double DefaultFillThickness = 3.0;
    private const double TrackThickness = 1.0;

    private readonly Border _track;
    private readonly Border _fill;
    private readonly Border _indeterminate1;
    private readonly Border _indeterminate2;
    private readonly Border[] _borders;
    private Storyboard? _indeterminateStoryboard;

    private bool IsSettled => IsIndeterminate && (ShowError || ShowPaused);

    static CardProgressIndicator()
    {
        IsHitTestVisibleProperty.OverrideMetadata(
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(false));

        ClipToBoundsProperty.OverrideMetadata(
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(true));
    }

    public CardProgressIndicator()
    {
        _track = CreateAccentBorder(TrackThickness);
        _fill = CreateAccentBorder(DefaultFillThickness);
        _indeterminate1 = CreateAccentBorder(DefaultFillThickness);
        _indeterminate2 = CreateAccentBorder(DefaultFillThickness);
        _borders = [_track, _fill, _indeterminate1, _indeterminate2];

        foreach (var border in _borders)
        {
            AddVisualChild(border);
        }

        Loaded += (_, _) => UpdateIndeterminateAnimation();
        Unloaded += (_, _) => StopIndeterminateAnimation();
        SizeChanged += (_, _) =>
        {
            if (IsSettled)
            {
                IndeterminateOffset2 = GetSettledOffset(ActualWidth);
                InvalidateArrange();
                return;
            }

            ApplyClips();
            UpdateIndeterminateAnimation();
        };
    }

    private static Border CreateAccentBorder(double bottomThickness)
    {
        return new Border
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0, 0, 0, bottomThickness),
            IsHitTestVisible = false,
            SnapsToDevicePixels = true,
        };
    }

    #region Value / Range

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(0.0, OnClipChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(0.0, OnClipChanged));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(100.0, OnClipChanged));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion

    #region State

    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(
            nameof(IsIndeterminate),
            typeof(bool),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(false, OnIndeterminateChanged));

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public static readonly DependencyProperty ShowErrorProperty =
        DependencyProperty.Register(
            nameof(ShowError),
            typeof(bool),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(false, OnPausedOrErrorChanged));

    public bool ShowError
    {
        get => (bool)GetValue(ShowErrorProperty);
        set => SetValue(ShowErrorProperty, value);
    }

    public static readonly DependencyProperty ShowPausedProperty =
        DependencyProperty.Register(
            nameof(ShowPaused),
            typeof(bool),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(false, OnPausedOrErrorChanged));

    public bool ShowPaused
    {
        get => (bool)GetValue(ShowPausedProperty);
        set => SetValue(ShowPausedProperty, value);
    }

    #endregion

    #region Appearance

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(new CornerRadius(0), OnAppearanceChanged));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty IndicatorThicknessProperty =
        DependencyProperty.Register(
            nameof(IndicatorThickness),
            typeof(double),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(DefaultFillThickness, OnAppearanceChanged));

    public double IndicatorThickness
    {
        get => (double)GetValue(IndicatorThicknessProperty);
        set => SetValue(IndicatorThicknessProperty, value);
    }

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(
            nameof(TrackBrush),
            typeof(Brush),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

    public Brush? TrackBrush
    {
        get => (Brush?)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public static readonly DependencyProperty FillBrushProperty =
        DependencyProperty.Register(
            nameof(FillBrush),
            typeof(Brush),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(null, OnAppearanceChanged));

    public Brush? FillBrush
    {
        get => (Brush?)GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    #endregion

    #region Animation offsets

    public static readonly DependencyProperty IndeterminateOffset1Property =
        DependencyProperty.Register(
            nameof(IndeterminateOffset1),
            typeof(double),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(0.0, OnClipChanged));

    public double IndeterminateOffset1
    {
        get => (double)GetValue(IndeterminateOffset1Property);
        set => SetValue(IndeterminateOffset1Property, value);
    }

    public static readonly DependencyProperty IndeterminateOffset2Property =
        DependencyProperty.Register(
            nameof(IndeterminateOffset2),
            typeof(double),
            typeof(CardProgressIndicator),
            new FrameworkPropertyMetadata(0.0, OnClipChanged));

    public double IndeterminateOffset2
    {
        get => (double)GetValue(IndeterminateOffset2Property);
        set => SetValue(IndeterminateOffset2Property, value);
    }

    #endregion

    protected override int VisualChildrenCount => _borders.Length;

    protected override Visual GetVisualChild(int index) => _borders[index];

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = new Size(
            double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);

        foreach (var border in _borders)
        {
            border.Measure(size);
        }

        if (IsSettled)
        {
            _indeterminate2.Measure(GetSettledRect(size).Size);
        }

        return size;
    }

    protected override Geometry? GetLayoutClip(Size layoutSlotSize)
    {
        return UiBorder.CalculateLayoutClip(layoutSlotSize, new Thickness(0), CornerRadius);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        ApplyAppearance();
        ArrangeChildren(finalSize);
        ApplyClips();
        return finalSize;
    }

    private void ArrangeChildren(Size size)
    {
        var full = new Rect(size);
        _track.Arrange(full);
        _fill.Arrange(full);
        _indeterminate1.Arrange(full);
        _indeterminate2.Arrange(IsSettled ? GetSettledRect(size) : full);
    }

    private Rect GetSettledRect(Size size)
    {
        var thickness = IndicatorThickness;
        var width = Math.Max(0, size.Width * 0.6);
        var y = Math.Max(0, size.Height - thickness);
        return new Rect(IndeterminateOffset2, y, width, thickness);
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (CardProgressIndicator)d;
        self.ApplyAppearance();
        self.InvalidateArrange();
    }

    private static void OnClipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (CardProgressIndicator)d;
        if (self.IsSettled)
        {
            self.InvalidateArrange();
            return;
        }

        self.ApplyClips();
    }

    private static void OnIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (CardProgressIndicator)d;
        self.InvalidateArrange();
        self.UpdateIndeterminateAnimation();
    }

    private static void OnPausedOrErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (CardProgressIndicator)d;
        self.InvalidateArrange();
        self.UpdateIndeterminateAnimation();
    }

    private void ApplyAppearance()
    {
        var radius = CornerRadius;
        var fillThickness = new Thickness(0, 0, 0, IndicatorThickness);
        var fillBrush = ResolveFillBrush();
        var indeterminate = IsIndeterminate;
        var settled = IsSettled;
        var pillRadius = new CornerRadius(IndicatorThickness / 2.0);

        _track.CornerRadius = radius;
        _track.BorderBrush = TrackBrush;
        _track.Visibility = indeterminate ? Visibility.Collapsed : Visibility.Visible;

        _fill.CornerRadius = radius;
        _fill.BorderThickness = fillThickness;
        _fill.BorderBrush = fillBrush;
        _fill.Visibility = indeterminate ? Visibility.Collapsed : Visibility.Visible;

        _indeterminate1.CornerRadius = radius;
        _indeterminate1.BorderThickness = fillThickness;
        _indeterminate1.BorderBrush = fillBrush;
        _indeterminate1.Visibility = indeterminate && !settled ? Visibility.Visible : Visibility.Collapsed;

        _indeterminate2.Visibility = indeterminate ? Visibility.Visible : Visibility.Collapsed;
        if (settled)
        {
            _indeterminate2.Background = fillBrush;
            _indeterminate2.BorderBrush = null;
            _indeterminate2.BorderThickness = new Thickness(0);
            _indeterminate2.CornerRadius = pillRadius;
            _indeterminate2.Clip = null;
        }
        else
        {
            _indeterminate2.Background = Brushes.Transparent;
            _indeterminate2.BorderBrush = fillBrush;
            _indeterminate2.BorderThickness = fillThickness;
            _indeterminate2.CornerRadius = radius;
        }
    }

    private void ApplyClips()
    {
        var width = RenderSize.Width;
        var height = RenderSize.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (IsIndeterminate)
        {
            _fill.Clip = null;
            if (IsSettled)
            {
                _indeterminate1.Clip = null;
                _indeterminate2.Clip = null;
                return;
            }

            _indeterminate1.Clip = new RectangleGeometry(new Rect(IndeterminateOffset1, 0, width * 0.4, height));
            _indeterminate2.Clip = new RectangleGeometry(new Rect(IndeterminateOffset2, 0, width * 0.6, height));
            return;
        }

        _indeterminate1.Clip = null;
        _indeterminate2.Clip = null;
        _fill.Clip = new RectangleGeometry(new Rect(0, 0, GetProgressWidth(width), height));
    }

    private Brush? ResolveFillBrush()
    {
        if (ShowError && TryFindResource("SystemFillColorCriticalBrush") is Brush error)
        {
            return error;
        }

        if (ShowPaused && TryFindResource("SystemFillColorCautionBrush") is Brush paused)
        {
            return paused;
        }

        return FillBrush;
    }

    private double GetProgressWidth(double width)
    {
        var range = Maximum - Minimum;
        if (range <= double.Epsilon || width <= 0)
        {
            return 0;
        }

        var ratio = (Value - Minimum) / range;
        if (ratio < 0)
        {
            ratio = 0;
        }
        else if (ratio > 1)
        {
            ratio = 1;
        }

        return width * ratio;
    }

    private static double GetSettledOffset(double width) => width * 0.2;

    private void UpdateIndeterminateAnimation()
    {
        var currentOffset2 = IndeterminateOffset2;
        StopIndeterminateAnimation();

        if (!IsIndeterminate || !IsLoaded || ActualWidth <= 0)
        {
            return;
        }

        var width = ActualWidth;
        if (ShowError || ShowPaused)
        {
            BeginSettledAnimation(currentOffset2, GetSettledOffset(width));
            return;
        }

        var width1 = width * 0.4;
        var width2 = width * 0.6;
        var spline = new KeySpline(0.4, 0, 0.6, 1);

        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        var first = new DoubleAnimationUsingKeyFrames();
        Storyboard.SetTarget(first, this);
        Storyboard.SetTargetProperty(first, new PropertyPath(IndeterminateOffset1Property));
        first.KeyFrames.Add(new DiscreteDoubleKeyFrame(width1 * -1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        first.KeyFrames.Add(new SplineDoubleKeyFrame(width1 * 3.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5))) { KeySpline = spline });
        first.KeyFrames.Add(new DiscreteDoubleKeyFrame(width1 * 3.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
        storyboard.Children.Add(first);

        var second = new DoubleAnimationUsingKeyFrames();
        Storyboard.SetTarget(second, this);
        Storyboard.SetTargetProperty(second, new PropertyPath(IndeterminateOffset2Property));
        second.KeyFrames.Add(new DiscreteDoubleKeyFrame(width2 * -1.5, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        second.KeyFrames.Add(new DiscreteDoubleKeyFrame(width2 * -1.5, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.75))));
        second.KeyFrames.Add(new SplineDoubleKeyFrame(width2 * 1.66, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))) { KeySpline = spline });
        storyboard.Children.Add(second);

        _indeterminateStoryboard = storyboard;
        storyboard.Begin(this, isControllable: true);
    }

    private void BeginSettledAnimation(double from, double to)
    {
        var spline = new KeySpline(0.0, 0.0, 0.0, 1.0);
        var storyboard = new Storyboard { FillBehavior = FillBehavior.HoldEnd };

        var move = new DoubleAnimationUsingKeyFrames();
        Storyboard.SetTarget(move, this);
        Storyboard.SetTargetProperty(move, new PropertyPath(IndeterminateOffset2Property));
        move.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        move.KeyFrames.Add(new SplineDoubleKeyFrame(to, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.75))) { KeySpline = spline });
        storyboard.Children.Add(move);

        _indeterminateStoryboard = storyboard;
        storyboard.Begin(this, isControllable: true);
    }

    private void StopIndeterminateAnimation()
    {
        if (_indeterminateStoryboard == null)
        {
            return;
        }

        _indeterminateStoryboard.Stop(this);
        _indeterminateStoryboard.Remove(this);
        _indeterminateStoryboard = null;
    }
}
