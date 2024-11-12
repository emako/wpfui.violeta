using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Ui.Violeta.Converters;

[ValueConversion(typeof(IConvertible), typeof(string))]
public class FileSizeStringConverter : IValueConverter
{
    public static FileSizeStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IConvertible convertibleValue)
        {
            try
            {
                long lengthOfDocument = System.Convert.ToInt64(convertibleValue);
                return ToFileSizeString(lengthOfDocument);
            }
            catch (FormatException)
            {
                if (parameter is string fallback)
                {
                    return fallback;
                }
                return "Invalid Size";
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static string ToFileSizeString(long lengthOfDocument)
    {
        if (lengthOfDocument < 1024L)
        {
            return $"{lengthOfDocument} B";
        }
        else if (lengthOfDocument < Math.Pow(1024d, 2d))
        {
            return $"{lengthOfDocument / 1024d:F3} KB";
        }
        else if (lengthOfDocument < Math.Pow(1024d, 3d))
        {
            return $"{lengthOfDocument / Math.Pow(1024d, 2d):F3} MB";
        }
        else if (lengthOfDocument < Math.Pow(1024d, 4d))
        {
            return $"{lengthOfDocument / Math.Pow(1024d, 3d):F3} GB";
        }
        else if (lengthOfDocument < Math.Pow(1024d, 5d))
        {
            return $"{lengthOfDocument / Math.Pow(1024d, 4d):F3} TB";
        }
        else if (lengthOfDocument < Math.Pow(1024d, 6d))
        {
            return $"{lengthOfDocument / Math.Pow(1024d, 5d):F3} PB";
        }
        else if (lengthOfDocument < Math.Pow(1024d, 7d))
        {
            return $"{lengthOfDocument / Math.Pow(1024d, 6d):F3} EB";
        }
        else if (lengthOfDocument < Math.Pow(1024d, 8d))
        {
            return $"{lengthOfDocument / Math.Pow(1024d, 7d):F3} ZB";
        }
        else
        {
            return $"{lengthOfDocument / Math.Pow(1024d, 8d):F3} YB";
        }
    }
}
