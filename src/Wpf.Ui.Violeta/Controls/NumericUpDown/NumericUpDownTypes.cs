using System;
using System.Globalization;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

// ────────────────────────────────────────────────────────────────────────────
// Default style key helper: all typed classes share the NumericUpDown style.
// ────────────────────────────────────────────────────────────────────────────

#region int

/// <summary>NumericUpDown for <see cref="int"/> values. Mirrors Ursa's <c>IntUpDown</c>.</summary>
public class NumericIntUpDown : NumericUpDownBase<int>
{
    static NumericIntUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericIntUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        // Override defaults for int
        MinimumProperty.OverrideMetadata(typeof(NumericIntUpDown), new FrameworkPropertyMetadata(int.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericIntUpDown), new FrameworkPropertyMetadata(int.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericIntUpDown), new FrameworkPropertyMetadata(1));
    }

    protected override int Zero => 0;
    protected override int Add(int a, int b) => checked(a + b);
    protected override int Subtract(int a, int b) => checked(a - b);

    protected override int? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => int.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region uint

/// <summary>NumericUpDown for <see cref="uint"/> values. Mirrors Ursa's <c>UIntUpDown</c>.</summary>
public class NumericUIntUpDown : NumericUpDownBase<uint>
{
    static NumericUIntUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericUIntUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericUIntUpDown), new FrameworkPropertyMetadata(uint.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericUIntUpDown), new FrameworkPropertyMetadata(uint.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericUIntUpDown), new FrameworkPropertyMetadata(1u));
    }

    protected override uint Zero => 0u;
    protected override uint Add(uint a, uint b) => checked(a + b);
    protected override uint Subtract(uint a, uint b) => checked(a - b);

    protected override uint? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => uint.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region double

/// <summary>NumericUpDown for <see cref="double"/> values. Mirrors Ursa's <c>DoubleUpDown</c>.</summary>
public class NumericDoubleUpDown : NumericUpDownBase<double>
{
    static NumericDoubleUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericDoubleUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericDoubleUpDown), new FrameworkPropertyMetadata(double.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericDoubleUpDown), new FrameworkPropertyMetadata(double.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericDoubleUpDown), new FrameworkPropertyMetadata(1.0));
    }

    protected override double Zero => 0.0;
    protected override double Add(double a, double b) => a + b;
    protected override double Subtract(double a, double b) => a - b;

    protected override double? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => double.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region float

/// <summary>NumericUpDown for <see cref="float"/> values. Mirrors Ursa's <c>FloatUpDown</c>.</summary>
public class NumericFloatUpDown : NumericUpDownBase<float>
{
    static NumericFloatUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericFloatUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericFloatUpDown), new FrameworkPropertyMetadata(float.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericFloatUpDown), new FrameworkPropertyMetadata(float.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericFloatUpDown), new FrameworkPropertyMetadata(1f));
    }

    protected override float Zero => 0f;
    protected override float Add(float a, float b) => a + b;
    protected override float Subtract(float a, float b) => a - b;

    protected override float? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => float.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region decimal

/// <summary>NumericUpDown for <see cref="decimal"/> values. Mirrors Ursa's <c>DecimalUpDown</c>.</summary>
public class NumericDecimalUpDown : NumericUpDownBase<decimal>
{
    static NumericDecimalUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericDecimalUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericDecimalUpDown), new FrameworkPropertyMetadata(decimal.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericDecimalUpDown), new FrameworkPropertyMetadata(decimal.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericDecimalUpDown), new FrameworkPropertyMetadata(1m));
    }

    protected override decimal Zero => 0m;
    protected override decimal Add(decimal a, decimal b) => a + b;
    protected override decimal Subtract(decimal a, decimal b) => a - b;

    protected override decimal? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => decimal.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region long

/// <summary>NumericUpDown for <see cref="long"/> values. Mirrors Ursa's <c>LongUpDown</c>.</summary>
public class NumericLongUpDown : NumericUpDownBase<long>
{
    static NumericLongUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericLongUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericLongUpDown), new FrameworkPropertyMetadata(long.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericLongUpDown), new FrameworkPropertyMetadata(long.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericLongUpDown), new FrameworkPropertyMetadata(1L));
    }

    protected override long Zero => 0L;
    protected override long Add(long a, long b) => checked(a + b);
    protected override long Subtract(long a, long b) => checked(a - b);

    protected override long? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => long.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region ulong

