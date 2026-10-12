using System;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.Markdown.Appearance;

internal static class ThemeManager
{
    internal const string LibraryNamespace = "wpf.ui.markdown;";

    internal const string ThemesDictionaryPath = "pack://application:,,,/Wpf.Ui.Markdown;component/Resources/Theme/";

    public static void RegistApplicationThemeChanged()
    {
        ApplicationThemeManager.Changed -= ApplicationThemeManager_Changed;
        ApplicationThemeManager.Changed += ApplicationThemeManager_Changed;
    }

    private static void ApplicationThemeManager_Changed(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        // Only light and dark brushes exist; high contrast falls back to the closer one.
        var themeDictionaryName = currentApplicationTheme switch
        {
            ApplicationTheme.Dark => "Dark",
            ApplicationTheme.HighContrast => ApplicationThemeManager.GetSystemTheme() == SystemTheme.HCBlack ? "Dark" : "Light",
            _ => "Light",
        };

        ResourceDictionaryManager appDictionaries = new(LibraryNamespace);
        appDictionaries.UpdateDictionary("theme", new Uri(ThemesDictionaryPath + themeDictionaryName + ".xaml", UriKind.Absolute));
    }
}
