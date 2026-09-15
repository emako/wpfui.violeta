using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

public class TextSpan(SVG svg, XmlNode node, Shape parent)
    : TextShapeBase(svg, node, parent), ITextChild
{
}
