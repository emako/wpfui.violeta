#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

// These classes are not intended to be viewmodels.
// They deal with the data about an editable palette and are passed to special purpose controls for editing
internal class ColorPaletteEntry : IColorPaletteEntry
{
    public ColorPaletteEntry(Color color)
    {
        ActiveColor = color;
    }

    public Color ActiveColor { get; }
}
