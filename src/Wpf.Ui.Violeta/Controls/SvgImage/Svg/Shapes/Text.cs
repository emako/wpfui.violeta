using System;
using System.Collections.Generic;
using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg;

using Utils;
using Shapes;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

/// <summary>
/// The original TextShape class.
/// </summary>
public sealed class TextShape2 : Shape
{
    [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'")]
    [SuppressMessage("Performance", "CA1866:Use char overload")]
    public TextShape2(SVG svg, XmlNode node, Shape parent)
        : base(svg, node, parent)
    {
        this.X = XmlUtil.AttrValue(node, "x", 0);
        this.Y = XmlUtil.AttrValue(node, "y", 0);
        this.Text = node.InnerText;
        this.GetTextStyle(svg);
        // check for tSpan tag
        if (node.InnerXml.IndexOf("<") >= 0)
            this.TextSpan = this.ParseTSpan(svg, node.InnerXml);
    }

    public double X { get; set; }
    public double Y { get; set; }
    public string Text { get; set; }
    public TextSpan2 TextSpan { get; private set; }

    protected override Fill DefaultFill()
    {
        return Fill.CreateDefault(Svg, "black");
    }

    protected override Stroke DefaultStroke()
    {
        return Stroke.CreateDefault(Svg, 0.1);
    }

    TextSpan2 ParseTSpan(SVG svg, string tspanText)
    {
        try
        {
            return TextSpan2.Parse(svg, tspanText, this);
        }
        catch
        {
            return null!;
        }
    }
}

public class TextSpan2 : Shape
{
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public enum eElementType
    {
        Tag,
        Text,
    }

    public override System.Windows.Media.Transform Transform => this.Parent.Transform;

    public eElementType ElementType { get; private set; }
    public List<StyleItem> Attributes { get; set; }
    public List<TextSpan2> Children { get; private set; }
    public int StartIndex { get; set; }
    public string Text { get; set; }
    public TextSpan2 End { get; set; }

    public TextSpan2(SVG svg, Shape parent, string text) : base(svg, (XmlNode)null!, parent)
    {
        this.ElementType = eElementType.Text;
        this.Text = text;
    }

    public TextSpan2(SVG svg, Shape parent, eElementType eType, List<StyleItem> attrs)
        : base(svg, attrs, parent)
    {
        this.ElementType = eType;
        this.Text = string.Empty;
        this.Children = [];
    }

    public override string ToString()
    {
        return this.Text;
    }

    [SuppressMessage("Performance", "CA1866:Use char overload")]
    [SuppressMessage("Style", "IDE0057:Use range operator")]
    static TextSpan2 NextTag(SVG svg, TextSpan2 parent, string text, ref int curPos)
    {
        int start = text.IndexOf("<", curPos);
        if (start < 0)
            return null!;
        int end = text.IndexOf(">", start + 1);
        if (end < 0)
            throw new Exception("Start '<' with no end '>'");

        end++;

        string tagtext = text.Substring(start, end - start);
        if (tagtext.IndexOf("<", 1) > 0)
            throw new Exception(string.Format("Start '<' within tag 'tag'"));

        List<StyleItem> attrs = [];
        int attrstart = tagtext.IndexOf("tspan");
        if (attrstart > 0)
        {
            attrstart += 5;
            while (attrstart < tagtext.Length - 1)
                attrs.Add(StyleItem.ReadNextAttr(tagtext, ref attrstart));
        }

        TextSpan2 tag = new(svg, parent, eElementType.Tag, attrs)
        {
            StartIndex = start,
            Text = text.Substring(start, end - start)
        };
        if (tag.Text.IndexOf("<", 1) > 0)
            throw new Exception(string.Format("Start '<' within tag 'tag'"));

        curPos = end;
        return tag;
    }

    [SuppressMessage("Maintainability", "CA1514:Avoid redundant length argument")]
    [SuppressMessage("Style", "IDE0057:Use range operator")]
    static TextSpan2 Parse(SVG svg, string text, ref int curPos, TextSpan2 parent, TextSpan2 curTag)
    {
        TextSpan2 tag = curTag;
        tag ??= NextTag(svg, parent, text, ref curPos);
        while (curPos < text.Length)
        {
            int prevPos = curPos;
            TextSpan2 next = NextTag(svg, tag, text, ref curPos);
            if (next == null && curPos < text.Length)
            {
                // remaining pure text
                string s = text.Substring(curPos, text.Length - curPos);
                tag.Children.Add(new TextSpan2(svg, tag, s));
                return tag;
            }
            if (next != null && next.StartIndex - prevPos > 0)
            {
                // pure text between tspan elements
                int diff = next.StartIndex - prevPos;
                string s = text.Substring(prevPos, diff);
                tag.Children.Add(new TextSpan2(svg, tag, s));
            }
            if (next!.Text.StartsWith("<tspan"))
            {
                // new nested element
                next = Parse(svg, text, ref curPos, tag, next);
                tag.Children.Add(next);
                continue;
            }
            if (next.Text.StartsWith("</tspan"))
            {
                // end of cur element
                tag.End = next;
                return tag;
            }
            if (next.Text.StartsWith("<textPath"))
            {
                continue;
            }
            if (next.Text.StartsWith("</textPath"))
            {
                continue;
            }
            throw new Exception(string.Format("unexpected tag '{0}'", next.Text));
        }
        return tag;
    }

    public static TextSpan2 Parse(SVG svg, string text, TextShape2 owner)
    {
        int curpos = 0;
        TextSpan2 root = new(svg, owner, eElementType.Tag, null!)
        {
            Text = "<root>",
            StartIndex = 0
        };
        return Parse(svg, text, ref curpos, null!, root);
    }

    public static void Print(TextSpan2 tag, string indent)
    {
        if (tag.ElementType == eElementType.Text)
            Console.WriteLine("{0} '{1}'", indent, tag.Text);
        indent += "   ";
        foreach (TextSpan2 c in tag.Children)
            Print(c, indent);
    }
}
