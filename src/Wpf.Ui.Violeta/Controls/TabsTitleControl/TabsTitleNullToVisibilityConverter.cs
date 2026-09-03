using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Converts null / non-null to <see cref="Visibility"/>.
/// When <see cref="Invert"/> is false: null → Collapsed, non-null → Visible.
/// When <see cref="Invert"/> is true: null → Visible, non-null → Collapsed.
/// </summary>
public sealed class TabsTitleNullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isNull = value is null;
        if (Invert)
        {
            isNull = !isNull;
        }

        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
