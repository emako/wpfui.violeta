using System;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.Violeta.Appearance;

/// <summary>
/// Describes a theme resource dictionary that <see cref="ThemeManager"/> can locate and swap.
/// </summary>
public sealed class ThemeDictionaryRegistration
{
    /// <summary>
    /// Built-in registration for WPF UI theme dictionaries.
    /// </summary>
    public static ThemeDictionaryRegistration DefaultWpfUi { get; } = FromPackUri(
        searchNamespace: "wpf.ui;",
        themesBaseUri: new Uri("pack://application:,,,/Wpf.Ui;component/Resources/Theme/", UriKind.Absolute),
        resolveThemeFileName: ThemeDictionaryFileNameResolvers.WpfUi,
        name: "Wpf.Ui");

    /// <summary>
    /// Built-in registration for Violeta theme dictionaries.
    /// </summary>
    public static ThemeDictionaryRegistration DefaultVioleta { get; } = FromPackUri(
        searchNamespace: "wpf.ui.violeta;",
        themesBaseUri: new Uri("pack://application:,,,/Wpf.Ui.Violeta;component/Resources/Theme/", UriKind.Absolute),
        resolveThemeFileName: ThemeDictionaryFileNameResolvers.LightDarkWithHighContrastFallback,
        name: "Wpf.Ui.Violeta");

    /// <summary>
    /// Substring used to locate the dictionary in application merged dictionaries.
    /// Typically the lowercase assembly URI segment, e.g. <c>wpf.ui.violeta;</c>.
    /// </summary>
    public string SearchNamespace { get; init; } = string.Empty;

    /// <summary>
    /// Substring used to match the dictionary role. Defaults to <c>theme</c>.
    /// </summary>
    public string DictionaryLookup { get; init; } = "theme";

    /// <summary>
    /// Optional display name for diagnostics.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Base pack URI for theme XAML files when <see cref="ResolveThemeUri"/> is not set.
    /// </summary>
    public Uri? ThemesBaseUri { get; init; }

    /// <summary>
    /// Resolves the XAML file name without extension when <see cref="ResolveThemeUri"/> is not set.
    /// </summary>
    public Func<ApplicationTheme, SystemTheme, string>? ResolveThemeFileName { get; init; }

    /// <summary>
    /// Optional full URI resolver. When set, it takes precedence over
    /// <see cref="ThemesBaseUri"/> and <see cref="ResolveThemeFileName"/>.
    /// </summary>
    public Func<ApplicationTheme, SystemTheme, Uri>? ResolveThemeUri { get; init; }

    /// <summary>
    /// Creates a registration that swaps pack URIs under <paramref name="themesBaseUri"/>.
    /// </summary>
    public static ThemeDictionaryRegistration FromPackUri(
        string searchNamespace,
        Uri themesBaseUri,
        Func<ApplicationTheme, SystemTheme, string>? resolveThemeFileName = null,
        string dictionaryLookup = "theme",
        string? name = null)
    {
        if (string.IsNullOrWhiteSpace(searchNamespace))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchNamespace));
        }

        if (themesBaseUri is null)
        {
            throw new ArgumentNullException(nameof(themesBaseUri));
        }

        return new ThemeDictionaryRegistration
        {
            SearchNamespace = searchNamespace,
            ThemesBaseUri = themesBaseUri,
            ResolveThemeFileName = resolveThemeFileName ?? ThemeDictionaryFileNameResolvers.LightDark,
            DictionaryLookup = dictionaryLookup,
            Name = name,
        };
    }

    /// <summary>
    /// Creates a registration from explicit light and dark theme URIs.
    /// High contrast falls back to dark for <see cref="SystemTheme.HCBlack"/> and light otherwise.
    /// </summary>
    public static ThemeDictionaryRegistration FromThemes(
        string searchNamespace,
        Uri lightThemeUri,
        Uri darkThemeUri,
        Uri? highContrastThemeUri = null,
        string dictionaryLookup = "theme",
        string? name = null)
    {
        if (string.IsNullOrWhiteSpace(searchNamespace))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchNamespace));
        }

        if (lightThemeUri is null)
        {
            throw new ArgumentNullException(nameof(lightThemeUri));
        }

        if (darkThemeUri is null)
        {
            throw new ArgumentNullException(nameof(darkThemeUri));
        }

        return new ThemeDictionaryRegistration
        {
            SearchNamespace = searchNamespace,
            DictionaryLookup = dictionaryLookup,
            Name = name,
            ResolveThemeUri = (theme, systemTheme) => theme switch
            {
                ApplicationTheme.Dark => darkThemeUri,
                ApplicationTheme.HighContrast => highContrastThemeUri
                    ?? (systemTheme == SystemTheme.HCBlack ? darkThemeUri : lightThemeUri),
                _ => lightThemeUri,
            },
        };
    }

    internal Uri GetThemeUri(ApplicationTheme theme, SystemTheme systemTheme)
    {
        if (ResolveThemeUri is not null)
        {
            return ResolveThemeUri(theme, systemTheme);
        }

        if (ThemesBaseUri is null || ResolveThemeFileName is null)
        {
            throw new InvalidOperationException(
                $"Theme registration '{Name ?? SearchNamespace}' must define either ResolveThemeUri or both ThemesBaseUri and ResolveThemeFileName.");
        }

        string fileName = ResolveThemeFileName(theme, systemTheme);
        string basePath = ThemesBaseUri.ToString();

        if (!basePath.EndsWith("/", StringComparison.Ordinal))
        {
            basePath += '/';
        }

        return new Uri(basePath + fileName + ".xaml", UriKind.Absolute);
    }
}
