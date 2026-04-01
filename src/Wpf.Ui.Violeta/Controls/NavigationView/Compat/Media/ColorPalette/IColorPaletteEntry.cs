using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

// These classes are not intended to be viewmodels.
// They deal with the data about an editable palette and are passed to special purpose controls for editing
internal interface IColorPaletteEntry
{
    Color ActiveColor { get; }
}
