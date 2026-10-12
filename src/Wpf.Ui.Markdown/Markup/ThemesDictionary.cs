using System;
using System.Windows;
using System.Windows.Markup;
using Wpf.Ui.Appearance;
using Wpf.Ui.Markdown.Appearance;

namespace Wpf.Ui.Markdown.Markup;

/// <summary>
/// Provides the light or dark Markdown brushes, and keeps them in sync with <see cref="ApplicationThemeManager"/>.
/// </summary>
[Localizability(LocalizationCategory.Ignore)]
[Ambient]
[UsableDuringInitialization(true)]
public class ThemesDictionary : ResourceDictionary
{
    public ApplicationTheme Theme
    {
        set => SetSourceBasedOnSelectedTheme(value);
    }

    static ThemesDictionary()
    {
        ThemeManager.RegistApplicationThemeChanged();
    }

    public ThemesDictionary()
    {
        SetSourceBasedOnSelectedTheme(ApplicationTheme.Light);
    }

    private void SetSourceBasedOnSelectedTheme(ApplicationTheme? selectedApplicationTheme)
    {
        var themeName = selectedApplicationTheme switch
        {
            ApplicationTheme.Dark => "Dark",
            _ => "Light",
        };

        Source = new Uri($"{ThemeManager.ThemesDictionaryPath}{themeName}.xaml", UriKind.Absolute);
    }
}
