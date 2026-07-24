namespace Wpf.Ui.Violeta.Win32;

public enum WindowBackdropPreference
{
    /// <summary>
    /// No backdrop effect.
    /// </summary>
    None,

    /// <summary>
    /// Sets <c>DWMWA_SYSTEMBACKDROP_TYPE</c> to <see langword="0"></see>.
    /// </summary>
    Auto,

    /// <summary>
    /// Windows 11 Mica effect.
    /// </summary>
    Mica,

    /// <summary>
    /// Windows Acrylic effect.
    /// Automatically selects the best Acrylic effect available on the system (Acrylic11 > Acrylic10).
    /// </summary>
    Acrylic,

    /// <summary>
    /// Windows 11 wallpaper blur effect.
    /// </summary>
    Tabbed,

    /// <summary>
    /// Windows Acrylic effect.
    /// Windows 10 style, supported on Windows 10 and 11.
    /// The value here is defined by violeta and should not be used for system DWM calls.
    /// </summary>
    Acrylic10 = 0xEE,

    /// <summary>
    /// Windows Acrylic effect.
    /// Windows 11 style, supported on Windows 11 22523+ (Insider) and 22621+ (Stable).
    /// The value here is defined by violeta and should not be used for system DWM calls.
    /// </summary>
    Acrylic11 = 0xEF,
}
