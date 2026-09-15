using System;
using System.Collections.Generic;

namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

public class LengthContext(Shape owner, LengthUnit unit)
{
    public Shape Owner { get; set; } = owner;
    public LengthUnit Unit { get; set; } = unit;

    private static readonly Dictionary<string, LengthUnit> _unitMap = new(StringComparer.OrdinalIgnoreCase)
    {
        {"em", LengthUnit.em},
        {"ex", LengthUnit.ex},
        {"ch", LengthUnit.ch},
        {"rem", LengthUnit.rem},
        {"vw", LengthUnit.vw},
        {"vh", LengthUnit.vh},
        {"vmin", LengthUnit.vmin},
        {"vmax", LengthUnit.vmax},
        {"cm", LengthUnit.cm},
        {"mm", LengthUnit.mm},
        {"Q", LengthUnit.Q},
        {"in", LengthUnit.Inches},
        {"pc", LengthUnit.pc},
        {"pt", LengthUnit.pt},
        {"px", LengthUnit.px},
    };

    public static LengthUnit Parse(string text, LengthOrientation orientation = LengthOrientation.None)
    {
        if (string.IsNullOrEmpty(text))
        {
            return LengthUnit.Number;
        }
        string trimmed = text.Trim();
        if (trimmed == "%")
        {
            return orientation switch
            {
                LengthOrientation.Horizontal => LengthUnit.PercentWidth,
                LengthOrientation.Vertical => LengthUnit.PercentHeight,
                _ => LengthUnit.PercentDiagonal,
            };
        }
        if (_unitMap.TryGetValue(trimmed, out LengthUnit unit))
        {
            return unit;
        }
        return LengthUnit.Unknown;
    }
}
