using System.Collections.Generic;
using System.Linq;
using Wpf.Ui.Markdown.FrontMatter;
using Wpf.Ui.Markdown.Html;
using Wpf.Ui.Markdown.Media;
using Wpf.Ui.Markdown.Plugins;
using Wpf.Ui.Markdown.SyntaxHigh;

namespace Wpf.Ui.Markdown;

internal static class DefaultPluginSetups
{
    public static MdXamlPlugins Apply(MdXamlPlugins? plugins)
    {
        var target = plugins ?? new MdXamlPlugins();

        AddIfAbsent<FrontMatterPluginSetup>(target.Setups);
        AddIfAbsent<HtmlPluginSetup>(target.Setups);
        AddIfAbsent<SvgPluginSetup>(target.Setups);
        AddIfAbsent<AnimatedGifPluginSetup>(target.Setups);
        AddIfAbsent<SyntaxHighPluginSetup>(target.Setups);

        return target;
    }

    private static void AddIfAbsent<T>(IList<IPluginSetup> setups) where T : IPluginSetup, new()
    {
        if (!setups.Any(p => p is T))
        {
            setups.Add(new T());
        }
    }
}
