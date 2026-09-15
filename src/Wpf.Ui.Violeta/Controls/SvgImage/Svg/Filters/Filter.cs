using System.Xml;

#if NETFRAMEWORK

using System.Windows.Media.Effects;
using System.Diagnostics.CodeAnalysis;

#endif

namespace Wpf.Ui.Violeta.Controls.Svg.Filters;

using Shapes;

public sealed class Filter(SVG svg, XmlNode node, Shape parent)
    : Group(svg, node, parent)
{
#if NETFRAMEWORK

    [SuppressMessage("Style", "IDE0220:Add explicit cast")]
    public BitmapEffect GetBitmapEffect()
    {
        var beg = new BitmapEffectGroup();
        foreach (FilterBaseFe element in Elements)
        {
            beg.Children.Add(element.GetBitmapEffect());
        }

        return beg;
    }

#endif
}
