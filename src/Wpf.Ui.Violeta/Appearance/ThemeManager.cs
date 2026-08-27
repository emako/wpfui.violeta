using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Appearance;

public static class ThemeManager
{
    private static readonly List<ThemeDictionaryRegistration> RegistrationsInternal = [];

    private static ReadOnlyCollection<ThemeDictionaryRegistration>? _registrations;

    private static ApplicationTheme _cachedApplicationTheme = ApplicationTheme.Unknown;

    private static bool _applying;

    static ThemeManager()
    {
        Register(ThemeDictionaryRegistration.DefaultWpfUi);
        Register(ThemeDictionaryRegistration.DefaultVioleta);
    }

    /// <summary>
    /// Gets the currently registered theme dictionary sources.
    /// </summary>
    public static ReadOnlyCollection<ThemeDictionaryRegistration> Registrations =>
        _registrations ??= new ReadOnlyCollection<ThemeDictionaryRegistration>(RegistrationsInternal);

    /// <summary>
    /// Raised after registered theme dictionaries have been swapped.
    /// </summary>
    public static event ThemeChangedEvent? Changed;

    /// <summary>
    /// Registers a theme dictionary source that <see cref="Apply"/> will update.
    /// </summary>
    public static void Register(ThemeDictionaryRegistration registration)
    {
        _ = registration ?? throw new ArgumentNullException(nameof(registration));

        lock (RegistrationsInternal)
        {
            if (!RegistrationsInternal.Contains(registration))
            {
                RegistrationsInternal.Add(registration);
            }
        }
    }

    /// <summary>
    /// Removes a previously registered theme dictionary source.
    /// </summary>
    public static bool Unregister(ThemeDictionaryRegistration registration)
    {
        _ = registration ?? throw new ArgumentNullException(nameof(registration));

        lock (RegistrationsInternal)
        {
            return RegistrationsInternal.Remove(registration);
        }
    }

