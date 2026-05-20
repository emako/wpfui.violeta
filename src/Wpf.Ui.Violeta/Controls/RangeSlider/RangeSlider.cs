using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Indicates which thumb is currently "active" for keyboard navigation.
/// </summary>
public enum RangeSliderActiveThumb
{
    Lower,
    Upper,
}

/// <summary>
/// A WPF-UI Fluent styled range slider that exposes a <see cref="LowerValue"/> and an
/// <see cref="UpperValue"/> to represent a selected range on a continuous scale.
/// </summary>
[TemplatePart(Name = PART_Track, Type = typeof(RangeTrack))]
[TemplatePart(Name = PART_LowerThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_UpperThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_TopTick, Type = typeof(TickBar))]
[TemplatePart(Name = PART_BottomTick, Type = typeof(TickBar))]
[TemplatePart(Name = PART_LeftTick, Type = typeof(TickBar))]
[TemplatePart(Name = PART_RightTick, Type = typeof(TickBar))]
public class RangeSlider : Control
{
    // --- Part names -----------------------------------------------------------
    public const string PART_Track = nameof(PART_Track);

    public const string PART_LowerThumb = nameof(PART_LowerThumb);
    public const string PART_UpperThumb = nameof(PART_UpperThumb);
    public const string PART_TopTick = nameof(PART_TopTick);
    public const string PART_BottomTick = nameof(PART_BottomTick);
    public const string PART_LeftTick = nameof(PART_LeftTick);
    public const string PART_RightTick = nameof(PART_RightTick);

    // --- Template parts -------------------------------------------------------
    private RangeTrack? _track;

    private Thumb? _lowerThumb;
    private Thumb? _upperThumb;

    // State for drag handling
    private bool _isDraggingLower;

    private bool _isDraggingUpper;

    // Which thumb responds to keyboard arrow keys
    private RangeSliderActiveThumb _activeThumb = RangeSliderActiveThumb.Lower;

    // --- Constructor ----------------------------------------------------------

