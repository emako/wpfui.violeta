using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines a specific component in the RGB color model.
/// </summary>
public enum RgbComponent
{
    /// <summary>The Alpha component. See <see cref="Color.A"/>.</summary>
    Alpha = 0,

    /// <summary>The Red component. See <see cref="Color.R"/>.</summary>
    Red = 1,

    /// <summary>The Green component. See <see cref="Color.G"/>.</summary>
    Green = 2,

    /// <summary>The Blue component. See <see cref="Color.B"/>.</summary>
    Blue = 3,
}
