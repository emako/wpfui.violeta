using Wpf.Ui.Markdown.Plugins;

namespace Wpf.Ui.Markdown.Media;

public class SvgPluginSetup : IPluginSetup
{
    public void Setup(MdXamlPlugins plugins)
    {
        plugins.ElementLoader.Add(new SvgElementLoader());
    }
}
