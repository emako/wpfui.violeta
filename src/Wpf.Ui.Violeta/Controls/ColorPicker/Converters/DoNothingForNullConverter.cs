using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Converter that will do nothing (not update bound values) when a null value is encountered.
/// </summary>
public class DoNothingForNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value ?? Binding.DoNothing;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value ?? Binding.DoNothing;
}
