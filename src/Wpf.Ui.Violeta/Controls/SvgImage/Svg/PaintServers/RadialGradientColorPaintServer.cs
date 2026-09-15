using System.Xml;
using System.Diagnostics;

using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Svg.PaintServers;

using System.Diagnostics.CodeAnalysis;
using Utils;

public sealed class RadialGradientColorPaintServer : GradientColorPaintServer
{
    public RadialGradientColorPaintServer(PaintServerManager owner, XmlNode node)
        : base(owner, node)
    {
        Debug.Assert(node.Name == SVGTags.sRadialGradient);
        this.CX = XmlUtil.AttrValue(node, "cx", 0.5);
        this.CY = XmlUtil.AttrValue(node, "cy", 0.5);
        this.FX = XmlUtil.AttrValue(node, "fx", this.CX);
        this.FY = XmlUtil.AttrValue(node, "fy", this.CY);
        this.R = XmlUtil.AttrValue(node, "r", 0.5);
        this.Normalize();
    }

    public RadialGradientColorPaintServer(PaintServerManager owner, Brush newBrush)
        : base(owner)
    {
        Brush = newBrush;
    }

    public double CX { get; private set; }

    public double CY { get; private set; }

    public double FX { get; private set; }

    public double FY { get; private set; }

    public double R { get; private set; }

    public override Brush GetBrush(double opacity, SVG svg, SVGRender svgRender, Rect bounds)
    {
        if (Brush != null) return Brush;

        RadialGradientBrush b = new();
        foreach (GradientStop stop in Stops) b.GradientStops.Add(stop);

        b.GradientOrigin = new Point(0.5, 0.5);
        b.Center = new Point(0.5, 0.5);
        b.RadiusX = 0.5;
        b.RadiusY = 0.5;

        if (GradientUnits == SVGTags.sUserSpaceOnUse)
        {
            b.Center = new Point(CX, CY);
            b.GradientOrigin = new Point(FX, FY);
            b.RadiusX = R;
            b.RadiusY = R;
            b.MappingMode = BrushMappingMode.Absolute;
        }
        else
        {
            double scale = 1d / 100d;
            if (!double.IsNaN(CX) && !double.IsNaN(CY))
            {
                //b.GradientOrigin = new Point(this.CX*scale, this.CY*scale);
                b.Center = new Point(CX /* *scale */, this.CY /* *scale */);
            }
            if (!double.IsNaN(FX) && !double.IsNaN(FY))
            {
                b.GradientOrigin = new Point(FX * scale, FY * scale);
            }
            if (!double.IsNaN(R))
            {
                b.RadiusX = R /* *scale*/;
                b.RadiusY = R /* *scale*/;
            }
            b.MappingMode = BrushMappingMode.RelativeToBoundingBox;
        }
        if (Transform != null) b.Transform = Transform;

        Brush = b;

        return b;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private void Normalize()
    {
    }
}
