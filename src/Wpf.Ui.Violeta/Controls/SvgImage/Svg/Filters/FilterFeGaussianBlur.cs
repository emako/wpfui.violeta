using System.Xml;

#if NETFRAMEWORK

using System.Windows.Media.Effects;

#endif

namespace Wpf.Ui.Violeta.Controls.Svg.Filters;

using Utils;
using Shapes;

public class FilterFeGaussianBlur : FilterBaseFe
{
    public string In { get; set; } = null!;

    public double StdDeviationX { get; set; }

    public double StdDeviationY { get; set; }

    public FilterFeGaussianBlur(SVG svg, XmlNode node, Shape parent)
        : base(svg, node, parent)
    {
        StdDeviationX = StdDeviationY = XmlUtil.AttrValue(node, "stdDeviation", 0);
    }

#if NETFRAMEWORK

    public override BitmapEffect GetBitmapEffect()
    {
        return new BlurBitmapEffect() { Radius = StdDeviationX };
    }

#endif
}
