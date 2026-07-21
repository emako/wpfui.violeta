using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Produces a uniform CornerRadius from the smaller of two lengths, scaled by an optional ratio.
/// </summary>
[ValueConversion(typeof(CornerRadius), typeof(CornerRadius))]
public class CalendarRoundRadiusConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            throw new NotImplementedException();
        }

        _ = double.TryParse(values[0].ToString(), out double d1);
        _ = double.TryParse(values[1].ToString(), out double d2);

        double ratio = 1;
        if (values.Length >= 3)
        {
            _ = double.TryParse(values[2].ToString(), out ratio);
        }

        return new CornerRadius(Math.Min(d1, d2) * ratio / 2);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
