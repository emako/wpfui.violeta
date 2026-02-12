using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

public class RunningBorder : Border
{
    private bool _test;

    protected override Size MeasureOverride(Size constraint)
    {
        if (_test)
        {
            _test = false;
            return constraint;
        }

        var child = Child;
        var borderThickness = BorderThickness;
        var padding = Padding;

        if (UseLayoutRounding)
        {
            var dpiScaleX = DpiHelper.ScaleX;
            var dpiScaleY = DpiHelper.ScaleY;

            borderThickness = new Thickness(
                DpiHelper2.RoundLayoutValue(borderThickness.Left, dpiScaleX),
                DpiHelper2.RoundLayoutValue(borderThickness.Top, dpiScaleY),
                DpiHelper2.RoundLayoutValue(borderThickness.Right, dpiScaleX),
                DpiHelper2.RoundLayoutValue(borderThickness.Bottom, dpiScaleY));
        }

        var borderSize = ConvertThickness2Size(borderThickness);
        var paddingSize = ConvertThickness2Size(padding);
        var mySize = new Size();

        if (child != null)
        {
            var combined = new Size(borderSize.Width + paddingSize.Width, borderSize.Height + paddingSize.Height);
            var borderConstraint = new Size(Math.Max(0.0, constraint.Width - combined.Width), Math.Max(0.0, constraint.Height - combined.Height));
            var childConstraint = new Size(Math.Max(0.0, double.PositiveInfinity - combined.Width), Math.Max(0.0, double.PositiveInfinity - combined.Height));

            child.Measure(borderConstraint);
            var childSize = child.DesiredSize;

            mySize.Width = childSize.Width + combined.Width;
            mySize.Height = childSize.Height + combined.Height;

            child.Measure(childConstraint);
        }
        else
        {
            mySize = new Size(borderSize.Width + paddingSize.Width, borderSize.Height + paddingSize.Height);
        }

        return mySize;
    }

    private static Size ConvertThickness2Size(Thickness th) => new(th.Left + th.Right, th.Top + th.Bottom);
}

file static class DpiHelper2
{
    public static double RoundLayoutValue(double value, double dpiScale)
    {
        double newValue;

        if (!MathHelper.AreClose(dpiScale, 1.0))
        {
            newValue = Math.Round(value * dpiScale) / dpiScale;
            if (double.IsNaN(newValue) || double.IsInfinity(newValue) || MathHelper.AreClose(newValue, double.MaxValue))
            {
                newValue = value;
            }
        }
        else
        {
            newValue = Math.Round(value);
        }

        return newValue;
    }
}

file static class MathHelper
{
    public static bool AreClose(double value1, double value2) =>
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        value1 == value2 || IsVerySmall(value1 - value2);

    public static bool IsVerySmall(double value) => Math.Abs(value) < 1E-06;
}
