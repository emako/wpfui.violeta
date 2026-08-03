using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Converts HsvColor to/from "H,S,V,A" text with S/V/A as 0..100 (Semi.Avalonia pattern).
/// </summary>
public class HsvColorToTextConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HsvColor hsvColor
            ? $"{Math.Round(hsvColor.H)},{Math.Round(hsvColor.S * 100)},{Math.Round(hsvColor.V * 100)},{Math.Round(hsvColor.A * 100)}"
            : DependencyProperty.UnsetValue;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
            return Binding.DoNothing;

        var parts = str.Split(',');
        if (parts.Length != 4 || parts.Any(string.IsNullOrWhiteSpace))
            return Binding.DoNothing;

        if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var h) &&
            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var s) &&
            double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var v) &&
            double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
        {
            return new HsvColor(a / 100, h, s / 100, v / 100);
        }

        return Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
