using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.ColorPickerHelpers;

/// <summary>
/// Contains helpers useful when working with colors.
/// </summary>
public static class ColorHelper
{
    private static readonly Dictionary<HsvColor, string> _cachedDisplayNames = [];
    private static readonly List<(string Name, Color Color, HsvColor Hsv)> _namedColors = [];
    private static readonly object _displayNameCacheMutex = new();
    private static readonly object _knownColorCacheMutex = new();

    /// <summary>
    /// Gets the relative (perceptual) luminance/brightness of the given color.
    /// 1 is closer to white while 0 is closer to black.
    /// </summary>
    public static double GetRelativeLuminance(Color color)
    {
        double rg = color.R <= 10 ? color.R / 3294.0 : Math.Pow(color.R / 269.0 + 0.0513, 2.4);
        double gg = color.G <= 10 ? color.G / 3294.0 : Math.Pow(color.G / 269.0 + 0.0513, 2.4);
        double bg = color.B <= 10 ? color.B / 3294.0 : Math.Pow(color.B / 269.0 + 0.0513, 2.4);
        return 0.2126 * rg + 0.7152 * gg + 0.0722 * bg;
    }

    /// <summary>
    /// Determines if color display names are supported based on the current thread culture.
    /// </summary>
    public static bool ToDisplayNameExists =>
        CultureInfo.CurrentUICulture.Name.StartsWith("EN", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines an approximate display name for the given color.
    /// </summary>
    public static string ToDisplayName(Color color)
    {
        var hsvColor = color.ToHsv();

        if (color.A == 0x00)
            return "Transparent";

        var roundedHsvColor = new HsvColor(
            1.0,
            Math.Round(hsvColor.H, 0),
            Math.Round(hsvColor.S, 1),
            Math.Round(hsvColor.V, 1));

        lock (_displayNameCacheMutex)
        {
            if (_cachedDisplayNames.TryGetValue(roundedHsvColor, out var displayName))
                return displayName;
        }

        EnsureNamedColors();

        string closestName = string.Empty;
        var closestDistance = double.PositiveInfinity;

        foreach (var named in _namedColors)
        {
            if (named.Name == "Transparent")
                continue;

            double distance = Math.Sqrt(
                Math.Pow(roundedHsvColor.H - named.Hsv.H, 2.0) +
                Math.Pow(roundedHsvColor.S - named.Hsv.S, 2.0) +
                Math.Pow(roundedHsvColor.V - named.Hsv.V, 2.0));

            if (distance < closestDistance)
            {
                closestName = named.Name;
                closestDistance = distance;
            }
        }

        if (!string.IsNullOrEmpty(closestName))
        {
            lock (_displayNameCacheMutex)
            {
                _cachedDisplayNames[roundedHsvColor] = closestName;
            }
            return closestName;
        }

        return string.Empty;
    }

    private static void EnsureNamedColors()
    {
        lock (_knownColorCacheMutex)
        {
            if (_namedColors.Count > 0)
                return;

            foreach (PropertyInfo prop in typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (prop.PropertyType != typeof(Color))
                    continue;

                var c = (Color)prop.GetValue(null)!;
                string name = SplitPascalCase(prop.Name);
                _namedColors.Add((name, c, c.ToHsv()));
            }
        }
    }

    private static string SplitPascalCase(string name)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i != 0 && char.IsUpper(name[i]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }
}
