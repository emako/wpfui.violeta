using Wpf.Ui.Markdown.Plugins;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Wpf.Ui.Markdown.FrontMatter;

internal class FrontMatterViewerArranger : IViewerArranger
{
    public void Arrange(Control viewer, FlowDocument document)
    {
        if (FrontMatter.GetFrontMatterYaml(document) is { } yaml) 
            FrontMatter.SetFrontMatterYaml(viewer, yaml);
        else
            ResetArrange(viewer);
    }

    public void ResetArrange(Control viewer)
    {
        FrontMatter.ClearFrontMatterYaml(viewer);
    }
}