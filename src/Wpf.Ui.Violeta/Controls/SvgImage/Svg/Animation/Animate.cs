using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Animation;

using Utils;
using Shapes;
using System.Diagnostics.CodeAnalysis;

public sealed class Animate : AnimationBase
{
    [SuppressMessage("Performance", "CA1866:Use char overload")]
    [SuppressMessage("Style", "IDE0057:Use range operator")]
    public Animate(SVG svg, XmlNode node, Shape parent)
        : base(svg, node, parent)
    {
        this.From = XmlUtil.AttrValue(node, "from", null!);
        this.To = XmlUtil.AttrValue(node, "to", null!);
        this.AttributeName = XmlUtil.AttrValue(node, "attributeName", null!);
        this.RepeatType = XmlUtil.AttrValue(node, "repeatCount", "indefinite");
        this.Values = XmlUtil.AttrValue(node, "values", null!);

        this.hRef = XmlUtil.AttrValue(node, "xlink:href", string.Empty);
        if (hRef.StartsWith("#")) hRef = hRef.Substring(1);
    }

    public string AttributeName { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public string RepeatType { get; set; }

    public string Values { get; set; }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public string hRef { get; set; }
}
