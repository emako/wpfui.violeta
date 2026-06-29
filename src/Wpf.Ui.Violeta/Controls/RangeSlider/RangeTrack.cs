using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A layout element for <see cref="RangeSlider"/> that positions two thumbs and a selection-range
/// highlight.  The three visual children are (in z-order):
/// <list type="number">
///   <item><see cref="SelectionBorder"/> – the accent-coloured bar between the two thumbs.</item>
///   <item><see cref="LowerThumb"/> – the thumb for the lower value.</item>
///   <item><see cref="UpperThumb"/> – the thumb for the upper value.</item>
/// </list>
/// </summary>
public class RangeTrack : FrameworkElement
{
    // --- Visual children ------------------------------------------------------

    private readonly Border _selectionBorder = new() { IsHitTestVisible = false };
    private Thumb? _lowerThumb;
    private Thumb? _upperThumb;

    protected override int VisualChildrenCount
    {
        get
        {
            int count = 1; // selectionBorder is always present
            if (_lowerThumb != null) count++;
            if (_upperThumb != null) count++;
            return count;
        }
    }

    protected override Visual GetVisualChild(int index)
    {
        return index switch
        {
            0 => _selectionBorder,
            1 => _lowerThumb ?? throw new ArgumentOutOfRangeException(nameof(index)),
            2 => _upperThumb ?? throw new ArgumentOutOfRangeException(nameof(index)),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    // --- Constructor ----------------------------------------------------------

    static RangeTrack()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeTrack),
            new FrameworkPropertyMetadata(typeof(RangeTrack)));
    }

    public RangeTrack()
    {
        AddVisualChild(_selectionBorder);
        AddLogicalChild(_selectionBorder);
    }

    // --- Dependency Properties ------------------------------------------------

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeTrack),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeTrack),
            new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty LowerValueProperty =
        DependencyProperty.Register(nameof(LowerValue), typeof(double), typeof(RangeTrack),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    public static readonly DependencyProperty UpperValueProperty =
        DependencyProperty.Register(nameof(UpperValue), typeof(double), typeof(RangeTrack),
            new FrameworkPropertyMetadata(100d,
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RangeTrack),
            new FrameworkPropertyMetadata(Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty SelectionRangeFillProperty =
        DependencyProperty.Register(nameof(SelectionRangeFill), typeof(Brush), typeof(RangeTrack),
            new FrameworkPropertyMetadata(null, OnSelectionRangeFillChanged));

    public Brush? SelectionRangeFill
    {
        get => (Brush?)GetValue(SelectionRangeFillProperty);
        set => SetValue(SelectionRangeFillProperty, value);
    }

    private static void OnSelectionRangeFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RangeTrack)d)._selectionBorder.Background = (Brush?)e.NewValue;
    }

    public static readonly DependencyProperty LowerThumbProperty =
        DependencyProperty.Register(nameof(LowerThumb), typeof(Thumb), typeof(RangeTrack),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnLowerThumbChanged));

    public Thumb? LowerThumb
    {
        get => (Thumb?)GetValue(LowerThumbProperty);
        set => SetValue(LowerThumbProperty, value);
    }

    private static void OnLowerThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var track = (RangeTrack)d;
        if (e.OldValue is Thumb old)
        {
            track.RemoveVisualChild(old);
            track.RemoveLogicalChild(old);
            track._lowerThumb = null;
        }
        if (e.NewValue is Thumb newThumb)
        {
            track._lowerThumb = newThumb;
            track.AddVisualChild(newThumb);
            track.AddLogicalChild(newThumb);
        }
    }

    public static readonly DependencyProperty UpperThumbProperty =
        DependencyProperty.Register(nameof(UpperThumb), typeof(Thumb), typeof(RangeTrack),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnUpperThumbChanged));

    public Thumb? UpperThumb
    {
        get => (Thumb?)GetValue(UpperThumbProperty);
        set => SetValue(UpperThumbProperty, value);
    }

    private static void OnUpperThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var track = (RangeTrack)d;
        if (e.OldValue is Thumb old)
        {
            track.RemoveVisualChild(old);
            track.RemoveLogicalChild(old);
            track._upperThumb = null;
        }
        if (e.NewValue is Thumb newThumb)
        {
            track._upperThumb = newThumb;
            track.AddVisualChild(newThumb);
            track.AddLogicalChild(newThumb);
        }
    }

    // --- Measure / Arrange ----------------------------------------------------

    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = new Size();
        _selectionBorder.Measure(availableSize);

        if (_lowerThumb != null)
        {
            _lowerThumb.Measure(availableSize);
            desired = new Size(
                Math.Max(desired.Width, _lowerThumb.DesiredSize.Width),
                Math.Max(desired.Height, _lowerThumb.DesiredSize.Height));
        }
        if (_upperThumb != null)
        {
            _upperThumb.Measure(availableSize);
            desired = new Size(
                Math.Max(desired.Width, _upperThumb.DesiredSize.Width),
                Math.Max(desired.Height, _upperThumb.DesiredSize.Height));
        }
        return desired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Orientation == Orientation.Horizontal)
            ArrangeHorizontal(finalSize);
        else
            ArrangeVertical(finalSize);

        return finalSize;
    }

    private void ArrangeHorizontal(Size finalSize)
    {
        double thumbW = Math.Max(
            _lowerThumb?.DesiredSize.Width ?? 0,
            _upperThumb?.DesiredSize.Width ?? 0);
        double thumbH = Math.Max(
            _lowerThumb?.DesiredSize.Height ?? 0,
            _upperThumb?.DesiredSize.Height ?? 0);

        double usable = Math.Max(0, finalSize.Width - thumbW);
        double range = Maximum - Minimum;

        double lowerRatio = range <= 0 ? 0 : (LowerValue - Minimum) / range;
        double upperRatio = range <= 0 ? 1 : (UpperValue - Minimum) / range;

        double lowerLeft = lowerRatio * usable;
        double upperLeft = upperRatio * usable;

        double thumbTop = Math.Max(0, (finalSize.Height - thumbH) / 2);

        _lowerThumb?.Arrange(new Rect(lowerLeft, thumbTop, thumbW, thumbH));
        _upperThumb?.Arrange(new Rect(upperLeft, thumbTop, thumbW, thumbH));

        // selection range: spans from centre of lower thumb to centre of upper thumb
        double selLeft = lowerLeft + thumbW / 2;
        double selWidth = Math.Max(0, upperLeft - lowerLeft);
        const double selH = 4;
        double selTop = Math.Max(0, (finalSize.Height - selH) / 2);
        _selectionBorder.Arrange(new Rect(selLeft, selTop, selWidth, selH));
    }

    private void ArrangeVertical(Size finalSize)
    {
        double thumbW = Math.Max(
            _lowerThumb?.DesiredSize.Width ?? 0,
            _upperThumb?.DesiredSize.Width ?? 0);
        double thumbH = Math.Max(
            _lowerThumb?.DesiredSize.Height ?? 0,
            _upperThumb?.DesiredSize.Height ?? 0);

        double usable = Math.Max(0, finalSize.Height - thumbH);
        double range = Maximum - Minimum;

        // For vertical: lower value → bottom, upper value → top.
        double lowerRatio = range <= 0 ? 0 : (LowerValue - Minimum) / range;
        double upperRatio = range <= 0 ? 1 : (UpperValue - Minimum) / range;

        // Invert: low value is at the bottom (large top offset)
        double lowerTop = (1 - lowerRatio) * usable;
        double upperTop = (1 - upperRatio) * usable;

        double thumbLeft = Math.Max(0, (finalSize.Width - thumbW) / 2);

        _lowerThumb?.Arrange(new Rect(thumbLeft, lowerTop, thumbW, thumbH));
        _upperThumb?.Arrange(new Rect(thumbLeft, upperTop, thumbW, thumbH));

        // selection range: from centre of upper thumb to centre of lower thumb
        double selTop = upperTop + thumbH / 2;
        double selHeight = Math.Max(0, lowerTop - upperTop);
        const double selW = 4;
        double selLeft = Math.Max(0, (finalSize.Width - selW) / 2);
        _selectionBorder.Arrange(new Rect(selLeft, selTop, selW, selHeight));
    }

    // --- Hit testing helpers --------------------------------------------------

    /// <summary>
    /// Converts a point (in RangeTrack local coordinates) to a slider value.
    /// </summary>
    public double GetValueFromPoint(Point point)
    {
        double thumbW = Math.Max(
            _lowerThumb?.DesiredSize.Width ?? 0,
            _upperThumb?.DesiredSize.Width ?? 0);
        double thumbH = Math.Max(
            _lowerThumb?.DesiredSize.Height ?? 0,
            _upperThumb?.DesiredSize.Height ?? 0);

        double ratio;
        if (Orientation == Orientation.Horizontal)
        {
            double usable = Math.Max(1, ActualWidth - thumbW);
            double x = Clamp(point.X - thumbW / 2, 0, usable);
            ratio = x / usable;
        }
        else
        {
            double usable = Math.Max(1, ActualHeight - thumbH);
            double y = Clamp(point.Y - thumbH / 2, 0, usable);
            ratio = 1.0 - y / usable; // inverted for vertical
        }

        return Minimum + ratio * (Maximum - Minimum);
    }

    /// <summary>
    /// Returns the pixel position of the centre of the lower thumb (in local coords).
    /// </summary>
    internal double GetLowerThumbCentre()
    {
        if (_lowerThumb == null) return 0;
        if (Orientation == Orientation.Horizontal)
            return _lowerThumb.TranslatePoint(new Point(_lowerThumb.ActualWidth / 2, 0), this).X;
        else
            return _lowerThumb.TranslatePoint(new Point(0, _lowerThumb.ActualHeight / 2), this).Y;
    }

    /// <summary>
    /// Returns the pixel position of the centre of the upper thumb (in local coords).
    /// </summary>
    internal double GetUpperThumbCentre()
    {
        if (_upperThumb == null) return 0;
        if (Orientation == Orientation.Horizontal)
            return _upperThumb.TranslatePoint(new Point(_upperThumb.ActualWidth / 2, 0), this).X;
        else
            return _upperThumb.TranslatePoint(new Point(0, _upperThumb.ActualHeight / 2), this).Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
            return min;

        return value > max ? max : value;
    }
}
