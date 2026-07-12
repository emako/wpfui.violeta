#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class ControlExtensions
{
    public static FrameworkElement GetTemplateRoot(this Control control)
    {
        if (VisualTreeHelper.GetChildrenCount(control) > 0)
        {
            return VisualTreeHelper.GetChild(control, 0) as FrameworkElement;
        }
        return null;
    }

    public static T GetTemplateChild<T>(this Control control, string childName) where T : DependencyObject
    {
        return control.Template?.FindName(childName, control) as T;
    }
}
