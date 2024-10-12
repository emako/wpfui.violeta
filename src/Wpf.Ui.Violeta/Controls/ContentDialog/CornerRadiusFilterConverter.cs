using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

public sealed class CornerRadiusFilterConverter : DependencyObject, IValueConverter
{
    public CornerRadiusFilterKind Filter { get; set; }

    public double Scale { get; set; } = 1d;

    public static CornerRadius Convert(CornerRadius radius, CornerRadiusFilterKind filterKind)
    {
        CornerRadius result = radius;
        switch (filterKind)
        {
            case CornerRadiusFilterKind.Top:
                result.BottomLeft = 0.0;
                result.BottomRight = 0.0;
                break;

            case CornerRadiusFilterKind.Right:
                result.TopLeft = 0.0;
                result.BottomLeft = 0.0;
                break;

            case CornerRadiusFilterKind.Bottom:
                result.TopLeft = 0.0;
                result.TopRight = 0.0;
                break;

            case CornerRadiusFilterKind.Left:
                result.TopRight = 0.0;
                result.BottomRight = 0.0;
                break;
        }

        return result;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CornerRadius radius)
        {
            double scale = Scale;
            if (!double.IsNaN(scale))
            {
                radius.TopLeft *= scale;
                radius.TopRight *= scale;
                radius.BottomRight *= scale;
                radius.BottomLeft *= scale;
            }

            CornerRadiusFilterKind filter = Filter;
            if (filter == CornerRadiusFilterKind.TopLeftValue || filter == CornerRadiusFilterKind.BottomRightValue)
            {
                return GetDoubleValue(radius, filter);
            }

            return Convert(radius, filter);
        }

        return new CornerRadius(0.0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new CornerRadius(0.0);
    }

    private double GetDoubleValue(CornerRadius radius, CornerRadiusFilterKind filterKind)
    {
        return filterKind switch
        {
            CornerRadiusFilterKind.TopLeftValue => radius.TopLeft,
            CornerRadiusFilterKind.BottomRightValue => radius.BottomRight,
            _ => 0.0,
        };
    }
}

public enum CornerRadiusFilterKind
{
    None,
    Top,
    Right,
    Bottom,
    Left,
    TopLeftValue,
    BottomRightValue
}
