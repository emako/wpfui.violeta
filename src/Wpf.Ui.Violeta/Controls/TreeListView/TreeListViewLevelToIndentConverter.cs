using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(int), typeof(Thickness))]
internal sealed class TreeListViewLevelToIndentConverter : IValueConverter
{
    private const double IndentSize = 19d;

    public object Convert(object o, Type type, object parameter, CultureInfo culture)
    {
        return new Thickness((int)o * IndentSize, 0d, 0d, 0d);
    }

    public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
