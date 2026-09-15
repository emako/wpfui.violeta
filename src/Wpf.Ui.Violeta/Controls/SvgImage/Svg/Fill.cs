using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Svg;

using PaintServers;
using Shapes;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public sealed class Fill(SVG svg)
{
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public enum eFillRule
    {
        nonzero,
        evenodd
    }

    [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
    private SVG _svg = svg;

    private bool _isDefault = false;

    private eFillRule _fillRule = eFillRule.nonzero;

    private string _paintServerKey;

    private double _opacity = 100;

    public static Fill CreateDefault(SVG svg, string fillColor)
    {
        var fill = new Fill(svg)
        {
            PaintServerKey = svg.PaintServers.Parse(fillColor),
            _isDefault = true
        };

        return fill;
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public SVG get => _svg;

    public bool IsDefault
    {
        get => _isDefault;
        set => _isDefault = value;
    }

    public eFillRule FillRule
    {
        get => _fillRule;
        set
        {
            Debug.Assert(_isDefault == false);
            _fillRule = value;
        }
    }

    public string PaintServerKey
    {
        get => _paintServerKey;
        set
        {
            Debug.Assert(_isDefault == false);
            _paintServerKey = value;
        }
    }

    public double Opacity
    {
        get => _opacity;
        set
        {
            Debug.Assert(_isDefault == false);
            _opacity = value;
        }
    }

    public bool IsEmpty(SVG svg)
    {
        if (svg == null) return true;

        if (!svg.PaintServers.ContainsServer(this.PaintServerKey))
        {
            return true;
        }
        return svg.PaintServers.GetServer(this.PaintServerKey) == null;
    }

    public Brush FillBrush(SVG svg, SVGRender svgRender, Shape shape, double elementOpacity, Rect bounds)
    {
        var paintServer = svg.PaintServers.GetServer(this.PaintServerKey);
        if (paintServer != null)
        {
            if (paintServer is CurrentColorPaintServer)
            {
                var shapePaintServer = svg.PaintServers.GetServer(shape.PaintServerKey);
                if (shapePaintServer != null)
                {
                    return shapePaintServer.GetBrush(this.Opacity * elementOpacity, svg, svgRender, bounds);
                }
            }
            if (paintServer is InheritPaintServer)
            {
                var p = shape.RealParent ?? shape.Parent;
                while (p != null)
                {
                    if (p.Fill != null)
                    {
                        var checkPaintServer = svg.PaintServers.GetServer(p.Fill.PaintServerKey);
                        if (checkPaintServer != null && checkPaintServer is not InheritPaintServer)
                        {
                            return checkPaintServer.GetBrush(this.Opacity * elementOpacity, svg, svgRender, bounds);
                        }
                    }
                    p = p.RealParent ?? p.Parent;
                }
                return null!;
            }
            return paintServer.GetBrush(this.Opacity * elementOpacity, svg, svgRender, bounds);
        }
        return null!;
    }
}
