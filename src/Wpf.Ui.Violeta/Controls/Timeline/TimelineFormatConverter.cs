using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

public class TimelineFormatConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length > 1 && values[0] is DateTime date && values[1] is string fmt)
        {
            return date.ToString(fmt, culture);
        }
        return string.Empty;
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
