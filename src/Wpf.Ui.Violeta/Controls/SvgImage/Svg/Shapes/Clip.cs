using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

public sealed class Clip(SVG svg, XmlNode node, Shape parent)
    : Group(svg, node, parent)
{
    private Geometry clpGeo = null!;

    public Geometry ClipGeometry
    {
        get
        {
            if (clpGeo != null) return clpGeo;

            var retVal = getGeoForShape(Elements[0]);

            if (Elements.Count > 1)
                foreach (var element in Elements)
                {
                    retVal = new CombinedGeometry(retVal, getGeoForShape(element));
                }

            clpGeo = retVal;
            return clpGeo;
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    private Geometry getGeoForShape(Shape shp)
    {
        if (shp is RectangleShape)
        {
            var r = shp as RectangleShape;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var g = new RectangleGeometry(new Rect(r.RX, r.RY, r.Width, r.Height));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            r.geometryElement = g;
            return g;
        }
        if (shp is CircleShape)
        {
            var c = shp as CircleShape;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var g = new EllipseGeometry(new Point(c.CX, c.CY), c.R, c.R);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            c.geometryElement = g;
            return g;
        }
        return null!;
    }
}
