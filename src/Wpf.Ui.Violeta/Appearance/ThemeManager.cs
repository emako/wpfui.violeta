using Microsoft.Win32;
using System;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Appearance;

public static class ThemeManager
{
    internal const string LibraryNamespace = "wpf.ui.violeta;";

    internal const string ThemesDictionaryPath = "pack://application:,,,/Wpf.Ui.Violeta;component/Resources/Theme/";

    public static void RegisterApplicationThemeChanged()
    {
        ApplicationThemeManager.Changed -= OnApplicationThemeManagerChanged;
        ApplicationThemeManager.Changed += OnApplicationThemeManagerChanged;
    }

    private static void OnApplicationThemeManagerChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
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
        uint dataSize = sizeof(uint);
        int result = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "AppsUseLightTheme",
            AdvApi32.RRF_RT_REG_DWORD, IntPtr.Zero, out uint data, ref dataSize);

        if (result != 0)
        {
            return ApplicationTheme.Light;
        }

        return data > 0 ? ApplicationTheme.Light : ApplicationTheme.Dark;
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
            StringBuilder themeBuilder = new(520);
            uint themeDataSize = (uint)(themeBuilder.Capacity * sizeof(char));
            int themeResult = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes",
                "CurrentTheme",
                AdvApi32.RRF_RT_REG_SZ, IntPtr.Zero, themeBuilder, ref themeDataSize);

            var currentTheme = themeResult == 0 ? themeBuilder.ToString() : "aero.theme";

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
            uint dataSize = sizeof(uint);
            int result = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "SystemUsesLightTheme",
                AdvApi32.RRF_RT_REG_DWORD, IntPtr.Zero, out uint data, ref dataSize);

            var rawSystemUsesLightTheme = result == 0 ? data : 1u;

            return rawSystemUsesLightTheme is 0 ? SystemTheme.Dark : SystemTheme.Light;
        }
    }

    public static bool AppsUseDarkTheme()
    {
        uint dataSize = sizeof(uint);
        int result = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "AppsUseLightTheme",
            AdvApi32.RRF_RT_REG_DWORD, IntPtr.Zero, out uint data, ref dataSize);

        if (result != 0)
            return true;

        return data == 0;
    }

    public static bool SystemUsesDarkTheme()
    {
        uint dataSize = sizeof(uint);
        int result = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "SystemUsesLightTheme",
            AdvApi32.RRF_RT_REG_DWORD, IntPtr.Zero, out uint data, ref dataSize);

        if (result != 0)
            return true;

        return data == 0;
    }

    public static void Apply(ApplicationTheme theme, WindowBackdropPreference backgroundEffect = WindowBackdropPreference.Mica, bool updateAccent = true)
    {
        if (theme == ApplicationTheme.Unknown)
        {
            // To change `Unknown` to `System` as default.
            // If you want to follow the system theme, simply call `TrackSystemThemeChanges(isTracked: true);`.
            theme = GetApplicationTheme();
        }

        if (ApplicationThemeManager.GetAppTheme() != theme)
        {
            ApplicationThemeManager.Apply(theme, (WindowBackdropType)backgroundEffect, updateAccent);
        }
    }

    public static void TrackSystemThemeChanges(bool isTracked = true)
    {
        if (isTracked)
        {
            SystemEvents.UserPreferenceChanged -= OnSystemThemeChanged;
            SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
        }
        else
        {
            SystemEvents.UserPreferenceChanged -= OnSystemThemeChanged;
        }

        static void OnSystemThemeChanged(object? sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => Apply(ApplicationTheme.Unknown));
        }
    }
}
