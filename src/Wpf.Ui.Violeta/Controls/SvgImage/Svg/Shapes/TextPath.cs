using System;
using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

/// <summary>
/// A placeholder class for TextPath.
/// </summary>
internal class TextPath : TextShapeBase, ITextChild
{
    protected TextPath(SVG svg, XmlNode node, Shape parent) : base(svg, node, parent)
    {
        throw new NotImplementedException("TextPath is not yet implemented.");
    }
}
