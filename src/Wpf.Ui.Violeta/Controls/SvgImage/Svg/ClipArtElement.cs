using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg;

using Utils;

public abstract class ClipArtElement
{
    private string _id;

    protected ClipArtElement(XmlNode node)
    {
        if (node == null)
            _id = "<null>";
        else
            _id = XmlUtil.AttrValue(node, "id");
    }

    public string Id { get => _id; }
}
