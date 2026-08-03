using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public partial class ColorSlider
{
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(ColorSlider),
            new FrameworkPropertyMetadata(
                Colors.White,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColorRelatedPropertyChanged));

    public static readonly DependencyProperty ColorComponentProperty =
        DependencyProperty.Register(
            nameof(ColorComponent), typeof(ColorComponent), typeof(ColorSlider),
            new PropertyMetadata(ColorComponent.Component1, OnColorRelatedPropertyChanged));

    public static readonly DependencyProperty ColorModelProperty =
        DependencyProperty.Register(
            nameof(ColorModel), typeof(ColorModel), typeof(ColorSlider),
            new PropertyMetadata(ColorModel.Rgba, OnColorRelatedPropertyChanged));

    public static readonly DependencyProperty HsvColorProperty =
        DependencyProperty.Register(
            nameof(HsvColor), typeof(HsvColor), typeof(ColorSlider),
            new FrameworkPropertyMetadata(
                Colors.White.ToHsv(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnHsvColorChanged));

    public static readonly DependencyProperty IsAlphaVisibleProperty =
        DependencyProperty.Register(
            nameof(IsAlphaVisible), typeof(bool), typeof(ColorSlider),
            new PropertyMetadata(false, OnColorRelatedPropertyChanged));

    public static readonly DependencyProperty IsPerceptiveProperty =
        DependencyProperty.Register(
            nameof(IsPerceptive), typeof(bool), typeof(ColorSlider),
            new PropertyMetadata(true, OnColorRelatedPropertyChanged));

    public static readonly DependencyProperty IsRoundingEnabledProperty =
        DependencyProperty.Register(
            nameof(IsRoundingEnabled), typeof(bool), typeof(ColorSlider),
            new PropertyMetadata(false, OnRoundingEnabledChanged));

    public static readonly DependencyPropertyKey IsDarkSelectorPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsDarkSelector), typeof(bool), typeof(ColorSlider),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsDarkSelectorProperty =
        IsDarkSelectorPropertyKey.DependencyProperty;

    public static readonly DependencyPropertyKey IsLightSelectorPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsLightSelector), typeof(bool), typeof(ColorSlider),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsLightSelectorProperty =
        IsLightSelectorPropertyKey.DependencyProperty;

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public ColorComponent ColorComponent
    {
        get => (ColorComponent)GetValue(ColorComponentProperty);
        set => SetValue(ColorComponentProperty, value);
    }

    public ColorModel ColorModel
    {
        get => (ColorModel)GetValue(ColorModelProperty);
        set => SetValue(ColorModelProperty, value);
    }

    public HsvColor HsvColor
    {
        get => (HsvColor)GetValue(HsvColorProperty);
        set => SetValue(HsvColorProperty, value);
    }

    public bool IsAlphaVisible
    {
        get => (bool)GetValue(IsAlphaVisibleProperty);
        set => SetValue(IsAlphaVisibleProperty, value);
    }

    public bool IsPerceptive
    {
        get => (bool)GetValue(IsPerceptiveProperty);
        set => SetValue(IsPerceptiveProperty, value);
    }

    public bool IsRoundingEnabled
    {
        get => (bool)GetValue(IsRoundingEnabledProperty);
        set => SetValue(IsRoundingEnabledProperty, value);
    }

    public bool IsDarkSelector => (bool)GetValue(IsDarkSelectorProperty);

    public bool IsLightSelector => (bool)GetValue(IsLightSelectorProperty);

    private static void OnColorRelatedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSlider slider)
            slider.OnColorPropertyChanged(e);
    }

    private static void OnHsvColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSlider slider)
            slider.OnHsvColorPropertyChanged(e);
    }

    private static void OnRoundingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSlider slider && !slider._ignorePropertyChanged)
            slider.SetColorToSliderValues();
    }
}