    /// <summary>
    /// Removes all registrations that match <paramref name="searchNamespace"/>.
    /// </summary>
    public static int Unregister(string searchNamespace)
    {
        if (string.IsNullOrWhiteSpace(searchNamespace))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchNamespace));
        }

        lock (RegistrationsInternal)
        {
            return RegistrationsInternal.RemoveAll(
                registration => string.Equals(
                    registration.SearchNamespace,
                    searchNamespace,
                    StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void RegisterApplicationThemeChanged()
    {
        ApplicationThemeManager.Changed -= OnApplicationThemeManagerChanged;
        ApplicationThemeManager.Changed += OnApplicationThemeManagerChanged;
    }

    private static void OnApplicationThemeManagerChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        if (_applying)
        {
            return;
        }

        // Fallback when an app still calls ApplicationThemeManager.Apply.
        UpdateDictionary(currentApplicationTheme);
        _cachedApplicationTheme = currentApplicationTheme;
        Changed?.Invoke(currentApplicationTheme, systemAccent);
    }

    /// <summary>
    /// Gets the currently applied application theme (light / dark / high contrast).
    /// </summary>
    public static ApplicationTheme GetAppTheme()
    {
        if (_cachedApplicationTheme == ApplicationTheme.Unknown)
        {
            FetchApplicationTheme();
        }

        return _cachedApplicationTheme;
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
    /// Gets the Windows system theme, collapsed to light or dark.
    /// </summary>
    /// <returns>
    /// Only the following enum will be returned.
    /// <para><see cref="SystemTheme.Dark"/></para>
    /// <para><see cref="SystemTheme.Light"/></para>
    /// </returns>
    public static SystemTheme GetSystemTheme()
    {
        return ReadSystemTheme() switch
        {
            SystemTheme.Dark or SystemTheme.HCBlack or SystemTheme.Glow or SystemTheme.CapturedMotion => SystemTheme.Dark,
            _ => SystemTheme.Light,
        };
    }

    /// <summary>
    /// Reads the raw Windows theme from the registry, including high contrast and packed themes.
    /// </summary>
    private static SystemTheme ReadSystemTheme()
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

        uint dataSize = sizeof(uint);
        int result = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "SystemUsesLightTheme",
            AdvApi32.RRF_RT_REG_DWORD, IntPtr.Zero, out uint data, ref dataSize);

        var rawSystemUsesLightTheme = result == 0 ? data : 1u;

        return rawSystemUsesLightTheme is 0 ? SystemTheme.Dark : SystemTheme.Light;
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

    /// <summary>
    /// Applies light / dark / high contrast to all registered theme resource dictionaries.
    /// Window backdrop is left to each <c>ShellWindow.WindowBackdropType</c>.
    /// </summary>
    public static void Apply(ApplicationTheme theme, bool updateAccent = true)
    {
        if (theme == ApplicationTheme.Unknown)
        {
            theme = GetApplicationTheme();
        }

        if (GetAppTheme() == theme)
        {
            return;
        }

        ApplyCore(theme, updateAccent);
    }

    /// <summary>
    /// Applies the Windows system theme (including high contrast).
    /// </summary>
    public static void ApplySystemTheme(bool updateAccent = true)
    {
        SystemTheme systemTheme = ReadSystemTheme();
        ApplicationTheme themeToSet = ApplicationTheme.Light;

        if (systemTheme is SystemTheme.Dark or SystemTheme.CapturedMotion or SystemTheme.Glow)
        {
            themeToSet = ApplicationTheme.Dark;
        }
        else if (systemTheme is SystemTheme.HC1 or SystemTheme.HC2 or SystemTheme.HCBlack or SystemTheme.HCWhite)
        {
            themeToSet = ApplicationTheme.HighContrast;
        }

        Apply(themeToSet, updateAccent);
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
            Dispatcher.CurrentDispatcher.Invoke(() => ApplySystemTheme());
        }
    }

    private static void ApplyCore(ApplicationTheme theme, bool updateAccent)
    {
        if (_applying)
        {
            return;
        }

        _applying = true;
        try
        {
            if (updateAccent)
            {
                ApplicationAccentColorManager.Apply(
                    ApplicationAccentColorManager.GetColorizationColor(),
                    theme,
                    systemGlassColor: false);
            }

            UpdateDictionary(theme);

            _cachedApplicationTheme = theme;

            Color accent = ApplicationAccentColorManager.SystemAccent;
            Changed?.Invoke(theme, accent);

            // Keeps WPF UI's private theme cache and <see cref="ApplicationThemeManager.Changed"/> in sync
            // so FluentWindow / accent manager / TextEditor still observe the new theme.
#if false
            try
            {
                typeof(ApplicationThemeManager)
                    .GetField("_cachedApplicationTheme", BindingFlags.Static | BindingFlags.NonPublic)
                    ?.SetValue(null, theme);
            }
            catch
            {
                // WPF UI internals may change; dictionaries are already swapped.
            }

            try
            {
                if (typeof(ApplicationThemeManager)
                        .GetField(nameof(ApplicationThemeManager.Changed), BindingFlags.Static | BindingFlags.NonPublic)
                        ?.GetValue(null) is ThemeChangedEvent handler)
                {
                    handler.Invoke(theme, accent);
                }
            }
            catch
            {
                // WPF UI internals may change; Violeta listeners already received Changed.
            }
#endif
        }
        finally
        {
            _applying = false;
        }
    }

    private static void UpdateDictionary(ApplicationTheme theme)
    {
        SystemTheme systemTheme = ReadSystemTheme();

        ThemeDictionaryRegistration[] registrations;
        lock (RegistrationsInternal)
        {
            registrations = [.. RegistrationsInternal];
        }

        foreach (ThemeDictionaryRegistration registration in registrations)
        {
            UpdateDictionary(registration, theme, systemTheme);
        }
    }

    private static void UpdateDictionary(
        ThemeDictionaryRegistration registration,
        ApplicationTheme theme,
        SystemTheme systemTheme)
    {
        _ = new ResourceDictionaryManager(registration.SearchNamespace).UpdateDictionary(
            registration.DictionaryLookup,
            registration.GetThemeUri(theme, systemTheme));
    }

    private static void FetchApplicationTheme()
    {
        ThemeDictionaryRegistration[] registrations;
        lock (RegistrationsInternal)
        {
            registrations = [.. RegistrationsInternal];
        }

        foreach (ThemeDictionaryRegistration registration in registrations)
        {
            ResourceDictionary? themeDictionary =
                new ResourceDictionaryManager(registration.SearchNamespace).GetDictionary(registration.DictionaryLookup);

            if (themeDictionary?.Source is null)
            {
                continue;
            }

            if (TryInferApplicationTheme(themeDictionary.Source.ToString(), out ApplicationTheme inferredTheme))
            {
                _cachedApplicationTheme = inferredTheme;
                return;
            }
        }
    }

    private static bool TryInferApplicationTheme(string themeUri, out ApplicationTheme theme)
    {
        theme = ApplicationTheme.Unknown;

        if (themeUri.Contains("dark", StringComparison.OrdinalIgnoreCase))
        {
            theme = ApplicationTheme.Dark;
            return true;
        }

        if (themeUri.Contains("hc1", StringComparison.OrdinalIgnoreCase)
            || themeUri.Contains("hc2", StringComparison.OrdinalIgnoreCase)
            || themeUri.Contains("hcblack", StringComparison.OrdinalIgnoreCase)
            || themeUri.Contains("hcwhite", StringComparison.OrdinalIgnoreCase)
            || themeUri.Contains("highcontrast", StringComparison.OrdinalIgnoreCase))
        {
            theme = ApplicationTheme.HighContrast;
            return true;
        }

        if (themeUri.Contains("light", StringComparison.OrdinalIgnoreCase))
        {
            theme = ApplicationTheme.Light;
            return true;
        }

        return false;
    }
}
