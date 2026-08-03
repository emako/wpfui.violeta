using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public partial class ColorPreviewer
{
    public static readonly DependencyProperty HsvColorProperty =
        DependencyProperty.Register(
            nameof(HsvColor), typeof(HsvColor), typeof(ColorPreviewer),
            new FrameworkPropertyMetadata(
                Colors.Transparent.ToHsv(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnHsvColorChanged));

    public static readonly DependencyProperty IsAccentColorsVisibleProperty =
        DependencyProperty.Register(
            nameof(IsAccentColorsVisible), typeof(bool), typeof(ColorPreviewer),
            new PropertyMetadata(true));

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

    private static void OnHsvColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPreviewer previewer)
        {
            previewer.OnColorChanged(new ColorChangedEventArgs(
                ((HsvColor)e.OldValue).ToRgb(),
                ((HsvColor)e.NewValue).ToRgb()));
        }
    }
}
