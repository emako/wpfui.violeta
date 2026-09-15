using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Utils;

internal static class XmlUtil
{
    [SuppressMessage("Style", "IDE0057:Use range operator")]
    static bool GetValueRespectingUnits(string inputstring, out double value, double percentageMaximum)
    {
        value = 0;
        var units = string.Empty;
        int index = inputstring.LastIndexOfAny(['.', '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);
        if (index >= 0)
        {
            string svalue = inputstring.Substring(0, index + 1);
            if (index + 1 < inputstring.Length)
                units = inputstring.Substring(index + 1);
            try
            {
                value = XmlConvert.ToDouble(svalue);

                switch (units) // from http://www.selfsvg.info/?section=3.4
                {
                    case "pt": value *= 1.25; break;
                    case "mm": value *= 3.54; break;
                    case "pc": value *= 15; break;
                    case "cm": value *= 35.43; break;
                    case "in": value *= 90; break;
                    case "%": value = value * percentageMaximum / 100; break;
                }

                return true;
            }
            catch (FormatException)
            { }
        }
        return false;
    }

    public static double GetDoubleValue(string value, double percentageMaximum = 1)
    {
        if (GetValueRespectingUnits(value, out double result, percentageMaximum))
        {
            return result;
        }
        return 0;
    }

    public static double AttrValue(StyleItem attr)
    {
        GetValueRespectingUnits(attr.Value, out double result, 1);
        return result;
    }

    public static double AttrValue(XmlNode node, string id, double defaultvalue, double percentageMaximum = 1)
    {
        XmlAttribute attr = node.Attributes![id]!;
        if (attr == null)
            return defaultvalue;

        if (attr != null && GetValueRespectingUnits(attr.Value, out double result, percentageMaximum))
        {
            return result;
        }
        return defaultvalue;
    }

    public static string AttrValue(XmlNode node, string id, string defaultvalue)
    {
        if (node.Attributes == null)
            return defaultvalue;
        XmlAttribute attr = node.Attributes[id]!;
        if (attr != null)
            return attr.Value;
        return defaultvalue;
    }

    public static string AttrValue(XmlNode node, string id)
    {
        return AttrValue(node, id, string.Empty);
    }

    public static double ParseDouble(SVG svg, string svalue)
    {
        _ = svg;
        if (GetValueRespectingUnits(svalue, out double value, 1))
            return value;
        return 0.1;
    }

    public static XmlAttribute CreateAttr(XmlNode owner, string name, string value)
    {
        return new TempXmlAttribute(owner, name, value);
    }

    internal sealed class TempXmlAttribute : XmlAttribute
    {
        public TempXmlAttribute(XmlNode owner, string name, string value)
            : base(string.Empty, name, string.Empty, owner.OwnerDocument!)
        {
            this.Value = value;
        }
    }
}
