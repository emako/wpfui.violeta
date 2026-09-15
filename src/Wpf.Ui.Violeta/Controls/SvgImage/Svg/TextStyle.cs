namespace Wpf.Ui.Violeta.Controls.Svg;

using System.Windows;
using Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Utils;

public sealed class TextStyle
{
    private static readonly FontResolver _fontResolver = new(0);

    //This should be configurable in some way.
    private static readonly TextStyle _defaults = new()
    {
        FontFamily = "Arial Unicode MS, Verdana",
        FontSize = 12,
        Fontweight = FontWeights.Normal,
        Fontstyle = FontStyles.Normal,
        TextAlignment = System.Windows.TextAlignment.Left,
        WordSpacing = 0,
        LetterSpacing = 0,
        BaseLineShift = string.Empty,
    };

    private string _fontFamily = null!;
    private double? _fontSize;
    private FontWeight? _fontweight;
    private FontStyle? _fontstyle;
    private TextAlignment? _textAlignment;
    private double? _wordSpacing;
    private double? _letterSpacing;
    private string _baseLineShift = null!;

    public TextStyle(TextStyle aCopy)
    {
        Copy(aCopy);
    }

    private TextStyle()
    {
    }

    public TextStyle(Shape owner)
    {
        if (owner.Parent != null)
        {
            Copy(owner.Parent.TextStyle);
        }
    }

    public Typeface GetTypeface()
    {
        var fontFamily = _fontResolver.ResolveFontFamily(FontFamily);
        return new Typeface(fontFamily,
        Fontstyle,
        Fontweight,
        FontStretch.FromOpenTypeStretch(9),
        new FontFamily(_defaults.FontFamily));
    }

    public GlyphTypeface GetGlyphTypeface()
    {
        var typeface = GetTypeface();
        if (typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
        {
            return glyphTypeface;
        }
        return null!;
    }

    public string FontFamily
    {
        get => _fontFamily ?? _defaults.FontFamily;
        set => _fontFamily = value;
    }

    public double FontSize
    {
        get => _fontSize ?? _defaults.FontSize;
        set => _fontSize = value;
    }

    public FontWeight Fontweight
    {
        get => _fontweight ?? _defaults.Fontweight;
        set => _fontweight = value;
    }

    public FontStyle Fontstyle
    {
        get => _fontstyle ?? _defaults.Fontstyle;
        set => _fontstyle = value;
    }

    public TextDecorationCollection TextDecoration { get; set; } = null!;

    public TextAlignment TextAlignment
    {
        get => _textAlignment ?? _defaults.TextAlignment;
        set => _textAlignment = value;
    }

    public double WordSpacing
    {
        get => _wordSpacing ?? _defaults.WordSpacing;
        set => _wordSpacing = value;
    }

    public double LetterSpacing
    {
        get => _letterSpacing ?? _defaults.LetterSpacing;
        set => _letterSpacing = value;
    }

    public string BaseLineShift
    {
        get => _baseLineShift ?? _defaults.BaseLineShift;
        set => _baseLineShift = value;
    }

    private void Copy(TextStyle aCopy)
    {
        if (aCopy == null)
            return;
        _fontFamily = aCopy._fontFamily;
        _fontSize = aCopy._fontSize;
        _fontweight = aCopy._fontweight;
        _fontstyle = aCopy._fontstyle;
        _textAlignment = aCopy._textAlignment;
        _wordSpacing = aCopy._wordSpacing;
        _letterSpacing = aCopy._letterSpacing;
        _baseLineShift = aCopy._baseLineShift;
    }

    public static TextStyle Merge(TextStyle baseStyle, TextStyle overrides)
    {
        var result = new TextStyle
        {
            _fontFamily = overrides._fontFamily ?? baseStyle._fontFamily,
            _fontSize = overrides._fontSize ?? baseStyle._fontSize,
            _fontweight = overrides._fontweight ?? baseStyle._fontweight,
            _fontstyle = overrides._fontstyle ?? baseStyle._fontstyle,
            _textAlignment = overrides._textAlignment ?? baseStyle._textAlignment,
            _wordSpacing = overrides._wordSpacing ?? baseStyle._wordSpacing,
            _letterSpacing = overrides._letterSpacing ?? baseStyle._letterSpacing,
            _baseLineShift = overrides._baseLineShift ?? baseStyle._baseLineShift
        };

        if (overrides.TextDecoration != null)
        {
            //None was explicitly set
            if (overrides.TextDecoration.Count <= 0)
            {
                result.TextDecoration = [];
            }
            //Copy overrides
            else if (baseStyle.TextDecoration == null || baseStyle.TextDecoration.Count <= 0)
            {
                result.TextDecoration = [with(overrides.TextDecoration.Select(CopyTextDecoration))];
            }
            //merge with base style
            else
            {
                var merged = new List<TextDecoration>();
                merged.AddRange(baseStyle.TextDecoration.Select(CopyTextDecoration));
                merged.AddRange(overrides.TextDecoration.Select(CopyTextDecoration));
                result.TextDecoration = [with(merged)];
            }
        }
        else if (baseStyle.TextDecoration != null)
        {
            result.TextDecoration = [.. baseStyle.TextDecoration.Select(CopyTextDecoration)];
        }
        else
        {
            result.TextDecoration = null!;
        }
        return result;
    }

    /// <summary>
    /// Does not clone the TextDecorationCollection, but creates a new instance with the same properties. This prevents issues with shared references in the TextDecorationCollection.
    /// </summary>
    /// <param name="textDecoration">The TextDecoration to copy.</param>
    /// <returns>
    /// A new TextDecoration instance with the same properties as the original.
    /// </returns>
    private static TextDecoration CopyTextDecoration(TextDecoration textDecoration)
    {
        return new TextDecoration(textDecoration.Location, textDecoration.Pen, textDecoration.PenOffset, textDecoration.PenOffsetUnit, textDecoration.PenThicknessUnit);
    }
}
