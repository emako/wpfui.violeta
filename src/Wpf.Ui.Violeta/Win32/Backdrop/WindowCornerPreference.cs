namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Win32 corner rounding preference (requires Windows 11+).
/// </summary>
public enum WindowCornerPreference
{
    /// <summary>
    /// Let the system decide whether or not to round window corners.
    /// Equivalent to DWMWCP_DEFAULT
    /// </summary>
    Default = 0,

    /// <summary>
    /// Never round window corners.
    /// Equivalent to DWMWCP_DONOTROUND
    /// </summary>
    DoNotRound = 1,

    /// <summary>
    /// Round the corners if appropriate.
    /// Equivalent to DWMWCP_ROUND
    /// </summary>
    Round = 2,

    /// <summary>
    /// Round the corners if appropriate, with a small radius.
    /// Equivalent to DWMWCP_ROUNDSMALL
    /// </summary>
    RoundSmall = 3,
}
