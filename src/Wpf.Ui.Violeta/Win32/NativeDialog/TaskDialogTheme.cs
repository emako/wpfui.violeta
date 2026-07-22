namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Specifies the visual theme applied to task dialogs.
/// </summary>
public enum TaskDialogTheme
{
    /// <summary>
    /// Follow the Windows apps-use-light-theme preference read from the registry.
    /// </summary>
    /// <remarks>
    /// The preference is resolved live from
    /// HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme.
    /// A value of 0 enables dark mode; any other value uses the light theme.
    /// Dark mode requires Windows 10 version 1809 (build 17763) or later; on older systems the light theme is used.
    /// </remarks>
    System,

    /// <summary>
    /// Force dark regardless of OS setting.
    /// </summary>
    /// <remarks>
    /// Dark mode requires Windows 10 version 1809 (build 17763) or later; on older systems the light theme is used.
    /// </remarks>
    Dark,

    /// <summary>
    /// Force light regardless of OS setting
    /// </summary>
    Light,
}
