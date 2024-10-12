using Microsoft.Win32;
using System;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Appearance;

public static class ThemeManager
{
    internal const string LibraryNamespace = "wpf.ui.violeta;";

    internal const string ThemesDictionaryPath = "pack://application:,,,/Wpf.Ui.Violeta;component/Resources/Theme/";

    public static void RegisterApplicationThemeChanged()
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

    public static ApplicationTheme GetSystemTheme()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        object? registryValueObject = key?.GetValue("AppsUseLightTheme");

        if (registryValueObject == null)
        {
            return ApplicationTheme.Light;
        }

        var registryValue = (int)registryValueObject;

        return registryValue > 0 ? ApplicationTheme.Light : ApplicationTheme.Dark;
    }

    public static void Apply(ApplicationTheme theme)
    {
        if (theme == ApplicationTheme.Unknown)
        {
            // To change `Unknown` to `System`.
            theme = GetSystemTheme();
        }

        if (ApplicationThemeManager.GetAppTheme() != theme)
        {
            ApplicationThemeManager.Apply(theme, WindowBackdropType.Mica, true);
        }
    }
}
