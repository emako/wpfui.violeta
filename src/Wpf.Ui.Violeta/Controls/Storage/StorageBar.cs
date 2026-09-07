using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Windows Settings style capacity bar.
/// </summary>
[TemplatePart(Name = PartRoot, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartValueColumn, Type = typeof(ColumnDefinition))]
[TemplatePart(Name = PartGapColumn, Type = typeof(ColumnDefinition))]
[TemplatePart(Name = PartTrackColumn, Type = typeof(ColumnDefinition))]
[TemplatePart(Name = PartValueBar, Type = typeof(Border))]
[TemplatePart(Name = PartTrackBar, Type = typeof(Border))]
public class StorageBar : RangeBase
{
    private const string PartRoot = "PART_Root";
    private const string PartValueColumn = "PART_ValueColumn";
    private const string PartGapColumn = "PART_GapColumn";
    private const string PartTrackColumn = "PART_TrackColumn";
    private const string PartValueBar = "PART_ValueBar";
    private const string PartTrackBar = "PART_TrackBar";

    private FrameworkElement? _root;
    private ColumnDefinition? _valueColumn;
    private ColumnDefinition? _gapColumn;
    private ColumnDefinition? _trackColumn;
    private Border? _valueBar;
    private Border? _trackBar;

    static StorageBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StorageBar),
            new FrameworkPropertyMetadata(typeof(StorageBar)));

        FocusableProperty.OverrideMetadata(
            typeof(StorageBar),
            new FrameworkPropertyMetadata(false));

        IsTabStopProperty.OverrideMetadata(
            typeof(StorageBar),
            new FrameworkPropertyMetadata(false));
    }

    public StorageBar()
    {
        IsEnabledChanged += (_, _) => ApplyAppearance();
        SizeChanged += (_, _) => UpdateColumns();
    }

    #region Percent

    private static readonly DependencyPropertyKey PercentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Percent),
            typeof(double),
            typeof(StorageBar),
            new PropertyMetadata(0d));

    public static readonly DependencyProperty PercentProperty = PercentPropertyKey.DependencyProperty;

    public double Percent => (double)GetValue(PercentProperty);

    #endregion

    #region PercentCaution

    public static readonly DependencyProperty PercentCautionProperty =
        DependencyProperty.Register(
            nameof(PercentCaution),
            typeof(double),
            typeof(StorageBar),
            new PropertyMetadata(75d, OnAppearanceChanged));

    public double PercentCaution
    {
        get => (double)GetValue(PercentCautionProperty);
        set => SetValue(PercentCautionProperty, value);
    }

    #endregion

    #region PercentCritical

    public static readonly DependencyProperty PercentCriticalProperty =
        DependencyProperty.Register(
            nameof(PercentCritical),
            typeof(double),
            typeof(StorageBar),
            new PropertyMetadata(90d, OnAppearanceChanged));

    public double PercentCritical
    {
        get => (double)GetValue(PercentCriticalProperty);
        set => SetValue(PercentCriticalProperty, value);
    }

    #endregion

    #region ValueBarHeight

    public static readonly DependencyProperty ValueBarHeightProperty =
        DependencyProperty.Register(
            nameof(ValueBarHeight),
            typeof(double),
            typeof(StorageBar),
            new PropertyMetadata(10d, OnLayoutChanged));

    public double ValueBarHeight
    {
        get => (double)GetValue(ValueBarHeightProperty);
        set => SetValue(ValueBarHeightProperty, value);
    }

    #endregion

    #region TrackBarHeight

    public static readonly DependencyProperty TrackBarHeightProperty =
        DependencyProperty.Register(
            nameof(TrackBarHeight),
            typeof(double),
            typeof(StorageBar),
            new PropertyMetadata(10d, OnLayoutChanged));

    public double TrackBarHeight
    {
        get => (double)GetValue(TrackBarHeightProperty);
        set => SetValue(TrackBarHeightProperty, value);
    }

    #endregion

    #region GapWidth

    public static readonly DependencyProperty GapWidthProperty =
        DependencyProperty.Register(
            nameof(GapWidth),
            typeof(double),
            typeof(StorageBar),
            new PropertyMetadata(0d, OnLayoutChanged));

    public double GapWidth
    {
        get => (double)GetValue(GapWidthProperty);
        set => SetValue(GapWidthProperty, value);
    }

    #endregion

    #region BarShape

    public static readonly DependencyProperty BarShapeProperty =
        DependencyProperty.Register(
            nameof(BarShape),
            typeof(BarShapes),
            typeof(StorageBar),
            new PropertyMetadata(BarShapes.Flat, OnLayoutChanged));

    public BarShapes BarShape
    {
        get => (BarShapes)GetValue(BarShapeProperty);
        set => SetValue(BarShapeProperty, value);
    }

    #endregion

    #region ValueCornerRadius / TrackCornerRadius

    private static readonly DependencyPropertyKey ValueCornerRadiusPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ValueCornerRadius),
            typeof(CornerRadius),
            typeof(StorageBar),
            new PropertyMetadata(new CornerRadius(3)));

    public static readonly DependencyProperty ValueCornerRadiusProperty =
        ValueCornerRadiusPropertyKey.DependencyProperty;

    public CornerRadius ValueCornerRadius => (CornerRadius)GetValue(ValueCornerRadiusProperty);

    private static readonly DependencyPropertyKey TrackCornerRadiusPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(TrackCornerRadius),
            typeof(CornerRadius),
            typeof(StorageBar),
            new PropertyMetadata(new CornerRadius(1.5)));

    public static readonly DependencyProperty TrackCornerRadiusProperty =
        TrackCornerRadiusPropertyKey.DependencyProperty;

    public CornerRadius TrackCornerRadius => (CornerRadius)GetValue(TrackCornerRadiusProperty);

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _root = GetTemplateChild(PartRoot) as FrameworkElement;
        _valueColumn = GetTemplateChild(PartValueColumn) as ColumnDefinition;
        _gapColumn = GetTemplateChild(PartGapColumn) as ColumnDefinition;
        _trackColumn = GetTemplateChild(PartTrackColumn) as ColumnDefinition;
        _valueBar = GetTemplateChild(PartValueBar) as Border;
        _trackBar = GetTemplateChild(PartTrackBar) as Border;

        UpdatePercent();
        UpdateCornerRadii();
        UpdateColumns();
        ApplyAppearance();
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdatePercent();
        UpdateColumns();
        ApplyAppearance();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        UpdatePercent();
        UpdateColumns();
        ApplyAppearance();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        UpdatePercent();
        UpdateColumns();
        ApplyAppearance();
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StorageBar bar)
        {
            bar.ApplyAppearance();
        }
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StorageBar bar)
        {
            bar.UpdateCornerRadii();
            bar.UpdateColumns();
        }
    }

    private void UpdatePercent()
    {
        double range = Maximum - Minimum;
        double percent = range <= 0
            ? 0
            : Clamp((Value - Minimum) / range * 100d, 0d, 100d);
        SetValue(PercentPropertyKey, percent);
    }

    private void UpdateCornerRadii()
    {
        SetValue(ValueCornerRadiusPropertyKey, CreateCornerRadius(ValueBarHeight));
        SetValue(TrackCornerRadiusPropertyKey, CreateCornerRadius(TrackBarHeight));
    }

    private CornerRadius CreateCornerRadius(double height)
    {
        double radius = BarShape switch
        {
            BarShapes.Flat => 0d,
            BarShapes.Soft => Math.Max(0d, height / 4d),
            _ => Math.Max(0d, height / 2d),
        };
        return new CornerRadius(radius);
    }

    private void UpdateColumns()
    {
        if (_valueColumn is null || _gapColumn is null || _trackColumn is null)
        {
            return;
        }

        double percent = Percent;
        double gap = GapWidth;

        if (percent <= 0.01)
        {
            _valueColumn.Width = new GridLength(0);
            _gapColumn.Width = new GridLength(0);
            _trackColumn.Width = new GridLength(1, GridUnitType.Star);
            return;
        }

        if (percent >= 99.99)
        {
            _valueColumn.Width = new GridLength(1, GridUnitType.Star);
            _gapColumn.Width = new GridLength(0);
            _trackColumn.Width = new GridLength(0);
            return;
        }

        _valueColumn.Width = new GridLength(percent, GridUnitType.Star);
        _gapColumn.Width = new GridLength(Math.Max(0d, gap));
        _trackColumn.Width = new GridLength(100d - percent, GridUnitType.Star);
    }

    private void ApplyAppearance()
    {
        if (_root is not null)
        {
            _root.Opacity = IsEnabled ? 1d : 0.4d;
        }

        if (_valueBar is null || _trackBar is null)
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

        _valueBar.SetResourceReference(Border.BackgroundProperty, valueKey);
        _trackBar.SetResourceReference(Border.BackgroundProperty, "ControlStrongStrokeColorDefaultBrush");
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
