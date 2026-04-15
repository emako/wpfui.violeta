#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class ThicknessHelper
{
    public static Thickness FromLengths(double left, double top, double right, double bottom)
    {
        return new Thickness(left, top, right, bottom);
    }

    public static Thickness FromUniformLength(double uniformLength)
    {
        return new Thickness(uniformLength);
    }
}
