using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

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
