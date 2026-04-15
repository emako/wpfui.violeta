#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class DoubleUtil
{
    public static int DoubleToInt(double val)
    {
        return 0 < val ? (int)(val + 0.5) : (int)(val - 0.5);
    }
}
