using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Holds the details of a ColorChanged event.
/// </summary>
/// <remarks>
/// HSV color information is intentionally not provided.
/// Use <see cref="ColorExtensions.ToHsv"/> to obtain it.
/// </remarks>
public class ColorChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorChangedEventArgs"/> class.
    /// </summary>
    [SuppressMessage("Style", "IDE0290:Use primary constructor")]
    public ColorChangedEventArgs(Color oldColor, Color newColor)
    {
        OldColor = oldColor;
        NewColor = newColor;
    }

    /// <summary>Gets the old/original color from before the change event.</summary>
    public Color OldColor { get; }

    /// <summary>Gets the new/updated color that triggered the change event.</summary>
    public Color NewColor { get; }
}
