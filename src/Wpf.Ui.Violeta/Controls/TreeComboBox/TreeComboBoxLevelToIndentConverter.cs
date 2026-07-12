using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(int), typeof(Thickness))]
internal sealed class TreeComboBoxLevelToIndentConverter : IValueConverter
{
    private const double IndentSize = 20.0;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new Thickness((int)(value ?? 0) * IndentSize, 0.0, 0.0, 0.0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
