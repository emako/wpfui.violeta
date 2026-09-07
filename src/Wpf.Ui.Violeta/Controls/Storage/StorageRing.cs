using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Windows Settings style capacity ring.
/// </summary>
[ContentProperty(nameof(Content))]
[TemplatePart(Name = PartRoot, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartTrackRing, Type = typeof(Arc))]
[TemplatePart(Name = PartValueRing, Type = typeof(Arc))]
public class StorageRing : RangeBase
{
    private const string PartRoot = "PART_Root";
    private const string PartTrackRing = "PART_TrackRing";
    private const string PartValueRing = "PART_ValueRing";

    private FrameworkElement? _root;
    private Arc? _trackRing;
    private Arc? _valueRing;

    static StorageRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StorageRing),
            new FrameworkPropertyMetadata(typeof(StorageRing)));

        FocusableProperty.OverrideMetadata(
            typeof(StorageRing),
            new FrameworkPropertyMetadata(false));

        IsTabStopProperty.OverrideMetadata(
            typeof(StorageRing),
            new FrameworkPropertyMetadata(false));
    }

    public StorageRing()
    {
        IsEnabledChanged += (_, _) => ApplyAppearance();
    }

    #region Content

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(StorageRing),
            new PropertyMetadata(null));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion

    #region ContentTemplate

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(
            nameof(ContentTemplate),
            typeof(DataTemplate),
            typeof(StorageRing),
            new PropertyMetadata(null));

    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    #endregion

    #region Percent

    private static readonly DependencyPropertyKey PercentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Percent),
            typeof(double),
            typeof(StorageRing),
            new PropertyMetadata(0d));

    public static readonly DependencyProperty PercentProperty = PercentPropertyKey.DependencyProperty;

    public double Percent => (double)GetValue(PercentProperty);

    #endregion

    #region ValueAngle

    private static readonly DependencyPropertyKey ValueAnglePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ValueAngle),
            typeof(double),
            typeof(StorageRing),
            new PropertyMetadata(0d));

    public static readonly DependencyProperty ValueAngleProperty = ValueAnglePropertyKey.DependencyProperty;

    public double ValueAngle => (double)GetValue(ValueAngleProperty);

    #endregion

    #region PercentCaution / PercentCritical

    public static readonly DependencyProperty PercentCautionProperty =
        DependencyProperty.Register(
            nameof(PercentCaution),
            typeof(double),
            typeof(StorageRing),
            new PropertyMetadata(75d, OnAppearanceChanged));

    public double PercentCaution
    {
        get => (double)GetValue(PercentCautionProperty);
        set => SetValue(PercentCautionProperty, value);
    }

    public static readonly DependencyProperty PercentCriticalProperty =
        DependencyProperty.Register(
            nameof(PercentCritical),
            typeof(double),
            typeof(StorageRing),
            new PropertyMetadata(90d, OnAppearanceChanged));

    public double PercentCritical
    {
        get => (double)GetValue(PercentCriticalProperty);
        set => SetValue(PercentCriticalProperty, value);
    }

    #endregion

    #region Thickness

    public static readonly DependencyProperty ValueRingThicknessProperty =
        DependencyProperty.Register(
            nameof(ValueRingThickness),
            typeof(double),
            typeof(StorageRing),
            new PropertyMetadata(6d));

    public double ValueRingThickness
    {
        get => (double)GetValue(ValueRingThicknessProperty);
        set => SetValue(ValueRingThicknessProperty, value);
    }

    public static readonly DependencyProperty TrackRingThicknessProperty =
        DependencyProperty.Register(
            nameof(TrackRingThickness),
            typeof(double),
            typeof(StorageRing),
            new PropertyMetadata(6d));

    public double TrackRingThickness
    {
        get => (double)GetValue(TrackRingThicknessProperty);
        set => SetValue(TrackRingThicknessProperty, value);
    }

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = GetTemplateChild(PartRoot) as FrameworkElement;
        _trackRing = GetTemplateChild(PartTrackRing) as Arc;
        _valueRing = GetTemplateChild(PartValueRing) as Arc;
        UpdatePercentAndAngle();
        ApplyAppearance();
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdatePercentAndAngle();
        ApplyAppearance();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        UpdatePercentAndAngle();
        ApplyAppearance();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        UpdatePercentAndAngle();
        ApplyAppearance();
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StorageRing ring)
        {
            ring.ApplyAppearance();
        }
    }

    private void UpdatePercentAndAngle()
    {
        double range = Maximum - Minimum;
        double percent = range <= 0
            ? 0
            : Clamp((Value - Minimum) / range * 100d, 0d, 100d);
        SetValue(PercentPropertyKey, percent);

        double angle = percent * 3.6d;
        if (angle >= 360d)
        {
            angle = 359.9d;
        }

        SetValue(ValueAnglePropertyKey, angle);

        if (_valueRing is not null)
        {
            _valueRing.EndAngle = angle;
            _valueRing.Visibility = percent <= 0.01 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void ApplyAppearance()
    {
        if (_root is not null)
        {
            _root.Opacity = IsEnabled ? 1d : 0.4d;
        }

        if (_valueRing is null || _trackRing is null)
        {
            return;
        }

        string valueKey;
        if (Percent >= PercentCritical)
        {
            valueKey = "SystemFillColorCriticalBrush";
        }
        else if (Percent >= PercentCaution)
        {
            valueKey = "SystemFillColorCautionBrush";
        }
        else
        {
            valueKey = "SystemAccentColorPrimaryBrush";
        }

        _valueRing.SetResourceReference(Shape.StrokeProperty, valueKey);
        _trackRing.SetResourceReference(Shape.StrokeProperty, "ControlStrongStrokeColorDefaultBrush");
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}
