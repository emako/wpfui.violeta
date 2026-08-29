using System;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Appearance;

/// <summary>
/// Converts <see cref="Color"/> values between light and dark themes.
/// </summary>
/// <remarks>
/// <para>
/// WinUI 3 itself does not expose a public “light → dark” color transform; theme brushes are
/// authored as pairs in <c>ThemeDictionaries</c>. This approach ports the closest Microsoft algorithms:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="IsColorLight"/> — Windows / WinUI theme guidance
/// (<c>((5*G)+(2*R)+B) &gt; (8*128)</c>).
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="ToDark"/> / <see cref="ToLight"/> — Office / roosterjs <c>getDarkColor</c>
/// LAB lightness remapping (preserves a/b, remaps L* against a dark base such as #333333).
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="CreateAccentPalette"/> — Fluent XAML Theme Editor 7-step accent ramp
/// (same approach used for <c>SystemAccentColorLight*</c> / <c>SystemAccentColorDark*</c>).
/// </description>
/// </item>
/// </list>
/// </remarks>
public static class ThemeColorApproach
{
    /// <summary>
    /// CIE L* of <c>#333333</c>, the default dark-mode base used by Office / roosterjs.
    /// </summary>
    public const double DefaultDarkBaseL = 21.247;

    /// <summary>
    /// Classifies whether a color is perceptually “light” using the Windows theme formula.
    /// </summary>
    /// <remarks>
    /// Equivalent to the <c>IsColorLight</c> helper in Microsoft docs for detecting dark mode
    /// from <c>UISettings.GetColorValue(UIColorType.Foreground)</c>.
    /// </remarks>
    public static bool IsColorLight(Color color)
    {
        return ((5 * color.G) + (2 * color.R) + color.B) > (8 * 128);
    }

    /// <summary>
    /// Maps a light-theme color to its dark-theme counterpart via LAB lightness remapping.
    /// </summary>
    /// <param name="color">Source color (typically authored for light mode).</param>
    /// <param name="baseL">
    /// Target dark-base L* in CIE LAB. Defaults to <see cref="DefaultDarkBaseL"/> (#333333).
    /// Lower values produce a darker overall palette.
    /// </param>
    /// <returns>The remapped color with the original alpha preserved.</returns>
    public static Color ToDark(Color color, double baseL = DefaultDarkBaseL)
    {
        ValidateBaseL(baseL);

        var lab = ColorUtils.RGBToLAB(color, round: false);
        double newL = RemapLightnessToDark(lab.L, baseL);
        return LabToColor(newL, lab.A, lab.B, color.A);
    }

    /// <summary>
    /// Inverse of <see cref="ToDark"/>: maps a dark-theme color back toward light-theme space.
    /// </summary>
    public static Color ToLight(Color color, double baseL = DefaultDarkBaseL)
    {
        ValidateBaseL(baseL);

        var lab = ColorUtils.RGBToLAB(color, round: false);
        double newL = RemapLightnessToLight(lab.L, baseL);
        return LabToColor(newL, lab.A, lab.B, color.A);
    }

    /// <summary>
    /// Adapts <paramref name="color"/> for the requested theme.
    /// </summary>
    public static Color Adapt(Color color, bool toDark, double baseL = DefaultDarkBaseL)
    {
        return toDark ? ToDark(color, baseL) : ToLight(color, baseL);
    }

    /// <summary>
    /// Builds a 7-step Fluent accent ramp from a base accent color
    /// (Light3 … Light1, Accent, Dark1 … Dark3).
    /// </summary>
    public static AccentColorPalette CreateAccentPalette(Color accent)
    {
        var scale = new ColorPalette(7, accent);
        var entries = scale.Palette;

        return new AccentColorPalette(
            Light3: entries[0].ActiveColor,
            Light2: entries[1].ActiveColor,
            Light1: entries[2].ActiveColor,
            Accent: entries[3].ActiveColor,
            Dark1: entries[4].ActiveColor,
            Dark2: entries[5].ActiveColor,
            Dark3: entries[6].ActiveColor);
    }

    /// <summary>
    /// Picks the accent shade commonly used as the primary brush in the given theme
    /// (WinUI typically uses Light2 in dark mode and Dark1 in light mode).
    /// </summary>
    public static Color GetPrimaryAccent(Color accent, bool isDarkTheme)
    {
        var palette = CreateAccentPalette(accent);
        return isDarkTheme ? palette.Light2 : palette.Dark1;
    }

    private static double RemapLightnessToDark(double lightL, double baseL)
    {
        // newL = (100 - L) * ((100 - baseL) / 100) + baseL
        double newL = (100.0 - lightL) * ((100.0 - baseL) / 100.0) + baseL;
        return Clamp(newL, 0.0, 100.0);
    }

    private static double RemapLightnessToLight(double darkL, double baseL)
    {
        // Inverse of RemapLightnessToDark.
        double newL = 100.0 - ((darkL - baseL) * 100.0 / (100.0 - baseL));
        return Clamp(newL, 0.0, 100.0);
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private static Color LabToColor(double l, double a, double b, byte alpha)
    {
        var rgb = ColorUtils.LABToRGB(new LAB(l, a, b, round: false), round: false);
        return rgb.Denormalize(alpha);
    }

    private static void ValidateBaseL(double baseL)
    {
        if (baseL is < 0.0 or >= 100.0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseL), baseL, "baseL must be in [0, 100).");
        }
    }
}

/// <summary>
/// Fluent-style 7-step accent color ramp (Light3 → Dark3).
/// </summary>
/// <param name="Light3">Lightest shade (<c>SystemAccentColorLight3</c>).</param>
/// <param name="Light2">Second-lightest shade (<c>SystemAccentColorLight2</c>).</param>
/// <param name="Light1">Light shade (<c>SystemAccentColorLight1</c>).</param>
/// <param name="Accent">Base accent (<c>SystemAccentColor</c>).</param>
/// <param name="Dark1">Dark shade (<c>SystemAccentColorDark1</c>).</param>
/// <param name="Dark2">Second-darkest shade (<c>SystemAccentColorDark2</c>).</param>
/// <param name="Dark3">Darkest shade (<c>SystemAccentColorDark3</c>).</param>
public readonly record struct AccentColorPalette(
    Color Light3,
    Color Light2,
    Color Light1,
    Color Accent,
    Color Dark1,
    Color Dark2,
    Color Dark3);
