using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Attached properties for WinUI-style prefix / suffix chrome on text inputs.
/// Use with <c>PrefixTextBoxStyle</c>.
/// </summary>
public static class InputAssist
{
    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.RegisterAttached(
            "Prefix",
            typeof(object),
            typeof(InputAssist),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public static object? GetPrefix(DependencyObject element) => element.GetValue(PrefixProperty);

    public static void SetPrefix(DependencyObject element, object? value) => element.SetValue(PrefixProperty, value);

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.RegisterAttached(
            "Suffix",
            typeof(object),
            typeof(InputAssist),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public static object? GetSuffix(DependencyObject element) => element.GetValue(SuffixProperty);

    public static void SetSuffix(DependencyObject element, object? value) => element.SetValue(SuffixProperty, value);
}
