using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

using System.Diagnostics.CodeAnalysis;
using Utils;

public sealed class RectangleShape(SVG svg, XmlNode node) : Shape(svg, node)
{
    protected override Fill DefaultFill()
    {
        return Fill.CreateDefault(Svg, "black");
    }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double RX { get; set; }

    public double RY { get; set; }

    [SuppressMessage("Performance", "CA1847:Use char literal for a single character lookup")]
    protected override void Parse(SVG svg, string name, string value)
    {
        if (name.Contains(":"))
            name = name.Split(':')[1];

        if (name == "x")
        {
            this.X = XmlUtil.GetDoubleValue(value, svg.Size.Width);
            return;
        }
        if (name == "y")
        {
            this.Y = XmlUtil.GetDoubleValue(value, svg.Size.Height);
            return;
        }
        if (name == "width")
        {
            this.Width = XmlUtil.GetDoubleValue(value, svg.Size.Width);
            return;
        }
        if (name == "height")
        {
            this.Height = XmlUtil.GetDoubleValue(value, svg.Size.Height);
            return;
        }
        if (name == "rx")
        {
            this.RX = XmlUtil.GetDoubleValue(value, svg.Size.Width);
            return;
        }
        if (name == "ry")
        {
            this.RY = XmlUtil.GetDoubleValue(value, svg.Size.Height);
            return;
        }

        base.Parse(svg, name, value);
    }
}
