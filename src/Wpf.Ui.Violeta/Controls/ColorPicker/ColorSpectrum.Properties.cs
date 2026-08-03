// Adapted from WinUI / Avalonia ColorSpectrum.Properties.
// Ported to WPF for Wpf.Ui.Violeta ColorPicker.

using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public partial class ColorSpectrum
{
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(ColorSpectrum),
            new FrameworkPropertyMetadata(
                Colors.White,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColorPropertyChanged));

    public static readonly DependencyProperty ComponentsProperty =
        DependencyProperty.Register(
            nameof(Components), typeof(ColorSpectrumComponents), typeof(ColorSpectrum),
            new PropertyMetadata(ColorSpectrumComponents.HueSaturation, OnComponentsChanged));

    public static readonly DependencyProperty HsvColorProperty =
        DependencyProperty.Register(
            nameof(HsvColor), typeof(HsvColor), typeof(ColorSpectrum),
            new FrameworkPropertyMetadata(
                Colors.White.ToHsv(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnHsvColorPropertyChanged));

    public static readonly DependencyProperty MaxHueProperty =
        DependencyProperty.Register(nameof(MaxHue), typeof(int), typeof(ColorSpectrum),
            new PropertyMetadata(359, OnHueRangeChanged));

    public static readonly DependencyProperty MaxSaturationProperty =
        DependencyProperty.Register(nameof(MaxSaturation), typeof(int), typeof(ColorSpectrum),
            new PropertyMetadata(100, OnSaturationRangeChanged));

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(ColorSpectrum),
            new PropertyMetadata(100, OnValueRangeChanged));

    public static readonly DependencyProperty MinHueProperty =
        DependencyProperty.Register(nameof(MinHue), typeof(int), typeof(ColorSpectrum),
            new PropertyMetadata(0, OnHueRangeChanged));

    public static readonly DependencyProperty MinSaturationProperty =
        DependencyProperty.Register(nameof(MinSaturation), typeof(int), typeof(ColorSpectrum),
            new PropertyMetadata(0, OnSaturationRangeChanged));

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(nameof(MinValue), typeof(int), typeof(ColorSpectrum),
            new PropertyMetadata(0, OnValueRangeChanged));

    public static readonly DependencyProperty ShapeProperty =
        DependencyProperty.Register(nameof(Shape), typeof(ColorSpectrumShape), typeof(ColorSpectrum),
            new PropertyMetadata(ColorSpectrumShape.Box, OnShapeChanged));

    public static readonly DependencyPropertyKey ThirdComponentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ThirdComponent), typeof(ColorComponent), typeof(ColorSpectrum),
            new PropertyMetadata(ColorComponent.Component3));

    public static readonly DependencyProperty ThirdComponentProperty =
        ThirdComponentPropertyKey.DependencyProperty;

    public static readonly DependencyPropertyKey IsPressedSelectorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsPressedSelector), typeof(bool), typeof(ColorSpectrum), new PropertyMetadata(false));

    public static readonly DependencyProperty IsPressedSelectorProperty = IsPressedSelectorPropertyKey.DependencyProperty;

    public static readonly DependencyPropertyKey IsLargeSelectorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLargeSelector), typeof(bool), typeof(ColorSpectrum), new PropertyMetadata(false));

    public static readonly DependencyProperty IsLargeSelectorProperty = IsLargeSelectorPropertyKey.DependencyProperty;

    public static readonly DependencyPropertyKey IsDarkSelectorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsDarkSelector), typeof(bool), typeof(ColorSpectrum), new PropertyMetadata(false));

    public static readonly DependencyProperty IsDarkSelectorProperty = IsDarkSelectorPropertyKey.DependencyProperty;

    public static readonly DependencyPropertyKey IsLightSelectorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLightSelector), typeof(bool), typeof(ColorSpectrum), new PropertyMetadata(false));

    public static readonly DependencyProperty IsLightSelectorProperty = IsLightSelectorPropertyKey.DependencyProperty;

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public ColorSpectrumComponents Components
    {
        get => (ColorSpectrumComponents)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }

    public HsvColor HsvColor
    {
        get => (HsvColor)GetValue(HsvColorProperty);
        set => SetValue(HsvColorProperty, value);
    }

    public int MaxHue { get => (int)GetValue(MaxHueProperty); set => SetValue(MaxHueProperty, value); }
    public int MaxSaturation { get => (int)GetValue(MaxSaturationProperty); set => SetValue(MaxSaturationProperty, value); }
    public int MaxValue { get => (int)GetValue(MaxValueProperty); set => SetValue(MaxValueProperty, value); }
    public int MinHue { get => (int)GetValue(MinHueProperty); set => SetValue(MinHueProperty, value); }
    public int MinSaturation { get => (int)GetValue(MinSaturationProperty); set => SetValue(MinSaturationProperty, value); }
    public int MinValue { get => (int)GetValue(MinValueProperty); set => SetValue(MinValueProperty, value); }

    public ColorSpectrumShape Shape
    {
        get => (ColorSpectrumShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public ColorComponent ThirdComponent => (ColorComponent)GetValue(ThirdComponentProperty);
    public bool IsPressedSelector => (bool)GetValue(IsPressedSelectorProperty);
    public bool IsLargeSelector => (bool)GetValue(IsLargeSelectorProperty);
    public bool IsDarkSelector => (bool)GetValue(IsDarkSelectorProperty);
    public bool IsLightSelector => (bool)GetValue(IsLightSelectorProperty);

    private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSpectrum s) s.HandleColorChanged((Color)e.OldValue);
    }

    private static void OnHsvColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSpectrum s) s.HandleHsvColorChanged((HsvColor)e.OldValue);
    }

    private static void OnComponentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSpectrum s) s.HandleComponentsChanged();
    }

    private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSpectrum s) s.CreateBitmapsAndColorMap();
    }

    private static void OnHueRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSpectrum s) s.HandleHueRangeChanged();
    }

    private static void OnSaturationRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSpectrum s) s.HandleSaturationRangeChanged();
    }

    private static void OnValueRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSpectrum s) s.HandleValueRangeChanged();
    }
}
