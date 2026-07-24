using System;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Win32;

internal static class Acrylic10Helper
{
    public static Color GetAcrylicTintColor(string? customColor = null)
    {
        if (!string.IsNullOrEmpty(customColor))
        {
            try
            {
                return ((SolidColorBrush)new BrushConverter().ConvertFromString(customColor)!).Color;
            }
            catch (Exception ex) when (ex is FormatException || ex is NotSupportedException)
            {
                // Ignore invalid color
            }
        }

        return ((SolidColorBrush)Application.Current.FindResource("MainWindowBackground")).Color;
    }

    public static Color GetAcrylic10TintColor(string? customColor = null, bool? isDarkTheme = null)
    {
        if (!string.IsNullOrEmpty(customColor))
        {
            try
            {
                return ((SolidColorBrush)new BrushConverter().ConvertFromString(customColor)!).Color;
            }
            catch (Exception ex) when (ex is FormatException || ex is NotSupportedException)
            {
                // Ignore invalid color
            }
        }

        return (isDarkTheme ?? OSThemeHelper.AppsUseDarkTheme())
            ? Color.FromRgb(0x17, 0x17, 0x17)
            : Color.FromRgb(0xF2, 0xF2, 0xF2);
    }

    public static double GetAcrylic10TintOpacity()
    {
        var acrylicTintOpacity = 0.7d;
        return acrylicTintOpacity;
    }

    public static Brush GetAcrylic10TintLuminosityOpacityBackground(bool isDarkTheme)
    {
        var acrylicTintLuminosityOpacity = 0.44d;
        var t = acrylicTintLuminosityOpacity * (isDarkTheme ? 0.6d : 1.25d);
        var v = isDarkTheme ? (byte)0x22 : (byte)0xE1;
        var brush = new SolidColorBrush(Color.FromArgb((byte)Math.Round(t * 255d * 0.6d), v, v, v));
        brush.Freeze();
        return brush;
    }
}