    static RangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider),
            new FrameworkPropertyMetadata(typeof(RangeSlider)));

        // Make RangeSlider keyboard-focusable
        KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(RangeSlider),
            new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
        KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(RangeSlider),
            new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
    }

    // --- Routed Events --------------------------------------------------------

    public static readonly RoutedEvent LowerValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(LowerValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(RangeSlider));

    /// <summary>Raised when <see cref="LowerValue"/> changes.</summary>
    public event RoutedPropertyChangedEventHandler<double> LowerValueChanged
    {
        add => AddHandler(LowerValueChangedEvent, value);
        remove => RemoveHandler(LowerValueChangedEvent, value);
    }

    public static readonly RoutedEvent UpperValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(UpperValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(RangeSlider));

    /// <summary>Raised when <see cref="UpperValue"/> changes.</summary>
    public event RoutedPropertyChangedEventHandler<double> UpperValueChanged
    {
        add => AddHandler(UpperValueChangedEvent, value);
        remove => RemoveHandler(UpperValueChangedEvent, value);
    }

    // --- Dependency Properties ------------------------------------------------

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnMinMaxChanged,
                CoerceMinimum));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100d,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnMinMaxChanged,
                CoerceMaximum));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty LowerValueProperty =
        DependencyProperty.Register(nameof(LowerValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnLowerValueChanged,
                CoerceLowerValue));

    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    public static readonly DependencyProperty UpperValueProperty =
        DependencyProperty.Register(nameof(UpperValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100d,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnUpperValueChanged,
                CoerceUpperValue));

    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RangeSlider),
            new FrameworkPropertyMetadata(Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(1d));

    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public static readonly DependencyProperty TickPlacementProperty =
        DependencyProperty.Register(nameof(TickPlacement), typeof(TickPlacement), typeof(RangeSlider),
            new FrameworkPropertyMetadata(TickPlacement.None));

    public TickPlacement TickPlacement
    {
        get => (TickPlacement)GetValue(TickPlacementProperty);
        set => SetValue(TickPlacementProperty, value);
    }

    public static readonly DependencyProperty IsSnapToTickEnabledProperty =
        DependencyProperty.Register(nameof(IsSnapToTickEnabled), typeof(bool), typeof(RangeSlider),
            new FrameworkPropertyMetadata(false));

    public bool IsSnapToTickEnabled
    {
        get => (bool)GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    public static readonly DependencyProperty SmallChangeProperty =
        DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(1d));

    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public static readonly DependencyProperty LargeChangeProperty =
        DependencyProperty.Register(nameof(LargeChange), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(10d));

    public double LargeChange
    {
        get => (double)GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(RangeSlider),
            new FrameworkPropertyMetadata(false));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    // --- Coerce callbacks -----------------------------------------------------

    private static object CoerceMinimum(DependencyObject d, object baseValue)
        => baseValue is double v ? Math.Min(v, ((RangeSlider)d).Maximum) : baseValue;

    private static object CoerceMaximum(DependencyObject d, object baseValue)
        => baseValue is double v ? Math.Max(v, ((RangeSlider)d).Minimum) : baseValue;

    private static object CoerceLowerValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        if (baseValue is double v)
            return Clamp(v, slider.Minimum, slider.UpperValue);
        return baseValue;
    }

    private static object CoerceUpperValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        if (baseValue is double v)
            return Clamp(v, slider.LowerValue, slider.Maximum);
        return baseValue;
    }

    // --- Changed callbacks ----------------------------------------------------

    private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        slider.CoerceValue(LowerValueProperty);
        slider.CoerceValue(UpperValueProperty);
    }

    private static void OnLowerValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        // Re-coerce UpperValue so it stays >= LowerValue
        slider.CoerceValue(UpperValueProperty);
        slider.RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, LowerValueChangedEvent));
    }

    private static void OnUpperValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        // Re-coerce LowerValue so it stays <= UpperValue
        slider.CoerceValue(LowerValueProperty);
        slider.RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, UpperValueChangedEvent));
    }

    // --- Template -------------------------------------------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Detach from old parts
        if (_lowerThumb != null)
        {
            _lowerThumb.DragStarted -= OnLowerThumbDragStarted;
            _lowerThumb.DragDelta -= OnLowerThumbDragDelta;
            _lowerThumb.DragCompleted -= OnLowerThumbDragCompleted;
            _lowerThumb.GotMouseCapture -= OnLowerThumbGotMouseCapture;
        }
        if (_upperThumb != null)
        {
            _upperThumb.DragStarted -= OnUpperThumbDragStarted;
            _upperThumb.DragDelta -= OnUpperThumbDragDelta;
            _upperThumb.DragCompleted -= OnUpperThumbDragCompleted;
            _upperThumb.GotMouseCapture -= OnUpperThumbGotMouseCapture;
        }
        _track?.MouseLeftButtonDown -= OnTrackMouseLeftButtonDown;

        _track = GetTemplateChild(PART_Track) as RangeTrack;
        _lowerThumb = GetTemplateChild(PART_LowerThumb) as Thumb;
        _upperThumb = GetTemplateChild(PART_UpperThumb) as Thumb;

        if (_lowerThumb != null)
        {
            _lowerThumb.DragStarted += OnLowerThumbDragStarted;
            _lowerThumb.DragDelta += OnLowerThumbDragDelta;
            _lowerThumb.DragCompleted += OnLowerThumbDragCompleted;
            _lowerThumb.GotMouseCapture += OnLowerThumbGotMouseCapture;
        }
        if (_upperThumb != null)
        {
            _upperThumb.DragStarted += OnUpperThumbDragStarted;
            _upperThumb.DragDelta += OnUpperThumbDragDelta;
            _upperThumb.DragCompleted += OnUpperThumbDragCompleted;
            _upperThumb.GotMouseCapture += OnUpperThumbGotMouseCapture;
        }
        _track?.MouseLeftButtonDown += OnTrackMouseLeftButtonDown;
    }

    // --- Drag: Lower Thumb ----------------------------------------------------

    private void OnLowerThumbGotMouseCapture(object sender, MouseEventArgs e)
        => _activeThumb = RangeSliderActiveThumb.Lower;

    private void OnLowerThumbDragStarted(object sender, DragStartedEventArgs e)
        => _isDraggingLower = true;

    private void OnLowerThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (IsReadOnly || _track == null) return;
        double delta = GetDeltaValue(e.HorizontalChange, e.VerticalChange);
        MoveLowerValue(delta);
    }

    private void OnLowerThumbDragCompleted(object sender, DragCompletedEventArgs e)
        => _isDraggingLower = false;

    // --- Drag: Upper Thumb ----------------------------------------------------

    private void OnUpperThumbGotMouseCapture(object sender, MouseEventArgs e)
        => _activeThumb = RangeSliderActiveThumb.Upper;

    private void OnUpperThumbDragStarted(object sender, DragStartedEventArgs e)
        => _isDraggingUpper = true;

    private void OnUpperThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (IsReadOnly || _track == null) return;
        double delta = GetDeltaValue(e.HorizontalChange, e.VerticalChange);
        MoveUpperValue(delta);
    }

    private void OnUpperThumbDragCompleted(object sender, DragCompletedEventArgs e)
        => _isDraggingUpper = false;

    // --- Track click ----------------------------------------------------------

    private void OnTrackMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsReadOnly || _track == null) return;

        // Don't handle if a thumb is being dragged
        if (_isDraggingLower || _isDraggingUpper) return;

        Point pos = e.GetPosition(_track);
        double clickValue = _track.GetValueFromPoint(pos);

        // Determine which thumb to move (nearest to click)
        double lowerDist = Math.Abs(clickValue - LowerValue);
        double upperDist = Math.Abs(clickValue - UpperValue);

        if (lowerDist <= upperDist)
        {
            SetLowerValue(clickValue);
            _activeThumb = RangeSliderActiveThumb.Lower;
        }
        else
        {
            SetUpperValue(clickValue);
            _activeThumb = RangeSliderActiveThumb.Upper;
        }

        Focus();
        e.Handled = true;
    }

    // --- Keyboard -------------------------------------------------------------

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsReadOnly || e.Handled) return;

        bool isHorizontal = Orientation == Orientation.Horizontal;

        switch (e.Key)
        {
            case Key.Left when isHorizontal:
            case Key.Down when !isHorizontal:
                MoveActiveThumb(-SmallChange);
                e.Handled = true;
                break;

            case Key.Right when isHorizontal:
            case Key.Up when !isHorizontal:
                MoveActiveThumb(SmallChange);
                e.Handled = true;
                break;

            case Key.PageDown:
                MoveActiveThumb(-LargeChange);
                e.Handled = true;
                break;

            case Key.PageUp:
                MoveActiveThumb(LargeChange);
                e.Handled = true;
                break;

            case Key.Home:
                if (_activeThumb == RangeSliderActiveThumb.Lower)
                    SetLowerValue(Minimum);
                else
                    SetUpperValue(LowerValue);
                e.Handled = true;
                break;

            case Key.End:
                if (_activeThumb == RangeSliderActiveThumb.Lower)
                    SetLowerValue(UpperValue);
                else
                    SetUpperValue(Maximum);
                e.Handled = true;
                break;

            case Key.Tab:
                // Switch active thumb on Tab
                _activeThumb = _activeThumb == RangeSliderActiveThumb.Lower
                    ? RangeSliderActiveThumb.Upper
                    : RangeSliderActiveThumb.Lower;
                e.Handled = true;
                break;
        }
    }

    // --- Value helpers --------------------------------------------------------

    private double GetDeltaValue(double horizontal, double vertical)
    {
        if (_track == null) return 0;

        double trackLength = Orientation == Orientation.Horizontal
            ? _track.ActualWidth
            : _track.ActualHeight;

        if (trackLength <= 0) return 0;

        double thumbSize = Orientation == Orientation.Horizontal
            ? (_lowerThumb?.ActualWidth ?? 0)
            : (_lowerThumb?.ActualHeight ?? 0);

        double usable = Math.Max(1, trackLength - thumbSize);
        double pixelDelta = Orientation == Orientation.Horizontal ? horizontal : -vertical;
        double valueDelta = pixelDelta / usable * (Maximum - Minimum);
        return valueDelta;
    }

    private void MoveActiveThumb(double delta)
    {
        if (_activeThumb == RangeSliderActiveThumb.Lower)
            MoveLowerValue(delta);
        else
            MoveUpperValue(delta);
    }

    private void MoveLowerValue(double delta)
    {
        double newValue = LowerValue + delta;
        SetLowerValue(newValue);
    }

    private void MoveUpperValue(double delta)
    {
        double newValue = UpperValue + delta;
        SetUpperValue(newValue);
    }

    private void SetLowerValue(double value)
    {
        if (IsSnapToTickEnabled)
            value = SnapToTick(value);
        value = Clamp(value, Minimum, UpperValue);
        LowerValue = value;
    }

    private void SetUpperValue(double value)
    {
        if (IsSnapToTickEnabled)
            value = SnapToTick(value);
        value = Clamp(value, LowerValue, Maximum);
        UpperValue = value;
    }

    private double SnapToTick(double value)
    {
        if (TickFrequency <= 0) return value;
        double prev = Minimum + Math.Floor((value - Minimum) / TickFrequency) * TickFrequency;
        double next = prev + TickFrequency;
        return (value - prev < next - value) ? prev : next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
            return min;

        return value > max ? max : value;
    }
}
