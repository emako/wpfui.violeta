using System;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Extracts a single member of a CornerRadius object.
/// For example, if you have a CornerRadius of 5,5,5,5 and you want to extract the TopLeft value, you would use this converter with the TargetMember set to TopLeft.
/// </summary>
public class CornerRadiusExtractionConverter : IValueConverter
{
    public CornerRadiusExtractMember TargetMember { get; set; }

    public double Scale { get; set; } = 1;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is CornerRadius radius)
        {
            double result;
            CornerRadius cornerRadius = radius;

            result = TargetMember switch
            {
                CornerRadiusExtractMember.TopLeft => cornerRadius.TopLeft,
                CornerRadiusExtractMember.TopRight => cornerRadius.TopRight,
                CornerRadiusExtractMember.BottomRight => cornerRadius.BottomRight,
                CornerRadiusExtractMember.BottomLeft => cornerRadius.BottomLeft,
                _ => cornerRadius.TopLeft,
            };
            return result * Scale;
        }

        return null!;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        switch (value)
        {
            case double:
                {
                    double doubleValue = (double)value / Scale;

                    return TargetMember switch
                    {
                        CornerRadiusExtractMember.TopLeft => new CornerRadius(doubleValue, 0, 0, 0),
                        CornerRadiusExtractMember.TopRight => new CornerRadius(0, doubleValue, 0, 0),
                        CornerRadiusExtractMember.BottomRight => new CornerRadius(0, 0, doubleValue, 0),
                        CornerRadiusExtractMember.BottomLeft => new CornerRadius(0, 0, 0, doubleValue),
                        _ => new CornerRadius(doubleValue),
                    };
                }

            default:
                return new CornerRadius(0);
        }
    }
}

public enum CornerRadiusExtractMember
{
    TopLeft,
    TopRight,
    BottomRight,
    BottomLeft,
}
