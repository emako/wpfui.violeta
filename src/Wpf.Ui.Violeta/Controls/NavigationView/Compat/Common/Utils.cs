using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class VisualStateUtil
{
    public static void GoToStateIfGroupExists(Control control, string groupName, string stateName, bool useTransitions)
    {
        VisualStateManager.GoToState(control, stateName, useTransitions);
    }
}

internal static class LayoutUtils
{
    public static double MeasureAndGetDesiredWidthFor(UIElement element, Size availableSize)
    {
        double desiredWidth = 0;
        if (element != null)
        {
            element.Measure(availableSize);
            desiredWidth = element.DesiredSize.Width;
        }
        return desiredWidth;
    }

    public static double GetActualWidthFor(FrameworkElement element)
    {
        return (element != null ? element.ActualWidth : 0);
    }
}

internal static class Util
{
    public static Visibility VisibilityFromBool(bool visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }
}
