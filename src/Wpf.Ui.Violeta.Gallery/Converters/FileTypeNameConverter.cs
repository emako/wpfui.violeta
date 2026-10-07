using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Gallery.Converters;

/// <summary>
/// Maps sample file-type integers (0/1/2) to display names, reproducing GitHub issue #47.
/// </summary>
public sealed class FileTypeNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            0 => "Docx File",
            1 => "Xlsx File",
            2 => "Pdf File",
            _ => value?.ToString() ?? string.Empty,
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
