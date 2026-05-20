using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Converts an <see cref="AnchorItem.Level"/> integer to a left <see cref="Thickness"/>
/// used for indenting nested items.
/// </summary>
[ValueConversion(typeof(int), typeof(Thickness))]
public sealed class AnchorLevelToMarginConverter : IValueConverter
{
    /// <summary>Pixels of indent added per nesting level.</summary>
    public double IndentWidth { get; set; } = 12d;

    public static readonly AnchorLevelToMarginConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int level = value is int l ? l : 0;
        return new Thickness(level * IndentWidth, 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}
