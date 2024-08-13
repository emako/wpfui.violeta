using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(ToastIcon), typeof(Visibility))]
internal sealed class ToastIconVisibilityConverter : IValueConverter
{
    public static ToastIconVisibilityConverter Default => new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ToastIcon icon)
        {
            return icon switch
            {
                ToastIcon.None => Visibility.Collapsed,
                _ => Visibility.Visible,
            };
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
