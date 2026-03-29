using System.Windows;
//using Windows.Win32.Foundation;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls.Compat
{
    internal static class PointUtil
    {
        internal static Rect ToRect(this RECT rc)
        {
            Rect rect = new Rect();

            rect.X = rc.left;
            rect.Y = rc.top;
            rect.Width = rc.right - rc.left;
            rect.Height = rc.bottom - rc.top;

            return rect;
        }
    }
}


