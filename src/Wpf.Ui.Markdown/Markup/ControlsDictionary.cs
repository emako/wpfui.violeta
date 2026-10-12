using System;
using System.Windows;
using System.Windows.Markup;

namespace Wpf.Ui.Markdown.Markup;

/// <summary>
/// Provides the styles of <see cref="MarkdownScrollViewer"/> and the shared Markdown variables.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;Application
///     xmlns:md="http://schemas.lepo.co/wpfui/2022/xaml/markdown"&gt;
///     &lt;Application.Resources&gt;
///         &lt;ResourceDictionary&gt;
///             &lt;ResourceDictionary.MergedDictionaries&gt;
///                 &lt;md:ThemesDictionary Theme="Dark" /&gt;
///                 &lt;md:ControlsDictionary /&gt;
///             &lt;/ResourceDictionary.MergedDictionaries&gt;
///         &lt;/ResourceDictionary&gt;
///     &lt;/Application.Resources&gt;
/// &lt;/Application&gt;
/// </code>
/// </example>
[Localizability(LocalizationCategory.Ignore)]
[Ambient]
[UsableDuringInitialization(true)]
public class ControlsDictionary : ResourceDictionary
{
    private const string DictionaryUri = "pack://application:,,,/Wpf.Ui.Markdown;component/Resources/Wpf.Ui.xaml";

    public ControlsDictionary()
    {
        Source = new Uri(DictionaryUri, UriKind.Absolute);
    }
}
