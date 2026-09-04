using System;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.Violeta.Appearance;

public static class ThemeDictionaryRegistrations
{
    /// <summary>
    /// Built-in registration for WPF UI theme dictionaries.
    /// </summary>
    public static ThemeDictionaryRegistration DefaultWpfUi { get; } = ThemeDictionaryRegistration.FromPackUri(
        searchNamespace: "wpf.ui;",
        themesBaseUri: new Uri("pack://application:,,,/Wpf.Ui;component/Resources/Theme/", UriKind.Absolute),
        name: "Wpf.Ui");

    /// <summary>
    /// Built-in registration for Violeta theme dictionaries.
    /// </summary>
    public static ThemeDictionaryRegistration DefaultVioleta { get; } = ThemeDictionaryRegistration.FromPackUri(
        searchNamespace: "wpf.ui.violeta;",
        themesBaseUri: new Uri("pack://application:,,,/Wpf.Ui.Violeta;component/Resources/Theme/", UriKind.Absolute),
        name: "Wpf.Ui.Violeta");

    /// <summary>
    /// Built-in registration for Emoji theme dictionaries.
    /// </summary>
    public static ThemeDictionaryRegistration DefaultEmoji { get; } = ThemeDictionaryRegistration.FromPackUri(
        searchNamespace: "wpf.ui.emoji;",
        themesBaseUri: new Uri("pack://application:,,,/Wpf.Ui.Emoji;component/Resources/Theme/", UriKind.Absolute),
        name: "Wpf.Ui.Emoji");
}

/// <summary>
/// Describes a theme resource dictionary that <see cref="ThemeManager"/> can locate and swap.
/// </summary>
public sealed class ThemeDictionaryRegistration
{
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
    /// Base pack URI for theme XAML files.
    /// </summary>
    public Uri? ThemesBaseUri { get; init; }

    /// <summary>
    /// Creates a registration that swaps pack URIs under <paramref name="themesBaseUri"/>.
    /// </summary>
    public static ThemeDictionaryRegistration FromPackUri(
        string searchNamespace,
        Uri themesBaseUri,
        string dictionaryLookup = "theme",
        string? name = null)
    {
        if (string.IsNullOrWhiteSpace(searchNamespace))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchNamespace));
        }

        _ = themesBaseUri ?? throw new ArgumentNullException(nameof(themesBaseUri));

        return new ThemeDictionaryRegistration
        {
            SearchNamespace = searchNamespace,
            ThemesBaseUri = themesBaseUri,
            DictionaryLookup = dictionaryLookup,
            Name = name,
        };
    }

    /// <summary>
    /// Resolves the pack URI for the given application and system theme.
    /// </summary>
    public Uri GetThemeUri(ApplicationTheme theme, SystemTheme systemTheme)
    {
        if (ThemesBaseUri is null)
        {
            throw new InvalidOperationException(
                $"Theme registration '{Name ?? SearchNamespace}' must define ThemesBaseUri.");
        }

        string fileName = theme switch
        {
            ApplicationTheme.Dark => "Dark",
            ApplicationTheme.HighContrast => systemTheme == SystemTheme.HCBlack ? "Dark" : "Light",
            _ => "Light",
        };

        string basePath = ThemesBaseUri.ToString();

#pragma warning disable CA1865 // Use char overload
        if (!basePath.EndsWith("/", StringComparison.Ordinal))
        {
            basePath += '/';
        }
#pragma warning restore CA1865 // Use char overload

        return new Uri(basePath + fileName + ".xaml", UriKind.Absolute);
    }
}
