using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

internal sealed class LeftSplitThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Thickness thickness)
        {
            return default(Thickness);
        }

        return new Thickness(thickness.Left, thickness.Top, 0, thickness.Bottom);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

internal sealed class RightSplitThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Thickness thickness)
        {
            return default(Thickness);
        }

        return new Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

internal sealed class LeftSplitCornerRadiusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CornerRadius cornerRadius)
        {
            return default(CornerRadius);
        }

        return new CornerRadius(cornerRadius.TopLeft, 0, 0, cornerRadius.BottomLeft);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

internal sealed class RightSplitCornerRadiusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CornerRadius cornerRadius)
        {
            return default(CornerRadius);
        }

        return new CornerRadius(0, cornerRadius.TopRight, cornerRadius.BottomRight, 0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
