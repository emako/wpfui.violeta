using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(ToastIcon), typeof(string))]
internal sealed class ToastIconConverter : IValueConverter
{
    public static ToastIconConverter Default => new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ToastIcon icon)
        {
            return icon switch
            {
                ToastIcon.Information => FontSymbols.Info,
                ToastIcon.Success => FontSymbols.Accept,
                ToastIcon.Warning => FontSymbols.Warning,
                ToastIcon.Error => FontSymbols.Cancel,
                ToastIcon.Question => FontSymbols.Unknown,
                ToastIcon.None or _ => string.Empty,
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
