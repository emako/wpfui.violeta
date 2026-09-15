using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Svg.PaintServers;

public sealed class CurrentColorPaintServer(PaintServerManager owner) : PaintServer(owner)
{
    public override Brush GetBrush(double opacity, SVG svg, SVGRender svgRender, Rect bounds)
    {
        return Brushes.Black;
    }
}
