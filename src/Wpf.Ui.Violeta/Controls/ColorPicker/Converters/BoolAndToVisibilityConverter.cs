using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Returns <see cref="Visibility.Visible"/> only when every bound value is <c>true</c>.
/// </summary>
public class BoolAndToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
            return Visibility.Collapsed;

        foreach (var value in values)
        {
            if (value is not true)
                return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
