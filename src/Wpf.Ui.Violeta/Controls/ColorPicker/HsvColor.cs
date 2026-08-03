using System;
using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines a color using the hue/saturation/value (HSV) model.
/// </summary>
public readonly struct HsvColor : IEquatable<HsvColor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HsvColor"/> struct.
    /// </summary>
    public HsvColor(double alpha, double hue, double saturation, double value)
    {
        A = Clamp(alpha, 0.0, 1.0);
        H = Clamp(hue, 0.0, 360.0);
        S = Clamp(saturation, 0.0, 1.0);
        V = Clamp(value, 0.0, 1.0);
        H = H == 360.0 ? 0 : H;
    }

    /// <summary>
    /// Internal constructor used when component ranges are already known.
    /// </summary>
    internal HsvColor(double alpha, double hue, double saturation, double value, bool clampValues)
    {
        if (clampValues)
        {
            A = Clamp(alpha, 0.0, 1.0);
            H = Clamp(hue, 0.0, 360.0);
            S = Clamp(saturation, 0.0, 1.0);
            V = Clamp(value, 0.0, 1.0);
            H = H == 360.0 ? 0 : H;
        }
        else
        {
            A = alpha;
            H = hue;
            S = saturation;
            V = value;
        }
    }

    /// <summary>
    /// Initializes a new instance from an RGB <see cref="Color"/>.
    /// </summary>
    public HsvColor(Color color)
    {
        var hsv = color.ToHsv();
        A = hsv.A;
        H = hsv.H;
        S = hsv.S;
        V = hsv.V;
    }

    /// <summary>Alpha (transparency) in the range 0..1.</summary>
    public double A { get; }

    /// <summary>Hue in the range 0..360 degrees.</summary>
    public double H { get; }

    /// <summary>Saturation in the range 0..1.</summary>
    public double S { get; }

    /// <summary>Value (brightness) in the range 0..1.</summary>
    public double V { get; }

    /// <inheritdoc/>
    public bool Equals(HsvColor other) =>
        other.A == A && other.H == H && other.S == S && other.V == V;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is HsvColor hsvColor && Equals(hsvColor);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = A.GetHashCode();
            hashCode = (hashCode * 397) ^ H.GetHashCode();
            hashCode = (hashCode * 397) ^ S.GetHashCode();
            hashCode = (hashCode * 397) ^ V.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>Returns the RGB equivalent of this HSV color.</summary>
    public Color ToRgb() => ToRgb(H, S, V, A);

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("hsva(");
        sb.Append(H.ToString(CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(S.ToString(CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(V.ToString(CultureInfo.InvariantCulture));
        sb.Append(", ");
        sb.Append(A.ToString(CultureInfo.InvariantCulture));
        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>Parses an HSV color string.</summary>
    public static HsvColor Parse(string s)
    {
        if (s is null)
            throw new ArgumentNullException(nameof(s));

        if (TryParse(s, out HsvColor hsvColor))
            return hsvColor;

        throw new FormatException($"Invalid HSV color string: '{s}'.");
    }

    /// <summary>Attempts to parse an HSV color string.</summary>
    public static bool TryParse(string? s, out HsvColor hsvColor)
    {
        bool prefixMatched = false;
        hsvColor = default;

        if (s is null)
            return false;

        string workingString = s.Trim();

        if (workingString.Length == 0 || workingString.IndexOf(",", StringComparison.Ordinal) < 0)
            return false;

        if (workingString.Length >= 11 &&
            workingString.StartsWith("hsva(", StringComparison.OrdinalIgnoreCase) &&
            workingString.EndsWith(")", StringComparison.Ordinal))
        {
            workingString = workingString.Substring(5, workingString.Length - 6);
            prefixMatched = true;
        }

        if (!prefixMatched &&
            workingString.Length >= 10 &&
            workingString.StartsWith("hsv(", StringComparison.OrdinalIgnoreCase) &&
            workingString.EndsWith(")", StringComparison.Ordinal))
        {
            workingString = workingString.Substring(4, workingString.Length - 5);
            prefixMatched = true;
        }

        if (!prefixMatched)
            return false;

        string[] components = workingString.Split(',');

        if (components.Length == 3)
        {
            if (double.TryParse(components[0].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out double hue) &&
                TryInternalParse(components[1], out double saturation) &&
                TryInternalParse(components[2], out double value))
            {
                hsvColor = new HsvColor(1.0, hue, saturation, value);
                return true;
            }
        }
        else if (components.Length == 4)
        {
            if (double.TryParse(components[0].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out double hue) &&
                TryInternalParse(components[1], out double saturation) &&
                TryInternalParse(components[2], out double value) &&
                TryInternalParse(components[3], out double alpha))
            {
                hsvColor = new HsvColor(alpha, hue, saturation, value);
                return true;
            }
        }

        return false;

        static bool TryInternalParse(string inString, out double outDouble)
        {
            inString = inString.Trim();
            int percentIndex = inString.IndexOf('%');

            if (percentIndex >= 0)
            {
                var result = double.TryParse(inString.Substring(0, percentIndex), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out double percentage);
                outDouble = percentage / 100.0;
                return result;
            }

            return double.TryParse(inString, NumberStyles.Number, CultureInfo.InvariantCulture, out outDouble);
        }
    }

    /// <summary>Creates a new <see cref="HsvColor"/> from individual component values.</summary>
    public static HsvColor FromAhsv(double a, double h, double s, double v) => new(a, h, s, v);

    /// <summary>Creates a new opaque <see cref="HsvColor"/> from individual component values.</summary>
    public static HsvColor FromHsv(double h, double s, double v) => new(1.0, h, s, v);

    /// <summary>
    /// Converts the given HSVA color component values to their RGB color equivalent.
    /// </summary>
    public static Color ToRgb(double hue, double saturation, double value, double alpha = 1.0)
    {
        while (hue >= 360.0)
            hue -= 360.0;
        while (hue < 0.0)
            hue += 360.0;

        saturation = saturation < 0.0 ? 0.0 : saturation > 1.0 ? 1.0 : saturation;
        value = value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;
        alpha = alpha < 0.0 ? 0.0 : alpha > 1.0 ? 1.0 : alpha;

        var chroma = saturation * value;
        var min = value - chroma;

        if (chroma == 0)
        {
            return Color.FromArgb(
                (byte)Math.Round(alpha * 255),
                (byte)Math.Round(min * 255),
                (byte)Math.Round(min * 255),
                (byte)Math.Round(min * 255));
        }

        int sextant = (int)(hue / 60);
        double intermediateColorPercentage = (hue / 60) - sextant;
        double max = chroma + min;

        double r = 0, g = 0, b = 0;

        switch (sextant)
        {
            case 0:
                r = max;
                g = min + (chroma * intermediateColorPercentage);
                b = min;
                break;
            case 1:
                r = min + (chroma * (1 - intermediateColorPercentage));
                g = max;
                b = min;
                break;
            case 2:
                r = min;
                g = max;
                b = min + (chroma * intermediateColorPercentage);
                break;
            case 3:
                r = min;
                g = min + (chroma * (1 - intermediateColorPercentage));
                b = max;
                break;
            case 4:
                r = min + (chroma * intermediateColorPercentage);
                g = min;
                b = max;
                break;
            case 5:
                r = max;
                g = min;
                b = min + (chroma * (1 - intermediateColorPercentage));
                break;
        }

        return Color.FromArgb(
            (byte)Math.Round(alpha * 255),
            (byte)Math.Round(r * 255),
            (byte)Math.Round(g * 255),
            (byte)Math.Round(b * 255));
    }

    public static bool operator ==(HsvColor left, HsvColor right) => left.Equals(right);

    public static bool operator !=(HsvColor left, HsvColor right) => !(left == right);

    public static explicit operator Color(HsvColor hsvColor) => hsvColor.ToRgb();

    private static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;
}

/// <summary>
/// Color conversion helpers mirroring Avalonia.Media.Color HSV extensions.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Converts an RGB <see cref="Color"/> to <see cref="HsvColor"/>.
    /// </summary>
    public static HsvColor ToHsv(this Color color)
    {
        return ToHsv(color.R / 255.0, color.G / 255.0, color.B / 255.0, color.A / 255.0);
    }

    /// <summary>
    /// Converts normalized RGB components to <see cref="HsvColor"/>.
    /// </summary>
    public static HsvColor ToHsv(double r, double g, double b, double alpha = 1.0)
    {
        r = r < 0.0 ? 0.0 : r > 1.0 ? 1.0 : r;
        g = g < 0.0 ? 0.0 : g > 1.0 ? 1.0 : g;
        b = b < 0.0 ? 0.0 : b > 1.0 ? 1.0 : b;
        alpha = alpha < 0.0 ? 0.0 : alpha > 1.0 ? 1.0 : alpha;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double chroma = max - min;
        double hue;

        if (chroma == 0)
        {
            hue = 0;
        }
        else if (max == r)
        {
            hue = 60 * (((g - b) / chroma) % 6);
        }
        else if (max == g)
        {
            hue = 60 * (((b - r) / chroma) + 2);
        }
        else
        {
            hue = 60 * (((r - g) / chroma) + 4);
        }

        if (hue < 0)
            hue += 360;

        double saturation = max == 0 ? 0 : chroma / max;

        return new HsvColor(alpha, hue, saturation, max, clampValues: false);
    }

    /// <summary>
    /// Creates a <see cref="Color"/> from a packed ARGB uint (same as Avalonia Color.FromUInt32).
    /// </summary>
    public static Color FromUInt32(uint value) =>
        Color.FromArgb(
            (byte)((value >> 24) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF));
}
