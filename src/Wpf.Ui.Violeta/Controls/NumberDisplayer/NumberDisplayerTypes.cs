using System;
using System.Globalization;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

#region int

/// <summary>Animated int display. Mirrors Ursa's <c>Int32Displayer</c>.</summary>
public class Int32Displayer : NumberDisplayer<int>
{
    static Int32Displayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Int32Displayer),
            new FrameworkPropertyMetadata(typeof(Int32Displayer)));
    }

    protected override int Interpolate(double progress, int oldValue, int newValue)
    {
        return oldValue + (int)((newValue - oldValue) * progress);
    }

    protected override string GetString(int value)
    {
        return string.IsNullOrEmpty(StringFormat)
            ? value.ToString(CultureInfo.CurrentCulture)
            : value.ToString(StringFormat, CultureInfo.CurrentCulture);
    }
}

#endregion int

#region long

/// <summary>Animated long display. Mirrors Ursa's <c>Int64Displayer</c>.</summary>
public class Int64Displayer : NumberDisplayer<long>
{
    static Int64Displayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Int64Displayer),
            new FrameworkPropertyMetadata(typeof(Int64Displayer)));
    }

    protected override long Interpolate(double progress, long oldValue, long newValue)
    {
        return oldValue + (long)((newValue - oldValue) * progress);
    }

    protected override string GetString(long value)
    {
        return string.IsNullOrEmpty(StringFormat)
            ? value.ToString(CultureInfo.CurrentCulture)
            : value.ToString(StringFormat, CultureInfo.CurrentCulture);
    }
}

#endregion long

#region double

/// <summary>Animated double display. Mirrors Ursa's <c>DoubleDisplayer</c>.</summary>
public class DoubleDisplayer : NumberDisplayer<double>
{
    static DoubleDisplayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DoubleDisplayer),
            new FrameworkPropertyMetadata(typeof(DoubleDisplayer)));
    }

    protected override double Interpolate(double progress, double oldValue, double newValue)
    {
        return oldValue + (newValue - oldValue) * progress;
    }

    protected override string GetString(double value)
    {
        return string.IsNullOrEmpty(StringFormat)
            ? value.ToString(CultureInfo.CurrentCulture)
            : value.ToString(StringFormat, CultureInfo.CurrentCulture);
    }
}

#endregion double

#region DateTime

/// <summary>Animated <see cref="DateTime"/> display. Mirrors Ursa's <c>DateDisplay</c>.</summary>
public class DateDisplay : NumberDisplayer<DateTime>
{
    static DateDisplay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DateDisplay),
            new FrameworkPropertyMetadata(typeof(DateDisplay)));
    }

    protected override DateTime Interpolate(double progress, DateTime oldValue, DateTime newValue)
    {
        var diff = (newValue - oldValue).TotalSeconds;
        try
        {
            return oldValue + TimeSpan.FromSeconds(diff * progress);
        }
        catch
        {
            return oldValue;
        }
    }

    protected override string GetString(DateTime value)
    {
        return string.IsNullOrEmpty(StringFormat)
            ? value.ToString(CultureInfo.CurrentCulture)
            : value.ToString(StringFormat, CultureInfo.CurrentCulture);
    }
}

#endregion DateTime
