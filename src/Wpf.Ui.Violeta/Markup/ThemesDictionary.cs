using System;
using System.Windows;
using System.Windows.Markup;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Markup;

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
        ThemeManager.RegisterApplicationThemeChanged();
    }

    public ThemesDictionary()
    {
        SetSourceBasedOnSelectedTheme(ApplicationTheme.Light);
    }

    private void SetSourceBasedOnSelectedTheme(ApplicationTheme? selectedApplicationTheme)
    {
        Source = ThemeDictionaryRegistration.DefaultVioleta.GetThemeUri(
            selectedApplicationTheme ?? ApplicationTheme.Light,
            ThemeManager.GetSystemTheme());
    }
}
