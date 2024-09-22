using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Controls;

[ValueConversion(typeof(int), typeof(Thickness))]
internal sealed class TreeModelLevelToIndentConverter : IValueConverter
{
    private const double IndentSize = 19d;

    public object? Convert(object? value, Type type, object? parameter, CultureInfo culture)
    {
        return new Thickness((int)value! * IndentSize, 0d, 0d, 0d);
    }

    public object? ConvertBack(object value, Type type, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
