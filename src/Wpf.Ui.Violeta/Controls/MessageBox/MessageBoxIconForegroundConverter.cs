using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(MessageBoxIcon), typeof(SolidColorBrush))]
public sealed class MessageBoxIconForegroundConverter : IValueConverter
{
    public static MessageBoxIconForegroundConverter New => new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Information => "#55CEF1".ToColor().ToBrush(),
                MessageBoxIcon.Success => "#75CD43".ToColor().ToBrush(),
                MessageBoxIcon.Warning => "#F9D01A".ToColor().ToBrush(),
                MessageBoxIcon.Error => "#FF5656".ToColor().ToBrush(),
                MessageBoxIcon.Question => "#55CEF1".ToColor().ToBrush(),
                MessageBoxIcon.None or _ => "#55CEF1".ToColor().ToBrush(),
            };
        }
        return "#55CEF1".ToColor().ToBrush();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

file static class ColorExtension
{
    public static Color ToColor(this string hex) => (Color)ColorConverter.ConvertFromString(hex);

    public static SolidColorBrush ToBrush(this Color color) => new(color);
}
