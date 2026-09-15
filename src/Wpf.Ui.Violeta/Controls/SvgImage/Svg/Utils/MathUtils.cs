using System;

namespace Wpf.Ui.Violeta.Controls.Svg.Utils;

internal static class MathUtil
{
    public static bool IsNearlyZero(this double value, double epsilon = double.Epsilon)
    {
        return Math.Abs(value) < epsilon;
    }

    public static bool IsNearlyEqual(this double a, double b, double epsilon = double.Epsilon)
    {
        return Math.Abs(a - b) < epsilon;
    }

    public static int Clamp(int value, int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("min must be less than or equal to max", nameof(min));
        }

        if (value < min)
        {
            return min;
        }
        else if (value > max)
        {
            return max;
        }

        return value;
    }
}
