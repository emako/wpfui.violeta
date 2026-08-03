using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Creates an accent color for a given base color value and step parameter.
/// </summary>
public class AccentColorConverter : IValueConverter
{
    public const double ValueDelta = 0.1;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int accentStep;
        Color? rgbColor = null;
        HsvColor? hsvColor = null;

        if (value is Color valueColor)
            rgbColor = valueColor;
        else if (value is HsvColor valueHsvColor)
            hsvColor = valueHsvColor;
        else if (value is SolidColorBrush valueBrush)
            rgbColor = valueBrush.Color;
        else
            return DependencyProperty.UnsetValue;

        try
        {
            accentStep = int.Parse(parameter?.ToString() ?? "", CultureInfo.InvariantCulture);
        }
        catch
        {
            return DependencyProperty.UnsetValue;
        }

        if (hsvColor == null && rgbColor != null)
            hsvColor = rgbColor.Value.ToHsv();

        if (hsvColor != null)
            return new SolidColorBrush(GetAccent(hsvColor.Value, accentStep).ToRgb());

        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DependencyProperty.UnsetValue;

    public static HsvColor GetAccent(HsvColor hsvColor, int accentStep)
    {
        if (accentStep != 0)
        {
            double colorValue = hsvColor.V;
            colorValue += accentStep * ValueDelta;
            colorValue = Math.Round(colorValue, 2);
            return new HsvColor(hsvColor.A, hsvColor.H, hsvColor.S, colorValue);
        }

        return hsvColor;
    }
}
