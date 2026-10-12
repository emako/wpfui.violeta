namespace Wpf.Ui.Markdown.Controls;

/// <summary>
/// <see cref="MarkdownScrollViewer"/> that opens hyperlinks like a browser: http(s) links open directly,
/// other targets ask for confirmation and <c>#anchor</c> links scroll inside the document.
/// </summary>
public class MarkdownViewer : MarkdownScrollViewer
{
    public MarkdownViewer()
    {
        ClickAction = ClickAction.SafetyOpenBrowser;
    }
}
