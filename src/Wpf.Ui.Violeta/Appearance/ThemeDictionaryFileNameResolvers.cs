using Wpf.Ui.Appearance;

namespace Wpf.Ui.Violeta.Appearance;

/// <summary>
/// Common theme file name resolvers for <see cref="ThemeDictionaryRegistration"/>.
/// </summary>
public static class ThemeDictionaryFileNameResolvers
{
    /// <summary>
    /// Resolves <c>Light</c> or <c>Dark</c> only. High contrast is not supported.
    /// </summary>
    public static string LightDark(ApplicationTheme theme, SystemTheme systemTheme) =>
        theme == ApplicationTheme.Dark ? "Dark" : "Light";

    /// <summary>
    /// Resolves <c>Light</c> or <c>Dark</c>. High contrast maps to <c>Dark</c> for
    /// <see cref="SystemTheme.HCBlack"/> and <c>Light</c> otherwise.
    /// </summary>
    public static string LightDarkWithHighContrastFallback(ApplicationTheme theme, SystemTheme systemTheme)
    {
        if (theme == ApplicationTheme.Dark)
        {
            return "Dark";
        }

        if (theme == ApplicationTheme.HighContrast)
        {
            return systemTheme == SystemTheme.HCBlack ? "Dark" : "Light";
        }

        return "Light";
    }

    /// <summary>
    /// WPF UI style resolver with full high contrast theme file names.
    /// </summary>
    public static string WpfUi(ApplicationTheme theme, SystemTheme systemTheme)
    {
        if (theme == ApplicationTheme.Dark)
        {
            return "Dark";
        }

        if (theme != ApplicationTheme.HighContrast)
        {
            return "Light";
        }

        return systemTheme switch
        {
            SystemTheme.HC1 => "HC1",
            SystemTheme.HC2 => "HC2",
            SystemTheme.HCBlack => "HCBlack",
            SystemTheme.HCWhite => "HCWhite",
            _ => "HCWhite",
        };
    }
}
