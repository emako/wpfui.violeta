using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Wpf.Ui.Violeta.Controls;

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
