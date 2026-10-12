using System.Windows.Controls;
using System.Windows.Documents;

namespace Wpf.Ui.Markdown.Plugins;

/// <summary>
/// Arranges WPF Control based on the FlowDocument (e.g. add elements around the viewer).
/// </summary>
public interface IViewerArranger
{
    /// <summary>
    /// Arranges the viewer using the given document.
    /// </summary>
    /// <param name="viewer">The control that displays the document (e.g. MarkdownScrollViewer).</param>
    /// <param name="document">The FlowDocument currently displayed.</param>
    void Arrange(Control viewer, FlowDocument document);

    /// <summary>
    /// Resets the previous arrangement so that a new Arrange call can be applied cleanly.
    /// </summary>
    /// <param name="viewer">The same control passed to Arrange.</param>
    void ResetArrange(Control viewer);
}
