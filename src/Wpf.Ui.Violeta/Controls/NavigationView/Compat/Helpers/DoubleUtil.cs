namespace Wpf.Ui.Violeta.Controls.Compat
{
    public static class DoubleUtil
    {
        public static int DoubleToInt(double val)
        {
            return 0 < val ? (int)(val + 0.5) : (int)(val - 0.5);
        }
    }
}

