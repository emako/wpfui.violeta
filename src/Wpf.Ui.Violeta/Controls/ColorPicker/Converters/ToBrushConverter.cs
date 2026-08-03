using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Converts the given value into a Brush when a conversion is possible.
/// </summary>
public class ToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Brush brush)
            return brush;
        if (value is Color valueColor)
            return new SolidColorBrush(valueColor);
        if (value is HsvColor valueHsvColor)
            return new SolidColorBrush(valueHsvColor.ToRgb());

        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;
}
