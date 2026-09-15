using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Animation;

using Shapes;

public sealed class AnimateMotion(SVG svg, XmlNode node, Shape parent)
    : AnimationBase(svg, node, parent)
{
}
