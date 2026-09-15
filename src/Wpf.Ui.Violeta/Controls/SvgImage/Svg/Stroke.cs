using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Svg;

using PaintServers;
using Shapes;
using System.Diagnostics.CodeAnalysis;

public sealed class Stroke(SVG svg)
{
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public enum eLineCap
    {
        butt,
        round,
        square,
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public enum eLineJoin
    {
        miter,
        round,
        bevel,
    }

    private bool _isDefault = false;

    private string _paintServerKey = null!;

    private double _opacity = 100;

    private double _width = 1;

    private eLineCap _lineCap = eLineCap.butt;

    private eLineJoin _lineJoin = eLineJoin.miter;

    private double[] _strokeArray = null!;

    public static Stroke CreateDefault(SVG svg, double width)
    {
        var stroke = new Stroke(svg)
        {
            _width = width,
            _isDefault = true
        };

        return stroke;
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public SVG get => svg;

    public bool IsDefault
    {
        get => _isDefault;
        set => _isDefault = value;
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

    public double Width
    {
        get => _width;
        set
        {
            Debug.Assert(_isDefault == false);
            _width = value;
        }
    }

    public eLineCap LineCap
    {
        get => _lineCap;
        set
        {
            Debug.Assert(_isDefault == false);
            _lineCap = value;
        }
    }

    public eLineJoin LineJoin
    {
        get => _lineJoin;
        set
        {
            Debug.Assert(_isDefault == false);
            _lineJoin = value;
        }
    }

    public double[] StrokeArray
    {
        get => _strokeArray;
        set
        {
            Debug.Assert(_isDefault == false);
            _strokeArray = value;
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

    public Brush StrokeBrush(SVG svg, SVGRender svgRender, Shape shape, double elementOpacity, Rect bounds)
    {
        var paintServer = svg.PaintServers.GetServer(PaintServerKey);
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
                    if (p.Stroke != null)
                    {
                        var checkPaintServer = svg.PaintServers.GetServer(p.Stroke.PaintServerKey);
                        if (checkPaintServer is not InheritPaintServer)
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
