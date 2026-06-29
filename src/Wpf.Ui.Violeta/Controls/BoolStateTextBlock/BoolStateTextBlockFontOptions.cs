using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public class BoolStateTextBlockFontOptions : Freezable
{
    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
        nameof(Foreground),
        typeof(Brush),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(SystemColors.ControlTextBrush));

    public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
        nameof(FontFamily),
        typeof(FontFamily),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(SystemFonts.MessageFontFamily));

    public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
        nameof(FontSize),
        typeof(double),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(SystemFonts.MessageFontSize));

    public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
        nameof(FontWeight),
        typeof(FontWeight),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(FontWeights.Normal));

    public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
        nameof(FontStyle),
        typeof(FontStyle),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(FontStyles.Normal));

    public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
        nameof(FontStretch),
        typeof(FontStretch),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(FontStretches.Normal));

    public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
        nameof(LineHeight),
        typeof(double),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
        nameof(TextDecorations),
        typeof(TextDecorationCollection),
        typeof(BoolStateTextBlockFontOptions),
        new PropertyMetadata(null));

    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontStretch FontStretch
    {
        get => (FontStretch)GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public TextDecorationCollection? TextDecorations
    {
        get => (TextDecorationCollection?)GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }

    protected override Freezable CreateInstanceCore()
    {
        return new BoolStateTextBlockFontOptions();
    }
}
