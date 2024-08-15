using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(MessageBoxIcon), typeof(string))]
public sealed class MessageBoxIconConverter : IValueConverter
{
    public static MessageBoxIconConverter New => new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Information => FontSymbols.Info,
                MessageBoxIcon.Success => FontSymbols.Accept,
                MessageBoxIcon.Warning => FontSymbols.Warning,
                MessageBoxIcon.Error => FontSymbols.Cancel,
                MessageBoxIcon.Question => FontSymbols.Unknown,
                MessageBoxIcon.None or _ => string.Empty,
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
