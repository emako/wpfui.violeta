using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

[ValueConversion(typeof(bool), typeof(Visibility))]
internal sealed class TreeListViewCanExpandConverter : IValueConverter
{
    public bool IsInverted { get; set; } = false;

    public object Convert(object o, Type type, object parameter, CultureInfo culture)
    {
        return ((bool)o ^ IsInverted) ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
