using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Returns <see cref="Visibility.Visible"/> when the bound int equals the converter parameter; otherwise Collapsed.
/// </summary>
public sealed class IndexToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int index)
            return Visibility.Collapsed;

        if (!TryParseIndex(parameter, out int target))
            return Visibility.Collapsed;

        return index == target ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;

    private static bool TryParseIndex(object? parameter, out int index)
    {
        switch (parameter)
        {
            case int i:
                index = i;
                return true;
            case string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out index):
                return true;
            default:
                index = -1;
                return false;
        }
    }
}
