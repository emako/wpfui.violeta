using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

using System.Diagnostics.CodeAnalysis;
using Utils;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public sealed class UseShape : Shape
{
    public double X { get; set; }

    public double Y { get; set; }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public string hRef { get; set; }

    [SuppressMessage("Style", "IDE0290:Use primary constructor")]
    public UseShape(SVG svg, XmlNode node) : base(svg, node)
    {
    }

    [SuppressMessage("Performance", "CA1866:Use char overload")]
    [SuppressMessage("Performance", "CA1847:Use char literal for a single character lookup")]
    [SuppressMessage("Style", "IDE0057:Use range operator")]
    protected override void Parse(SVG svg, string name, string value)
    {
        if (name.Contains(":"))
            name = name.Split(':')[1];

        if (name == SVGTags.sHref)
        {
            this.hRef = value;
            if (this.hRef.StartsWith("#"))
                this.hRef = this.hRef.Substring(1);
            return;
        }
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

        base.Parse(svg, name, value);
    }
}
