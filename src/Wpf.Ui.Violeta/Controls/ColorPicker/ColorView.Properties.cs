using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public partial class ColorView
{
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(ColorView),
            new FrameworkPropertyMetadata(
                Colors.White,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColorPropertyChanged,
                CoerceColor));

    public static readonly DependencyProperty ColorModelProperty =
        DependencyProperty.Register(nameof(ColorModel), typeof(ColorModel), typeof(ColorView),
            new PropertyMetadata(ColorModel.Rgba));

    public static readonly DependencyProperty ColorSpectrumComponentsProperty =
        DependencyProperty.Register(nameof(ColorSpectrumComponents), typeof(ColorSpectrumComponents), typeof(ColorView),
            new PropertyMetadata(ColorSpectrumComponents.HueSaturation));

    public static readonly DependencyProperty ColorSpectrumShapeProperty =
        DependencyProperty.Register(nameof(ColorSpectrumShape), typeof(ColorSpectrumShape), typeof(ColorView),
            new PropertyMetadata(ColorSpectrumShape.Box));

    public static readonly DependencyProperty HexInputAlphaPositionProperty =
        DependencyProperty.Register(nameof(HexInputAlphaPosition), typeof(AlphaComponentPosition), typeof(ColorView),
            new PropertyMetadata(AlphaComponentPosition.Leading));

    public static readonly DependencyProperty HsvColorProperty =
        DependencyProperty.Register(
            nameof(HsvColor), typeof(HsvColor), typeof(ColorView),
            new FrameworkPropertyMetadata(
                Colors.White.ToHsv(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnHsvColorPropertyChanged,
                CoerceHsvColor));

    public static readonly DependencyProperty IsAccentColorsVisibleProperty =
        DependencyProperty.Register(nameof(IsAccentColorsVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsAlphaEnabledProperty =
        DependencyProperty.Register(nameof(IsAlphaEnabled), typeof(bool), typeof(ColorView),
            new PropertyMetadata(true, OnIsAlphaEnabledChanged));

    public static readonly DependencyProperty IsAlphaVisibleProperty =
        DependencyProperty.Register(nameof(IsAlphaVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsColorComponentsVisibleProperty =
        DependencyProperty.Register(nameof(IsColorComponentsVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsColorModelVisibleProperty =
        DependencyProperty.Register(nameof(IsColorModelVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsColorPaletteVisibleProperty =
        DependencyProperty.Register(nameof(IsColorPaletteVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsColorPreviewVisibleProperty =
        DependencyProperty.Register(nameof(IsColorPreviewVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsColorSpectrumVisibleProperty =
        DependencyProperty.Register(nameof(IsColorSpectrumVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsColorSpectrumSliderVisibleProperty =
        DependencyProperty.Register(nameof(IsColorSpectrumSliderVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsComponentSliderVisibleProperty =
        DependencyProperty.Register(nameof(IsComponentSliderVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsComponentTextInputVisibleProperty =
        DependencyProperty.Register(nameof(IsComponentTextInputVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty IsHexInputVisibleProperty =
        DependencyProperty.Register(nameof(IsHexInputVisible), typeof(bool), typeof(ColorView), new PropertyMetadata(true));

    public static readonly DependencyProperty MaxHueProperty =
        DependencyProperty.Register(nameof(MaxHue), typeof(int), typeof(ColorView), new PropertyMetadata(359));

    public static readonly DependencyProperty MaxSaturationProperty =
        DependencyProperty.Register(nameof(MaxSaturation), typeof(int), typeof(ColorView), new PropertyMetadata(100));

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(ColorView), new PropertyMetadata(100));

    public static readonly DependencyProperty MinHueProperty =
        DependencyProperty.Register(nameof(MinHue), typeof(int), typeof(ColorView), new PropertyMetadata(0));

    public static readonly DependencyProperty MinSaturationProperty =
        DependencyProperty.Register(nameof(MinSaturation), typeof(int), typeof(ColorView), new PropertyMetadata(0));

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(nameof(MinValue), typeof(int), typeof(ColorView), new PropertyMetadata(0));

    public static readonly DependencyProperty PaletteColorsProperty =
        DependencyProperty.Register(nameof(PaletteColors), typeof(IEnumerable<Color>), typeof(ColorView), new PropertyMetadata(null));

    public static readonly DependencyProperty PaletteColumnCountProperty =
        DependencyProperty.Register(nameof(PaletteColumnCount), typeof(int), typeof(ColorView), new PropertyMetadata(4));

    public static readonly DependencyProperty PaletteProperty =
        DependencyProperty.Register(nameof(Palette), typeof(IColorPalette), typeof(ColorView),
            new PropertyMetadata(null, OnPaletteChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(ColorView),
            new PropertyMetadata((int)ColorViewTab.Spectrum));

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public ColorModel ColorModel
    {
        get => (ColorModel)GetValue(ColorModelProperty);
        set => SetValue(ColorModelProperty, value);
    }

    public ColorSpectrumComponents ColorSpectrumComponents
    {
        get => (ColorSpectrumComponents)GetValue(ColorSpectrumComponentsProperty);
        set => SetValue(ColorSpectrumComponentsProperty, value);
    }

    public ColorSpectrumShape ColorSpectrumShape
    {
        get => (ColorSpectrumShape)GetValue(ColorSpectrumShapeProperty);
        set => SetValue(ColorSpectrumShapeProperty, value);
    }

    public AlphaComponentPosition HexInputAlphaPosition
    {
        get => (AlphaComponentPosition)GetValue(HexInputAlphaPositionProperty);
        set => SetValue(HexInputAlphaPositionProperty, value);
    }

    public HsvColor HsvColor
    {
        get => (HsvColor)GetValue(HsvColorProperty);
        set => SetValue(HsvColorProperty, value);
    }

    public bool IsAccentColorsVisible
    {
        get => (bool)GetValue(IsAccentColorsVisibleProperty);
        set => SetValue(IsAccentColorsVisibleProperty, value);
    }

    public bool IsAlphaEnabled
    {
        get => (bool)GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    public bool IsAlphaVisible
    {
        get => (bool)GetValue(IsAlphaVisibleProperty);
        set => SetValue(IsAlphaVisibleProperty, value);
    }

    public bool IsColorComponentsVisible
    {
        get => (bool)GetValue(IsColorComponentsVisibleProperty);
        set => SetValue(IsColorComponentsVisibleProperty, value);
    }

    public bool IsColorModelVisible
    {
        get => (bool)GetValue(IsColorModelVisibleProperty);
        set => SetValue(IsColorModelVisibleProperty, value);
    }

    public bool IsColorPaletteVisible
    {
        get => (bool)GetValue(IsColorPaletteVisibleProperty);
        set => SetValue(IsColorPaletteVisibleProperty, value);
    }

    public bool IsColorPreviewVisible
    {
        get => (bool)GetValue(IsColorPreviewVisibleProperty);
        set => SetValue(IsColorPreviewVisibleProperty, value);
    }

    public bool IsColorSpectrumVisible
    {
        get => (bool)GetValue(IsColorSpectrumVisibleProperty);
        set => SetValue(IsColorSpectrumVisibleProperty, value);
    }

    public bool IsColorSpectrumSliderVisible
    {
        get => (bool)GetValue(IsColorSpectrumSliderVisibleProperty);
        set => SetValue(IsColorSpectrumSliderVisibleProperty, value);
    }

    public bool IsComponentSliderVisible
    {
        get => (bool)GetValue(IsComponentSliderVisibleProperty);
        set => SetValue(IsComponentSliderVisibleProperty, value);
    }

    public bool IsComponentTextInputVisible
    {
        get => (bool)GetValue(IsComponentTextInputVisibleProperty);
        set => SetValue(IsComponentTextInputVisibleProperty, value);
    }

    public bool IsHexInputVisible
    {
        get => (bool)GetValue(IsHexInputVisibleProperty);
        set => SetValue(IsHexInputVisibleProperty, value);
    }

    public int MaxHue
    {
        get => (int)GetValue(MaxHueProperty);
        set => SetValue(MaxHueProperty, value);
    }

    public int MaxSaturation
    {
        get => (int)GetValue(MaxSaturationProperty);
        set => SetValue(MaxSaturationProperty, value);
    }

    public int MaxValue
    {
        get => (int)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public int MinHue
    {
        get => (int)GetValue(MinHueProperty);
        set => SetValue(MinHueProperty, value);
    }

    public int MinSaturation
    {
        get => (int)GetValue(MinSaturationProperty);
        set => SetValue(MinSaturationProperty, value);
    }

    public int MinValue
    {
        get => (int)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public IEnumerable<Color>? PaletteColors
    {
        get => (IEnumerable<Color>?)GetValue(PaletteColorsProperty);
        set => SetValue(PaletteColorsProperty, value);
    }

    public int PaletteColumnCount
    {
        get => (int)GetValue(PaletteColumnCountProperty);
        set => SetValue(PaletteColumnCountProperty, value);
    }

    public IColorPalette? Palette
    {
        get => (IColorPalette?)GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private static object CoerceColor(DependencyObject d, object baseValue)
    {
        if (d is ColorView colorView && baseValue is Color value)
            return colorView.OnCoerceColor(value);
        return baseValue;
    }

    private static object CoerceHsvColor(DependencyObject d, object baseValue)
    {
        if (d is ColorView colorView && baseValue is HsvColor value)
            return colorView.OnCoerceHsvColor(value);
        return baseValue;
    }

    private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorView view)
            view.HandleColorChanged((Color)e.OldValue, (Color)e.NewValue);
    }

    private static void OnHsvColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorView view)
            view.HandleHsvColorChanged((HsvColor)e.OldValue, (HsvColor)e.NewValue);
    }

    private static void OnPaletteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorView view)
            view.HandlePaletteChanged();
    }

    private static void OnIsAlphaEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorView view)
            view.SetCurrentValue(HsvColorProperty, view.OnCoerceHsvColor(view.HsvColor));
    }
}
