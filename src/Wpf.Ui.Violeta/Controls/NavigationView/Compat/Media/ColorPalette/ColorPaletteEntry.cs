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
