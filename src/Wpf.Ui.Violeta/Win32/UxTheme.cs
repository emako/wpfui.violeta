using System;
using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

internal static class UxTheme
{
    [DllImport("uxtheme.dll", EntryPoint = "#132", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool ShouldAppsUseDarkMode();

    /// <summary>
    /// Windows 10 1903, aka 18362, broke the API.
    ///  - Before 18362, the #135 is AllowDarkModeForApp(BOOL)
    ///  - After 18362, the #135 is SetPreferredAppMode(PreferredAppMode)
    /// Since the support for AllowDarkModeForApp is uncertain, it will not be considered for use.
    /// </summary>
    [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int SetPreferredAppMode(PreferredAppMode preferredAppMode);

    [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true, CharSet = CharSet.Unicode)]
    [Obsolete("Since the support for AllowDarkModeForApp is uncertain, it will not be considered for use.")]
    public static extern void AllowDarkModeForApp(bool allowDark);

    [DllImport("uxtheme.dll", EntryPoint = "#136", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern void FlushMenuThemes();

    [DllImport("uxtheme.dll", EntryPoint = "#138", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool ShouldSystemUseDarkMode();

    public enum PreferredAppMode : int { Default, AllowDark, ForceDark, ForceLight, Max };
}
