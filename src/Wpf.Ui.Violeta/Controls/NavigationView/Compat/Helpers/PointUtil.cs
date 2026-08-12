using System.Windows;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class PointUtil
{
    internal static Rect ToRect(this RECT rc)
    {
        Rect rect = new()
        {
            X = rc.Left,
            Y = rc.Top,
            Width = rc.Right - rc.Left,
            Height = rc.Bottom - rc.Top
        };

        return rect;
    }
}
