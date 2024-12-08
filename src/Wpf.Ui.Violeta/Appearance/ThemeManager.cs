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

    /// <summary>
    /// Get the theme of the application (<seealso cref="ApplicationThemeManager.GetAppTheme"/>).
    /// </summary>
    /// <returns>
    /// Only the following enum will be returned.
    /// <para><see cref="ApplicationTheme.Dark"/></para>
    /// <para><see cref="ApplicationTheme.Light"/></para>
    /// </returns>
    public static ApplicationTheme GetApplicationTheme()
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

    /// <summary>
    /// Get the theme of the system (<seealso cref="SystemThemeManager.GetCachedSystemTheme"/>).
    /// </summary>
    /// <returns>
    /// Only the following enum will be returned.
    /// <para><see cref="SystemTheme.Dark"/></para>
    /// <para><see cref="SystemTheme.Light"/></para>
    /// </returns>
    public static SystemTheme GetSystemTheme()
    {
        return Get() switch
        {
            SystemTheme.Dark or SystemTheme.HCBlack or SystemTheme.Glow or SystemTheme.CapturedMotion => SystemTheme.Dark,
            _ => SystemTheme.Light,
        };

        static SystemTheme Get()
        {
            var currentTheme =
                Registry.GetValue(
                    "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes",
                    "CurrentTheme",
                    "aero.theme"
                ) as string
                ?? string.Empty;

            if (!string.IsNullOrEmpty(currentTheme))
            {
                currentTheme = currentTheme.ToLower().Trim();

                // This may be changed in the next versions, check the Insider previews
                if (currentTheme.Contains("basic.theme"))
                {
                    return SystemTheme.Light;
                }

                if (currentTheme.Contains("aero.theme"))
                {
                    return SystemTheme.Light;
                }

                if (currentTheme.Contains("dark.theme"))
                {
                    return SystemTheme.Dark;
                }

                if (currentTheme.Contains("hcblack.theme"))
                {
                    return SystemTheme.HCBlack;
                }

                if (currentTheme.Contains("hcwhite.theme"))
                {
                    return SystemTheme.HCWhite;
                }

                if (currentTheme.Contains("hc1.theme"))
                {
                    return SystemTheme.HC1;
                }

                if (currentTheme.Contains("hc2.theme"))
                {
                    return SystemTheme.HC2;
                }

                if (currentTheme.Contains("themea.theme"))
                {
                    return SystemTheme.Glow;
                }

                if (currentTheme.Contains("themeb.theme"))
                {
                    return SystemTheme.CapturedMotion;
                }

                if (currentTheme.Contains("themec.theme"))
                {
                    return SystemTheme.Sunrise;
                }

                if (currentTheme.Contains("themed.theme"))
                {
                    return SystemTheme.Flow;
                }
            }

            /*if (currentTheme.Contains("custom.theme"))
                return ; custom can be light or dark*/
            var rawSystemUsesLightTheme =
                Registry.GetValue(
                    "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
                    "SystemUsesLightTheme",
                    1
                ) ?? 1;

            return rawSystemUsesLightTheme is 0 ? SystemTheme.Dark : SystemTheme.Light;
        }
    }

    public static void Apply(ApplicationTheme theme)
    {
        if (theme == ApplicationTheme.Unknown)
        {
            // To change `Unknown` to `System`.
            theme = GetApplicationTheme();
        }

        if (ApplicationThemeManager.GetAppTheme() != theme)
        {
            ApplicationThemeManager.Apply(theme, WindowBackdropType.Mica, true);
        }
    }
}
