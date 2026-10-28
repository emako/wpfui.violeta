namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// How <see cref="MeterBar"/> arranges its legend.
/// </summary>
public enum MeterLegend
{
    /// <summary>
    /// Color, name, and value flow horizontally and wrap.
    /// </summary>
    Horizontal,

    /// <summary>
    /// One category per row, with the value on the right.
    /// </summary>
    Vertical,

    /// <summary>
    /// The legend is not shown. Segment tooltips remain available.
    /// </summary>
    None,
}
