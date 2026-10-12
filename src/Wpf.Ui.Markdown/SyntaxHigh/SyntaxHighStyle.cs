using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Wpf.Ui.Markdown.SyntaxHigh;

internal class SyntaxHighStyle
{
    private const string DocumentStyleFluent = "DocumentStyleFluent";
    private const string DocumentStyleStandard = "DocumentStyleStandard";
    private const string DocumentStyleCompact = "DocumentStyleCompact";
    private const string DocumentStyleGithubLike = "DocumentStyleGithubLike";
    private const string DocumentStyleSasabune = "DocumentStyleSasabune";
    private const string DocumentStyleSasabuneStandard = "DocumentStyleSasabuneStandard";
    private const string DocumentStyleSasabuneCompact = "DocumentStyleSasabuneCompact";

    static SyntaxHighStyle()
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
        var uriText = "/Wpf.Ui.Markdown;component/SyntaxHigh/Markdown.Style.xaml";
        var resourceUri = new Uri(uriText, UriKind.Relative);
        var core = (ResourceDictionary)Application.LoadComponent(resourceUri);
        return core;
    }

    public static Style OverwriteStyle(Style original)
    {
        if (original is null)
            throw new ArgumentNullException(nameof(original));

        var styleName = MarkdownStyle.FindMarkdownStyleName(original);
        if (styleName is null)
            return original;

        var syntaxStyle = GetStyleByName(styleName);
        if (syntaxStyle is null)
            return original;

        var newStyle = new Style(original.TargetType) { BasedOn = original };

        foreach (SetterBase setterBase in syntaxStyle.Setters)
            newStyle.Setters.Add(setterBase);

        if (syntaxStyle.Resources is not null && syntaxStyle.Resources.Count > 0)
        {
            if (newStyle.Resources is null)
                newStyle.Resources = new ResourceDictionary();
            foreach (object key in syntaxStyle.Resources.Keys)
                newStyle.Resources[key] = syntaxStyle.Resources[key];
        }

        return newStyle;
    }

    private static Style GetStyleByName(string name)
    {
        return name switch
        {
            DocumentStyleFluent => Fluent,
            DocumentStyleStandard => Standard,
            DocumentStyleCompact => Compact,
            DocumentStyleGithubLike => GithubLike,
            DocumentStyleSasabune => Sasabune,
            DocumentStyleSasabuneStandard => SasabuneStandard,
            DocumentStyleSasabuneCompact => SasabuneCompact,
            _ => Standard
        };
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

    public static Style Fluent => _fluent is null ? LoadXaml(DocumentStyleFluent) : _fluent;
    public static Style Standard => _standard is null ? LoadXaml(DocumentStyleStandard) : _standard;
    public static Style Compact => _compact is null ? LoadXaml(DocumentStyleCompact) : _compact;
    public static Style GithubLike => _githublike is null ? LoadXaml(DocumentStyleGithubLike) : _githublike;
    public static Style Sasabune => _sasabune is null ? LoadXaml(DocumentStyleSasabune) : _sasabune;
    public static Style SasabuneStandard => _sasabuneStandard is null ? LoadXaml(DocumentStyleSasabuneStandard) : _sasabuneStandard;
    public static Style SasabuneCompact => _sasabuneCompact is null ? LoadXaml(DocumentStyleSasabuneCompact) : _sasabuneCompact;
}
