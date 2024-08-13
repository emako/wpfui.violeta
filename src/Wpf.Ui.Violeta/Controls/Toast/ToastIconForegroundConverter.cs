using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(ToastIcon), typeof(SolidColorBrush))]
internal sealed class ToastIconForegroundConverter : IValueConverter
{
    public static ToastIconForegroundConverter Default => new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ToastIcon icon)
        {
            return icon switch
            {
                ToastIcon.Information => "#55CEF1".ToColor().ToBrush(),
                ToastIcon.Success => "#75CD43".ToColor().ToBrush(),
                ToastIcon.Warning => "#F9D01A".ToColor().ToBrush(),
                ToastIcon.Error => "#FF5656".ToColor().ToBrush(),
                ToastIcon.Question => "#55CEF1".ToColor().ToBrush(),
                ToastIcon.None or _ => "#55CEF1".ToColor().ToBrush(),
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
