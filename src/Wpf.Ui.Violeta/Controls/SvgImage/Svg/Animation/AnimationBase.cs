using System;
using System.Xml;

namespace Wpf.Ui.Violeta.Controls.Svg.Animation;

using Shapes;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Utils;

public abstract class AnimationBase : Shape
{
    // https://www.mediaevent.de/tutorial/svg-animate-attribute.html
    public TimeSpan Duration { get; set; }

    [SuppressMessage("Performance", "CA1846:Prefer 'AsSpan' over 'Substring'")]
    [SuppressMessage("Performance", "CA1866:Use char overload")]
    [SuppressMessage("Style", "IDE0057:Use range operator")]
    protected AnimationBase(SVG svg, XmlNode node, Shape parent)
        : base(svg, node, parent)
    {
        var d = XmlUtil.AttrValue(node, "dur", "");
        if (d.EndsWith("ms"))
            Duration = TimeSpan.FromMilliseconds(double.Parse(d.Substring(0, d.Length - 2), NumberStyles.Number, CultureInfo.InvariantCulture));
        else if (d.EndsWith("s"))
            Duration = TimeSpan.FromSeconds(double.Parse(d.Substring(0, d.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture));
        else
            Duration = TimeSpan.FromSeconds(double.Parse(d, NumberStyles.Number, CultureInfo.InvariantCulture));
    }
}
