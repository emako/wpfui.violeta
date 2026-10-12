using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using Wpf.Ui.Markdown.Plugins;
using YamlDotNet.RepresentationModel;

namespace Wpf.Ui.Markdown.FrontMatter;

/// <summary>
/// Filter that detects and parses YAML FrontMatter at the beginning of the document,
/// strips it from the text, and attaches the parsed YAML to the resulting <see cref="FlowDocument"/>.
/// Register as an <see cref="IFilter"/> (e.g. first in Filters) so it runs at the start of the transform chain.
/// </summary>
internal class FrontMatterFilter : IFilter
{
    private static readonly string s_firstLineDelimiter = "---\n";
    private static readonly string s_closingDelimiter = "\n---\n";

    public FlowDocument DoFilter(string markdownText, IConversion next)
    {
        if (next is null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        if (string.IsNullOrEmpty(markdownText)
         || markdownText.Length < s_firstLineDelimiter.Length + s_closingDelimiter.Length)
            return next.DoConversion(markdownText);

        if (!markdownText.StartsWith(s_firstLineDelimiter))
            return next.DoConversion(markdownText);

        var closingIndex = markdownText.IndexOf(s_closingDelimiter, s_firstLineDelimiter.Length);
        if (closingIndex < 0)
            return next.DoConversion(markdownText);

        var yamlText = markdownText.Substring(s_firstLineDelimiter.Length, closingIndex - s_firstLineDelimiter.Length);
        YamlNode? yamlNode = null;
        try
        {
            yamlNode = ParseYaml(yamlText);
            if (yamlNode is null)
                return next.DoConversion(markdownText);
        }
        catch
        {
            return next.DoConversion(markdownText);
        }

        var closingDelimiterEnd = closingIndex + s_closingDelimiter.Length;
        var bodyText = markdownText.Substring(closingDelimiterEnd);
        var document = next.DoConversion(bodyText);
        FrontMatter.SetFrontMatterYaml(document, yamlNode);
        return document;
    }

    private static YamlNode? ParseYaml(string yamlText)
    {
        if (string.IsNullOrWhiteSpace(yamlText))
        {
            return null;
        }

        var stream = new YamlStream();
        using var reader = new StringReader(yamlText);
        stream.Load(reader);

        if (stream.Documents.Count == 0)
        {
            return null;
        }

        return stream.Documents[0].RootNode;
    }
}