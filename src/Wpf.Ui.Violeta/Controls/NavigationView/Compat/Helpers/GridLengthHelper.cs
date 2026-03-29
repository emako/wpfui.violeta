using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat
{
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
}

