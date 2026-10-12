using System;
using System.Windows;

namespace Wpf.Ui.Markdown;

public static class MarkdownStyle
{
    private const string MarkdownStyleNameKey = "MarkdownStyleName";

    private const string DocumentStyleFluent = "DocumentStyleFluent";
    private const string DocumentStyleStandard = "DocumentStyleStandard";
    private const string DocumentStyleCompact = "DocumentStyleCompact";
    private const string DocumentStyleGithubLike = "DocumentStyleGithubLike";
    private const string DocumentStyleSasabune = "DocumentStyleSasabune";
    private const string DocumentStyleSasabuneStandard = "DocumentStyleSasabuneStandard";
    private const string DocumentStyleSasabuneCompact = "DocumentStyleSasabuneCompact";

    static MarkdownStyle()
    {
        var resources = LoadDictionary();
        _fluent = (Style)resources[DocumentStyleFluent];
        _standard = (Style)resources[DocumentStyleStandard];
        _compact = (Style)resources[DocumentStyleCompact];
        _githublike = (Style)resources[DocumentStyleGithubLike];
        _sasabune = (Style)resources[DocumentStyleSasabune];
        _sasabuneStandard = (Style)resources[DocumentStyleSasabuneStandard];
        _sasabuneCompact = (Style)resources[DocumentStyleSasabuneCompact];
    }

    static ResourceDictionary LoadDictionary()
    {
        var uriText = "/Wpf.Ui.Markdown;component/Engine/Markdown.Style.xaml";
        var resourceUri = new Uri(uriText, UriKind.Relative);
        var core = (ResourceDictionary)Application.LoadComponent(resourceUri);
        return core;
    }

    /*
        Workaround for Visual Studio Xaml Designer.
        When you open MarkdownStyle from Xaml Designer,
        A null error occurs. Perhaps static constructor is not executed.         
    */
    static Style LoadXaml(string name)
    {
        return (Style)LoadDictionary()[name];
    }

    private static readonly Style _fluent;
    private static readonly Style _standard;
    private static readonly Style _compact;
    private static readonly Style _githublike;
    private static readonly Style _sasabune;
    private static readonly Style _sasabuneCompact;
    private static readonly Style _sasabuneStandard;

    /// <summary>
    /// Follows the WPF UI theme resources, so light and dark themes switch with the application.
    /// </summary>
    public static Style Fluent => _fluent is null ? LoadXaml(DocumentStyleFluent) : _fluent;
    public static Style Standard => _standard is null ? LoadXaml(DocumentStyleStandard) : _standard;
    public static Style Compact => _compact is null ? LoadXaml(DocumentStyleCompact) : _compact;
    public static Style GithubLike => _githublike is null ? LoadXaml(DocumentStyleGithubLike) : _githublike;
    public static Style Sasabune => _sasabune is null ? LoadXaml(DocumentStyleSasabune) : _sasabune;
    public static Style SasabuneStandard => _sasabuneStandard is null ? LoadXaml(DocumentStyleSasabuneStandard) : _sasabuneStandard;
    public static Style SasabuneCompact => _sasabuneCompact is null ? LoadXaml(DocumentStyleSasabuneCompact) : _sasabuneCompact;

    public static string? FindMarkdownStyleName(Style style)
    {
        for (var s = style; s is not null; s = s.BasedOn)
        {
            var rsc = s.Resources;
            if (rsc is null)
                continue;

            if (!rsc.Contains(MarkdownStyleNameKey))
                continue;

            if (rsc[MarkdownStyleNameKey] is string name)
                return name;

            return null;
        }
        return null;
    }
}
