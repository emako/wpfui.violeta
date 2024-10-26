using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Converters;

[ValueConversion(typeof(string), typeof(ImageSource))]
public class PathToIconConverter : IValueConverter
{
    public static PathToIconConverter Instance { get; } = new();

    public bool Large { get; set; } = false;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            if (Directory.Exists(path))
            {
                return IconManager.FindIconForDir(Large);
            }
            else
            {
                return IconManager.FindIconForFilename(path, Large);
            }
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
