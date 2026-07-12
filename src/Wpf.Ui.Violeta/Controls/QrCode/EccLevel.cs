namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Indicates the level of error correction available in case of data loss or corruption.
/// The higher the correction level, the more data will be included in the QRCode.
/// </summary>
public enum EccLevel
{
    /// <summary>
    /// The lowest level of error correction where up to ~7% of data can be recovered if lost.
    /// </summary>
    Lowest,

    /// <summary>
    /// The standard level of error correction where up to ~15% of data can be recovered if lost.
    /// </summary>
    Medium,

    /// <summary>
    /// A high readability level of error correction where up to ~25% of data can be recovered if lost.
    /// </summary>
    Quality,

    /// <summary>
    /// The maximum level of error correction where up to ~30% of data can be recovered if lost.
    /// </summary>
    Highest,
}
