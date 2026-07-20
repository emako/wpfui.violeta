#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Globalization;
using System.Windows;
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

        double.TryParse(values[0].ToString(), out double d1);
        double.TryParse(values[1].ToString(), out double d2);

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

        double.TryParse(values[0].ToString(), out double d1);
        double.TryParse(values[1].ToString(), out double d2);

        double ratio = 1;
        if (values.Length >= 3)
        {
            double.TryParse(values[2].ToString(), out ratio);
        }

        return new CornerRadius(Math.Min(d1, d2) * ratio / 2);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Builds a placement rectangle from a width/height pair, inset by <see cref="Margin"/>.
/// Used to position a Popup relative to its owner.
/// </summary>
public class CalendarPlacementRectangleConverter : IMultiValueConverter
{
    public Thickness Margin { get; set; }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is double width && values[1] is double height)
        {
            var margin = Margin;
            var topLeft = new Point(margin.Left, margin.Top);
            var bottomRight = new Point(width - margin.Right, height - margin.Bottom);
            return new Rect(topLeft, bottomRight);
        }

        return Rect.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
