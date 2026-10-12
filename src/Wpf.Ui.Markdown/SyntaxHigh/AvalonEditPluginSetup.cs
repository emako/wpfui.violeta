using Wpf.Ui.Markdown.Plugins;

namespace Wpf.Ui.Markdown.SyntaxHigh;

public class SyntaxHighPluginSetup : IPluginSetup
{
    public void Setup(MdXamlPlugins plugins)
    {
        plugins.CodeBlockLoader.Add(new AvalonCodeBlockLoader());
        plugins.StyleOverwriter.Add(new SytaxHighStyleOverwriter());
    }
}
