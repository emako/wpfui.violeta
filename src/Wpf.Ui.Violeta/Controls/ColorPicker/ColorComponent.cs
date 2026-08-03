namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines a specific component within a color model.
/// </summary>
public enum ColorComponent
{
    /// <summary>Represents the alpha component.</summary>
    Alpha = 0,

    /// <summary>First color component: Red when RGB or Hue when HSV.</summary>
    Component1 = 1,

    /// <summary>Second color component: Green when RGB or Saturation when HSV.</summary>
    Component2 = 2,

    /// <summary>Third color component: Blue when RGB or Value when HSV.</summary>
    Component3 = 3,
}
