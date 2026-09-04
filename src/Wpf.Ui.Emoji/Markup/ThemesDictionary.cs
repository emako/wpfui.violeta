using System.Windows;
using System.Windows.Markup;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Emoji.Markup;

/// <summary>
/// Theme resource dictionary for <c>Wpf.Ui.Emoji</c>.
/// Swapping is handled by <see cref="ThemeManager"/> via
/// <see cref="ThemeDictionaryRegistration.DefaultEmoji"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;Application
///     xmlns:emoji="http://schemas.lepo.co/wpfui/2022/xaml/emoji"&gt;
///     &lt;Application.Resources&gt;
///         &lt;ResourceDictionary&gt;
///             &lt;ResourceDictionary.MergedDictionaries&gt;
///                 &lt;emoji:ThemesDictionary Theme="Dark" /&gt;
///             &lt;/ResourceDictionary.MergedDictionaries&gt;
///         &lt;/ResourceDictionary&gt;
///     &lt;/Application.Resources&gt;
/// &lt;/Application&gt;
/// </code>
/// </example>
[Localizability(LocalizationCategory.Ignore)]
[Ambient]
[UsableDuringInitialization(true)]
public class ThemesDictionary : ResourceDictionary
{
    public ApplicationTheme Theme
    {
        set => SetSourceBasedOnSelectedTheme(value);
    }

    public ThemesDictionary()
    {
        SetSourceBasedOnSelectedTheme(ApplicationTheme.Light);
    }

    private void SetSourceBasedOnSelectedTheme(ApplicationTheme? selectedApplicationTheme)
    {
        Source = ThemeDictionaryRegistrations.DefaultEmoji.GetThemeUri(
            selectedApplicationTheme ?? ApplicationTheme.Light,
            ThemeManager.GetSystemTheme());
    }
}
