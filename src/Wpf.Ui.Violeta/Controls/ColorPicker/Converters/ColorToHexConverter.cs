using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Converts a color to a hex string and vice versa.
/// </summary>
public class ColorToHexConverter : IValueConverter
{
    public bool IsAlphaVisible { get; set; } = true;

    public AlphaComponentPosition AlphaPosition { get; set; } = AlphaComponentPosition.Leading;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color color;
        bool includeSymbol = parameter as bool? ?? false;

        if (value is Color valueColor)
            color = valueColor;
        else if (value is HsvColor valueHsvColor)
            color = valueHsvColor.ToRgb();
        else if (value is SolidColorBrush valueBrush)
            color = valueBrush.Color;
        else
            return DependencyProperty.UnsetValue;

        return ToHexString(color, AlphaPosition, IsAlphaVisible, includeSymbol);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string hexValue = value?.ToString() ?? string.Empty;
        return ParseHexString(hexValue, AlphaPosition) ?? DependencyProperty.UnsetValue;
    }

    public static string ToHexString(
        Color color,
        AlphaComponentPosition alphaPosition,
        bool includeAlpha = true,
        bool includeSymbol = false)
    {
        uint intColor;
        string hexColor;

        if (includeAlpha)
        {
            if (alphaPosition == AlphaComponentPosition.Trailing)
                intColor = ((uint)color.R << 24) | ((uint)color.G << 16) | ((uint)color.B << 8) | color.A;
            else
                intColor = ((uint)color.A << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;

            hexColor = intColor.ToString("x8", CultureInfo.InvariantCulture).ToUpperInvariant();
        }
        else
        {
            intColor = ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
            hexColor = intColor.ToString("x6", CultureInfo.InvariantCulture).ToUpperInvariant();
        }

        if (includeSymbol)
            hexColor = '#' + hexColor;

        return hexColor;
    }

    [SuppressMessage("Performance", "CA1865:Use char overload")]
    public static Color? ParseHexString(string hexColor, AlphaComponentPosition alphaPosition)
    {
        hexColor = hexColor.Trim();

        if (!hexColor.StartsWith("#", StringComparison.Ordinal))
            hexColor = "#" + hexColor;

        if (TryParseHexFormat(hexColor.AsSpan(), alphaPosition, out Color color))
            return color;

        return null;
    }

    [SuppressMessage("Style", "IDE0057:Use range operator")]
    private static bool TryParseHexFormat(
        ReadOnlySpan<char> s,
        AlphaComponentPosition alphaPosition,
        out Color color)
    {
        color = default;

        if (s.Length < 2 || s[0] != '#')
            return false;

        ReadOnlySpan<char> input = s.Slice(1);

        if (input.Length == 3 || input.Length == 4)
        {
            var extendedLength = 2 * input.Length;
            Span<char> extended = stackalloc char[extendedLength];
            for (int i = 0; i < input.Length; i++)
            {
                extended[2 * i + 0] = input[i];
                extended[2 * i + 1] = input[i];
            }
            return TryParseCore(extended, alphaPosition, ref color);
        }

        return TryParseCore(input, alphaPosition, ref color);
    }

    private static bool TryParseCore(ReadOnlySpan<char> input, AlphaComponentPosition alphaPosition, ref Color color)
    {
        var alphaComponent = 0u;

        if (input.Length == 6)
        {
            alphaComponent = alphaPosition == AlphaComponentPosition.Trailing ? 0x000000FFu : 0xFF000000u;
        }
        else if (input.Length != 8)
        {
            return false;
        }

        if (!uint.TryParse(input.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
            return false;

        if (alphaComponent != 0)
        {
            if (alphaPosition == AlphaComponentPosition.Trailing)
                parsed = (parsed << 8) | alphaComponent;
            else
                parsed |= alphaComponent;
        }

        if (alphaPosition == AlphaComponentPosition.Trailing)
        {
            color = Color.FromArgb(
                (byte)(parsed & 0xFF),
                (byte)((parsed >> 24) & 0xFF),
                (byte)((parsed >> 16) & 0xFF),
                (byte)((parsed >> 8) & 0xFF));
        }
        else
        {
            color = Color.FromArgb(
                (byte)((parsed >> 24) & 0xFF),
                (byte)((parsed >> 16) & 0xFF),
                (byte)((parsed >> 8) & 0xFF),
                (byte)(parsed & 0xFF));
        }

        return true;
    }
}
