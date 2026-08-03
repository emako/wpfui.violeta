namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines the two HSV color components displayed by a <see cref="ColorSpectrum"/>.
/// Order corresponds with an X/Y axis in Box shape or a degree/radius in Ring shape.
/// </summary>
public enum ColorSpectrumComponents
{
    /// <summary>Hue (X/degrees) and Value (Y/radius).</summary>
    HueValue,

    /// <summary>Value (X/degrees) and Hue (Y/radius).</summary>
    ValueHue,

    /// <summary>Hue (X/degrees) and Saturation (Y/radius).</summary>
    HueSaturation,

    /// <summary>Saturation (X/degrees) and Hue (Y/radius).</summary>
    SaturationHue,

    /// <summary>Saturation (X/degrees) and Value (Y/radius).</summary>
    SaturationValue,

    /// <summary>Value (X/degrees) and Saturation (Y/radius).</summary>
    ValueSaturation,
}
