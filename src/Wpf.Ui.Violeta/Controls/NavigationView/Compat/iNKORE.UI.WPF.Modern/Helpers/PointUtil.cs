using System.Windows;
//using Windows.Win32.Foundation;
using iNKORE.UI.WPF.Modern.Native;

namespace iNKORE.UI.WPF.Modern.Helpers
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
