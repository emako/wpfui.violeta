using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Converts the given value into a Color when a conversion is possible.
/// </summary>
public class ToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color valueColor)
            return valueColor;

        if (value is HsvColor valueHsvColor)
            return valueHsvColor.ToRgb();

        if (value is SolidColorBrush valueBrush)
        {
            double alpha = valueBrush.Color.A * valueBrush.Opacity;
            return Color.FromArgb(
                (byte)Clamp(alpha, 0x00, 0xFF),
                valueBrush.Color.R,
                valueBrush.Color.G,
                valueBrush.Color.B);
        }

        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;

    private static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;
}
