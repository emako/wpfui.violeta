using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class PointUtil
{
    internal static Rect ToRect(this RECT rc)
    {
        Rect rect = new()
        {
            X = rc.left,
            Y = rc.top,
            Width = rc.right - rc.left,
            Height = rc.bottom - rc.top
        };

        return rect;
    }
}
