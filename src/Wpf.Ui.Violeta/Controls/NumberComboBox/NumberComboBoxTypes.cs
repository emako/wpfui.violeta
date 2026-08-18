using System.Globalization;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

// ----------------------------------------------------------------------------
// Each concrete class points its DefaultStyleKey to itself; the XAML resource
// dictionary contains a style for each type (BasedOn the shared ComboBox style).
// ----------------------------------------------------------------------------

#region int

/// <summary>NumberComboBox for <see cref="int"/> values.</summary>
public class NumberIntComboBox : NumberComboBoxBase<int>
{
    static NumberIntComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberIntComboBox),
            new FrameworkPropertyMetadata(typeof(NumberIntComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberIntComboBox), new FrameworkPropertyMetadata(int.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberIntComboBox), new FrameworkPropertyMetadata(int.MaxValue));
    }

    protected override int Zero => 0;

    protected override int? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => int.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion int

#region uint

/// <summary>NumberComboBox for <see cref="uint"/> values.</summary>
public class NumberUIntComboBox : NumberComboBoxBase<uint>
{
    static NumberUIntComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberUIntComboBox),
            new FrameworkPropertyMetadata(typeof(NumberUIntComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberUIntComboBox), new FrameworkPropertyMetadata(uint.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberUIntComboBox), new FrameworkPropertyMetadata(uint.MaxValue));
    }

    protected override uint Zero => 0u;

    protected override uint? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => uint.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion uint

#region double

/// <summary>NumberComboBox for <see cref="double"/> values.</summary>
public class NumberDoubleComboBox : NumberComboBoxBase<double>
{
    static NumberDoubleComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberDoubleComboBox),
            new FrameworkPropertyMetadata(typeof(NumberDoubleComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberDoubleComboBox), new FrameworkPropertyMetadata(double.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberDoubleComboBox), new FrameworkPropertyMetadata(double.MaxValue));
    }

    protected override double Zero => 0.0;

    protected override bool IsFloatingPointInput => true;

    protected override double? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => double.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion double

#region float

/// <summary>NumberComboBox for <see cref="float"/> values.</summary>
public class NumberFloatComboBox : NumberComboBoxBase<float>
{
    static NumberFloatComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberFloatComboBox),
            new FrameworkPropertyMetadata(typeof(NumberFloatComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberFloatComboBox), new FrameworkPropertyMetadata(float.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberFloatComboBox), new FrameworkPropertyMetadata(float.MaxValue));
    }

    protected override float Zero => 0f;

    protected override bool IsFloatingPointInput => true;

    protected override float? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => float.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion float

#region decimal

/// <summary>NumberComboBox for <see cref="decimal"/> values.</summary>
public class NumberDecimalComboBox : NumberComboBoxBase<decimal>
{
    static NumberDecimalComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberDecimalComboBox),
            new FrameworkPropertyMetadata(typeof(NumberDecimalComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberDecimalComboBox), new FrameworkPropertyMetadata(decimal.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberDecimalComboBox), new FrameworkPropertyMetadata(decimal.MaxValue));
    }

    protected override decimal Zero => 0m;

    protected override bool IsFloatingPointInput => true;

    protected override decimal? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => decimal.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion decimal

#region long

/// <summary>NumberComboBox for <see cref="long"/> values.</summary>
public class NumberLongComboBox : NumberComboBoxBase<long>
{
    static NumberLongComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberLongComboBox),
            new FrameworkPropertyMetadata(typeof(NumberLongComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberLongComboBox), new FrameworkPropertyMetadata(long.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberLongComboBox), new FrameworkPropertyMetadata(long.MaxValue));
    }

    protected override long Zero => 0L;

    protected override long? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => long.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion long

#region ulong

/// <summary>NumberComboBox for <see cref="ulong"/> values.</summary>
public class NumberULongComboBox : NumberComboBoxBase<ulong>
{
    static NumberULongComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberULongComboBox),
            new FrameworkPropertyMetadata(typeof(NumberULongComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberULongComboBox), new FrameworkPropertyMetadata(ulong.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberULongComboBox), new FrameworkPropertyMetadata(ulong.MaxValue));
    }

    protected override ulong Zero => 0UL;

    protected override ulong? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => ulong.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion ulong

#region short

/// <summary>NumberComboBox for <see cref="short"/> values.</summary>
public class NumberShortComboBox : NumberComboBoxBase<short>
{
    static NumberShortComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberShortComboBox),
            new FrameworkPropertyMetadata(typeof(NumberShortComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberShortComboBox), new FrameworkPropertyMetadata(short.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberShortComboBox), new FrameworkPropertyMetadata(short.MaxValue));
    }

    protected override short Zero => 0;

    protected override short? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => short.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion short

#region ushort

/// <summary>NumberComboBox for <see cref="ushort"/> values.</summary>
public class NumberUShortComboBox : NumberComboBoxBase<ushort>
{
    static NumberUShortComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberUShortComboBox),
            new FrameworkPropertyMetadata(typeof(NumberUShortComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberUShortComboBox), new FrameworkPropertyMetadata(ushort.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberUShortComboBox), new FrameworkPropertyMetadata(ushort.MaxValue));
    }

    protected override ushort Zero => 0;

    protected override ushort? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => ushort.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion ushort

#region byte

/// <summary>NumberComboBox for <see cref="byte"/> values.</summary>
public class NumberByteComboBox : NumberComboBoxBase<byte>
{
    static NumberByteComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberByteComboBox),
            new FrameworkPropertyMetadata(typeof(NumberByteComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberByteComboBox), new FrameworkPropertyMetadata(byte.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberByteComboBox), new FrameworkPropertyMetadata(byte.MaxValue));
    }

    protected override byte Zero => 0;

    protected override byte? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => byte.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion byte

#region sbyte

/// <summary>NumberComboBox for <see cref="sbyte"/> values.</summary>
public class NumberSByteComboBox : NumberComboBoxBase<sbyte>
{
    static NumberSByteComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberSByteComboBox),
            new FrameworkPropertyMetadata(typeof(NumberSByteComboBox)));

        MinimumProperty.OverrideMetadata(typeof(NumberSByteComboBox), new FrameworkPropertyMetadata(sbyte.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumberSByteComboBox), new FrameworkPropertyMetadata(sbyte.MaxValue));
    }

    protected override sbyte Zero => 0;

    protected override sbyte? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => sbyte.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion sbyte
