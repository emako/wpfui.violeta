using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Compares an enum (or other) value to a converter parameter; ConvertBack returns the parameter when true.
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value != null && parameter != null && value.Equals(parameter);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter != null)
            return parameter;
        return Binding.DoNothing;
    }
}
