using System;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.TextEditor.Appearance;

internal static class ThemeManager
{
    internal const string LibraryNamespace = "wpf.ui.texteditor;";

    internal const string ThemesDictionaryPath = "pack://application:,,,/Wpf.Ui.TextEditor;component/Resources/Theme/";

    public static void RegistApplicationThemeChanged()
    {
        ApplicationThemeManager.Changed -= ApplicationThemeManager_Changed;
        ApplicationThemeManager.Changed += ApplicationThemeManager_Changed;
    }

    private static void ApplicationThemeManager_Changed(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        ResourceDictionaryManager appDictionaries = new(LibraryNamespace);

        string themeDictionaryName = "Light";

        switch (currentApplicationTheme)
        {
            case ApplicationTheme.Dark:
                themeDictionaryName = "Dark";
                break;

            case ApplicationTheme.HighContrast:
                themeDictionaryName = ApplicationThemeManager.GetSystemTheme() switch
                {
                    SystemTheme.HC1 => "HC1",
                    SystemTheme.HC2 => "HC2",
                    SystemTheme.HCBlack => "HCBlack",
                    SystemTheme.HCWhite => "HCWhite",
                    _ => "HCWhite",
                };
                break;
        }

        // Only support light and dark themes, no more than that.
        // So we need to fall back to either light or dark mode.
        if (themeDictionaryName != "Light" && themeDictionaryName != "Dark")
        {
            if (themeDictionaryName == "HCBlack")
            {
                themeDictionaryName = "Dark";
            }
            else
            {
                themeDictionaryName = "Light";
            }
        }

        bool isUpdated = appDictionaries.UpdateDictionary(
            "theme",
            new Uri(ThemesDictionaryPath + themeDictionaryName + ".xaml", UriKind.Absolute)
        );

        if (!isUpdated)
        {
            return;
        }

        return;
    }
}
