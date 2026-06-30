#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class CachedVisualTreeHelpers
{
    public static Rect GetLayoutSlot(FrameworkElement element)
    {
        return LayoutInformation.GetLayoutSlot(element);
    }

    public static DependencyObject GetParent(DependencyObject element)
    {
        if (element is Visual || element is Visual3D)
        {
            return VisualTreeHelper.GetParent(element);
        }

        if (element is FrameworkContentElement fce)
        {
            return fce.Parent;
        }

        return null;
    }
}
