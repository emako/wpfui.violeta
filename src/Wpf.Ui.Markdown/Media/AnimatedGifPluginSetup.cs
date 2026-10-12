using Wpf.Ui.Markdown.Plugins;

namespace Wpf.Ui.Markdown.Media;

public class AnimatedGifPluginSetup : IPluginSetup
{
    public void Setup(MdXamlPlugins plugins)
    {
        plugins.ElementLoader.Add(new AnimatedGifLoader());
    }
}
