using System;
using System.Windows.Media;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using System.Diagnostics.CodeAnalysis;

namespace Wpf.Ui.Violeta.Controls.Svg.Utils;

internal static class DrawingGroupSerializer
{
    [SuppressMessage("Style", "IDE0063:Use simple 'using' statement")]
    public static string SerializeToXaml(DrawingGroup drawingGroup)
    {
        _ = drawingGroup ?? throw new ArgumentNullException(nameof(drawingGroup));

        // Freezing can help ensure serialization works without exceptions
        if (drawingGroup.CanFreeze && !drawingGroup.IsFrozen)
        {
            drawingGroup.Freeze();
        }

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = true,
        };

        using (var stringWriter = new StringWriter())
        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
        {
            XamlWriter.Save(drawingGroup, xmlWriter);
            return stringWriter.ToString();
        }
    }
}
