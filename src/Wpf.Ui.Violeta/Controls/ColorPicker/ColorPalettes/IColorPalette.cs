using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Interface to define a color palette.
/// </summary>
public interface IColorPalette
{
    /// <summary>Total number of colors (columns).</summary>
    public int ColorCount { get; }

    /// <summary>Total number of shades for each color (rows).</summary>
    public int ShadeCount { get; }

    /// <summary>Gets a color in the palette by index.</summary>
    public Color GetColor(int colorIndex, int shadeIndex);
}
