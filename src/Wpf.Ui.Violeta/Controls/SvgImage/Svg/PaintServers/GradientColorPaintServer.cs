using System.Collections.Generic;
using System.Windows.Media;
using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.PaintServers;

using System.Diagnostics.CodeAnalysis;
using Utils;

public abstract class GradientColorPaintServer : PaintServer
{
    // http://www.w3.org/TR/SVG11/pservers.html#LinearGradients
    [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
    private List<GradientStop> m_stops = [];

    protected GradientColorPaintServer(PaintServerManager owner)
        : base(owner)
    {
    }

    protected GradientColorPaintServer(PaintServerManager owner, XmlNode node)
        : base(owner)
    {
        this.GradientUnits = XmlUtil.AttrValue(node, "gradientUnits", string.Empty);
        string transform = XmlUtil.AttrValue(node, "gradientTransform", string.Empty);
        if (transform.Length > 0)
        {
            this.Transform = ShapeUtil.ParseTransform(transform.ToLower());
        }

        if (node.ChildNodes.Count == 0 && XmlUtil.AttrValue(node, "xlink:href", string.Empty).Length > 0)
        {
            string refid = XmlUtil.AttrValue(node, "xlink:href", string.Empty);
            string paintServerKey = owner.Parse(refid.Substring(1));
            if (owner.GetServer(paintServerKey) is not GradientColorPaintServer refcol) return;
            this.m_stops = [.. refcol.m_stops];
        }

        foreach (XmlNode childnode in node.ChildNodes)
        {
            if (childnode.Name == "stop")
            {
                List<StyleItem> styleattr = [];
                string fullstyle = XmlUtil.AttrValue(childnode, SVGTags.sStyle, string.Empty);
                if (fullstyle.Length > 0)
                {
                    foreach (StyleItem styleitem in StyleParser.SplitStyle(null!, fullstyle))
                        styleattr.Add(new StyleItem(styleitem.Name, styleitem.Value));
                }
                foreach (var attr1 in styleattr)
                    childnode.Attributes!.Append(XmlUtil.CreateAttr(childnode, attr1.Name, attr1.Value));

                double offset = XmlUtil.AttrValue(childnode, "offset", (double)0);
                string s = XmlUtil.AttrValue(childnode, "stop-color", "#0");

                double stopopacity = XmlUtil.AttrValue(childnode, "stop-opacity", (double)1);

                Color color;
                if (s.StartsWith("#")) color = PaintServerManager.ParseHexColor(s);
                else color = PaintServerManager.KnownColor(s);

                if (stopopacity != 1)
                    color = Color.FromArgb((byte)(stopopacity * 255), color.R, color.G, color.B);

                if (offset > 1) offset = offset / 100;
                this.m_stops.Add(new GradientStop(color, offset));
            }
        }
    }

    public IList<GradientStop> Stops => m_stops.AsReadOnly();

    public Transform Transform { get; protected set; } = null!;

    public string GradientUnits { get; private set; } = null!;
}
