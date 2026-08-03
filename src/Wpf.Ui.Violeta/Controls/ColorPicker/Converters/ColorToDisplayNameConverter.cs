using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Controls.ColorPickerHelpers;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Gets the approximated display name for the color.
/// </summary>
public class ColorToDisplayNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color color;

        if (value is Color valueColor)
            color = valueColor;
        else if (value is HsvColor valueHsvColor)
            color = valueHsvColor.ToRgb();
        else if (value is SolidColorBrush valueBrush)
            color = valueBrush.Color;
        else
            return DependencyProperty.UnsetValue;

        if (color.A == 0x00)
            return DependencyProperty.UnsetValue;

        return ColorHelper.ToDisplayName(color);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;
}
