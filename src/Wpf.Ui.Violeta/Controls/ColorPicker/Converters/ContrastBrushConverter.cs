using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls.ColorPickerHelpers;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Gets a SolidColorBrush (black or white) depending on the luminance of the supplied color.
/// </summary>
public class ContrastBrushConverter : IValueConverter
{
    private readonly ToColorConverter _toColorConverter = new();

    public byte AlphaThreshold { get; set; } = 128;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color comparisonColor;
        Color? defaultColor = null;

        var convertedValue = _toColorConverter.Convert(value, targetType, parameter, culture);
        if (convertedValue is Color valueColor)
            comparisonColor = valueColor;
        else
            return DependencyProperty.UnsetValue;

        var convertedParameter = _toColorConverter.Convert(parameter, targetType, parameter, culture);
        if (convertedParameter is Color parameterColor)
            defaultColor = parameterColor;

        if (comparisonColor.A < AlphaThreshold && defaultColor.HasValue)
            return new SolidColorBrush(defaultColor.Value);

        if (ColorHelper.GetRelativeLuminance(comparisonColor) <= 0.5)
            return new SolidColorBrush(Colors.White);

        return new SolidColorBrush(Colors.Black);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;
}