/// <summary>NumericUpDown for <see cref="ulong"/> values. Mirrors Ursa's <c>ULongUpDown</c>.</summary>
public class NumericULongUpDown : NumericUpDownBase<ulong>
{
    static NumericULongUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericULongUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericULongUpDown), new FrameworkPropertyMetadata(ulong.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericULongUpDown), new FrameworkPropertyMetadata(ulong.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericULongUpDown), new FrameworkPropertyMetadata(1UL));
    }

    protected override ulong Zero => 0UL;
    protected override ulong Add(ulong a, ulong b) => checked(a + b);
    protected override ulong Subtract(ulong a, ulong b) => checked(a - b);

    protected override ulong? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => ulong.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region short

/// <summary>NumericUpDown for <see cref="short"/> values. Mirrors Ursa's <c>ShortUpDown</c>.</summary>
public class NumericShortUpDown : NumericUpDownBase<short>
{
    static NumericShortUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericShortUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericShortUpDown), new FrameworkPropertyMetadata(short.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericShortUpDown), new FrameworkPropertyMetadata(short.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericShortUpDown), new FrameworkPropertyMetadata((short)1));
    }

    protected override short Zero => 0;
    protected override short Add(short a, short b) => checked((short)(a + b));
    protected override short Subtract(short a, short b) => checked((short)(a - b));

    protected override short? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => short.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region ushort

/// <summary>NumericUpDown for <see cref="ushort"/> values. Mirrors Ursa's <c>UShortUpDown</c>.</summary>
public class NumericUShortUpDown : NumericUpDownBase<ushort>
{
    static NumericUShortUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericUShortUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericUShortUpDown), new FrameworkPropertyMetadata(ushort.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericUShortUpDown), new FrameworkPropertyMetadata(ushort.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericUShortUpDown), new FrameworkPropertyMetadata((ushort)1));
    }

    protected override ushort Zero => 0;
    protected override ushort Add(ushort a, ushort b) => checked((ushort)(a + b));
    protected override ushort Subtract(ushort a, ushort b) => checked((ushort)(a - b));

    protected override ushort? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => ushort.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region byte

/// <summary>NumericUpDown for <see cref="byte"/> values. Mirrors Ursa's <c>ByteUpDown</c>.</summary>
public class NumericByteUpDown : NumericUpDownBase<byte>
{
    static NumericByteUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericByteUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericByteUpDown), new FrameworkPropertyMetadata(byte.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericByteUpDown), new FrameworkPropertyMetadata(byte.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericByteUpDown), new FrameworkPropertyMetadata((byte)1));
    }

    protected override byte Zero => 0;
    protected override byte Add(byte a, byte b) => checked((byte)(a + b));
    protected override byte Subtract(byte a, byte b) => checked((byte)(a - b));

    protected override byte? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => byte.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion

#region sbyte

/// <summary>NumericUpDown for <see cref="sbyte"/> values. Mirrors Ursa's <c>SByteUpDown</c>.</summary>
public class NumericSByteUpDown : NumericUpDownBase<sbyte>
{
    static NumericSByteUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericSByteUpDown),
            new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        MinimumProperty.OverrideMetadata(typeof(NumericSByteUpDown), new FrameworkPropertyMetadata(sbyte.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericSByteUpDown), new FrameworkPropertyMetadata(sbyte.MaxValue));
        StepProperty.OverrideMetadata(typeof(NumericSByteUpDown), new FrameworkPropertyMetadata((sbyte)1));
    }

    protected override sbyte Zero => 0;
    protected override sbyte Add(sbyte a, sbyte b) => checked((sbyte)(a + b));
    protected override sbyte Subtract(sbyte a, sbyte b) => checked((sbyte)(a - b));

    protected override sbyte? ParseText(string? text, NumberFormatInfo numberFormat, NumberStyles numberStyles)
        => sbyte.TryParse(text, numberStyles, numberFormat, out var v) ? v : null;
}

#endregion
