using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class VisualStateUtil
{
    /*
    public static VisualStateGroup GetVisualStateGroup(FrameworkElement control, string groupName)
    {
        VisualStateGroup group = null;
        var visualStateGroups = VisualStateManager.GetVisualStateGroups(control);
        foreach (VisualStateGroup visualStateGroup in visualStateGroups)
        {
            if (visualStateGroup.Name == groupName)
            {
                group = visualStateGroup;
                return group;
            }
        }
        return group;
    }
    */

    public static void GoToStateIfGroupExists(Control control, string groupName, string stateName, bool useTransitions)
    {
        //var visualStateGroup = GetVisualStateGroup(control, groupName);
        //if (visualStateGroup != null)
        {
            VisualStateManager.GoToState(control, stateName, useTransitions);
        }
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

