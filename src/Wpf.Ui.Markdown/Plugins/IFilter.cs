using System.Windows.Documents;

namespace Wpf.Ui.Markdown.Plugins;

/// <summary>
/// Filter that can transform markdown text into a FlowDocument, optionally delegating to the next filter in the chain.
/// </summary>
public interface IFilter
{
    /// <summary>
    /// Transforms markdown text into a FlowDocument. May call <paramref name="nextFilter"/> to delegate to the next filter or the default Markdown transform.
    /// </summary>
    /// <param name="markdownText">The markdown text to transform.</param>
    /// <param name="next">The next filter in the chain (or the default transform). Call <c>nextFilter.DoFilter(markdownText, ...)</c> to pass through.</param>
    /// <returns>The resulting FlowDocument.</returns>
    FlowDocument DoFilter(string markdownText, IConversion next);
}

public interface IConversion
{
    /// <summary>
    /// Transforms markdown text into a FlowDocument, without delegating to the next filter.
    /// </summary>
    /// <param name="markdownText">The markdown text to transform.</param>
    /// <returns>The resulting FlowDocument.</returns>
    FlowDocument DoConversion(string markdownText);
}
