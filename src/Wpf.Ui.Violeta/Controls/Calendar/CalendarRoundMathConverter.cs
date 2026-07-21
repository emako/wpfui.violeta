using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Returns the smaller of two lengths, or NaN if either is zero.
/// </summary>
[ValueConversion(typeof(double), typeof(double))]
public class CalendarRoundMathConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            throw new NotImplementedException();
        }

        _ = double.TryParse(values[0].ToString(), out double d1);
        _ = double.TryParse(values[1].ToString(), out double d2);

        if (d1 * d2 == 0)
        {
            return double.NaN;
        }

        return Math.Min(d1, d2);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
