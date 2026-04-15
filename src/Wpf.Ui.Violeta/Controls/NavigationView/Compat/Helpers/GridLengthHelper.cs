#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class GridLengthHelper
{
    public static GridLength FromPixels(double pixels)
    {
        return new GridLength(pixels);
    }

    public static GridLength FromValueAndType(double value, GridUnitType type)
    {
        return new GridLength(value, type);
    }
}
