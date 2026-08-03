using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Converts Color to/from "R,G,B,A" text (Semi.Avalonia pattern).
/// </summary>
public class ColorToTextConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Color color
            ? $"{color.R},{color.G},{color.B},{color.A}"
            : DependencyProperty.UnsetValue;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
            return Binding.DoNothing;

        var parts = str.Split(',');
        if (parts.Length != 4 || parts.Any(string.IsNullOrWhiteSpace))
            return Binding.DoNothing;

        if (byte.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) &&
            byte.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var g) &&
            byte.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b) &&
            byte.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var a))
        {
            return Color.FromArgb(a, r, g, b);
        }

        return Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
