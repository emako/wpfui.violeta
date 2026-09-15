using System;

namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a length that may have a value that is context dependent.
/// </summary>
/// <param name="value">The numerical part of the length that is related to the <paramref name="context"/></param>
/// <param name="context">If <see langword="null"/>, units will be ignored</param>
public struct LengthPercentageOrNumber(double value, LengthContext context)
{
    [SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.")]
    private static readonly Regex _lengthRegex = new(@"(?<Value>-?\d+(?:\.\d+)?)\s*(?<Unit>%|\w+)?", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Represents a length, percentage, or number value that has been resolved based on the context.
    /// </summary>
    public readonly double Value => ResolveValue();

    private static double ResolveAbsoluteValue(double value, LengthContext context)
    {
        return context.Unit switch
        {
            LengthUnit.cm => value * 35.43,
            LengthUnit.mm => value * 3.54,
            LengthUnit.Q => value * 3.54 / 4d,
            LengthUnit.Inches => value * 90d,
            LengthUnit.pc => value * 15d,
            LengthUnit.pt => value * 1.25,
            LengthUnit.px => value * 90d / 96d,
            _ => value,
        };
    }

    private static double ResolveViewboxValue(double value, LengthContext context)
    {
        double height;
        double width;
        if (context.Owner.Svg.ViewBox.HasValue)
        {
            height = context.Owner.Svg.ViewBox.Value.Height;
            width = context.Owner.Svg.ViewBox.Value.Width;
        }
        else
        {
            height = context.Owner.Svg.Size.Height;
            width = context.Owner.Svg.Size.Width;
        }
        return context.Unit switch
        {
            LengthUnit.Percent => throw new NotSupportedException($"Percent without specific orientation is not supported. Use ${LengthUnit.PercentWidth}, ${LengthUnit.PercentHeight}, or ${LengthUnit.PercentDiagonal} instead."),
            LengthUnit.PercentDiagonal => (value / 100d) * Math.Sqrt(Math.Pow(width, 2d) + Math.Pow(height, 2d)),
            LengthUnit.vw or LengthUnit.PercentWidth => (value / 100d) * width,
            LengthUnit.vh or LengthUnit.PercentHeight => (value / 100d) * height,
            LengthUnit.vmin => (value / 100d) * Math.Min(width, height),
            LengthUnit.vmax => (value / 100d) * Math.Max(width, height),
            _ => value,
        };
    }

    private static double ResolveRelativeValue(double value, LengthContext context)
    {
        switch (context.Unit)
        {
            case LengthUnit.em:
                return value * context.Owner.TextStyle.FontSize;

            case LengthUnit.ex:
                return value * context.Owner.TextStyle.GetTypeface().XHeight;

            case LengthUnit.ch:
                var glyphTypeface = context.Owner.TextStyle.GetGlyphTypeface();
                return value * glyphTypeface.AdvanceWidths[glyphTypeface.CharacterToGlyphMap['0']];

            case LengthUnit.rem:
                return value * context.Owner.GetRoot().TextStyle.FontSize;

            case LengthUnit.Unknown:
            case LengthUnit.Number:
            default:
                return value;
        }
    }

    private readonly double ResolveValue()
    {
        if (context == null)
        {
            return value; // No context, return raw value
        }

        return context.Unit switch
        {
            LengthUnit.Percent or LengthUnit.PercentWidth or LengthUnit.PercentHeight or LengthUnit.PercentDiagonal or LengthUnit.vw or LengthUnit.vh or LengthUnit.vmin or LengthUnit.vmax => ResolveViewboxValue(value, context),
            LengthUnit.em or LengthUnit.ex or LengthUnit.ch or LengthUnit.rem => ResolveRelativeValue(value, context),
            LengthUnit.cm or LengthUnit.mm or LengthUnit.Q or LengthUnit.Inches or LengthUnit.pc or LengthUnit.pt or LengthUnit.px => ResolveAbsoluteValue(value, context),
            _ => value,
        };
    }

    /// <summary>
    /// Parses a string representation of a length, percentage, or number value into a <see cref="LengthPercentageOrNumber"/> instance.
    /// </summary>
    /// <param name="owner">
    /// The element that the length is associated with.
    /// </param>
    /// <param name="value">A string representation of a length</param>
    /// <param name="orientation">Used to establish the context of the length.
    /// Should be <see cref="LengthOrientation.Horizontal"/> for inherntly horizontal values like 'x' and 'dx'.
    /// Should be <see cref="LengthOrientation.Vertical"/> for inherntly vertical values like 'y' and 'dy'.
    /// Should be <see cref="LengthOrientation.None"/> for other values.
    /// </param>
    /// <returns>
    /// A <see cref="LengthPercentageOrNumber"/> instance that represents the parsed value.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided value is not a valid length, percentage, or number.
    /// </exception>
    public static LengthPercentageOrNumber Parse(Shape owner, string value, LengthOrientation orientation = LengthOrientation.None)
    {
        var lengthMatch = _lengthRegex.Match(value.Trim());
        if (!lengthMatch.Success || !Double.TryParse(lengthMatch.Groups["Value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out double d))
        {
            throw new ArgumentException($"Invalid length/percentage/number value: {value}");
        }
        LengthContext context;
        if (lengthMatch.Groups["Unit"].Success)
        {
            string unitStr = lengthMatch.Groups["Unit"].Value;
            LengthUnit unit = LengthContext.Parse(unitStr, orientation);
            if (unit == LengthUnit.Unknown)
            {
                throw new ArgumentException($"Unknown length unit: {unitStr}");
            }
            context = new LengthContext(owner, unit);
        }
        else
        {
            // Default to Number if no unit is specified
            context = new LengthContext(owner, LengthUnit.Number);
        }
        return new LengthPercentageOrNumber(d, context);
    }
}
