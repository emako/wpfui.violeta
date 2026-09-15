using System.Xml;

#if NETFRAMEWORK

using System.Windows.Media.Effects;

#endif

namespace Wpf.Ui.Violeta.Controls.Svg.Filters;

using Shapes;

public abstract class FilterBaseFe(SVG svg, XmlNode node, Shape parent)
    : Shape(svg, node, parent)
{
#if NETFRAMEWORK

    public abstract BitmapEffect GetBitmapEffect();

#endif
}
