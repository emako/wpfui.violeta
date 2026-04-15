#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class ThemeResourceHelper
{
    private static readonly DependencyProperty ColorKeyProperty =
        DependencyProperty.RegisterAttached(
            "ColorKey",
            typeof(object),
            typeof(ThemeResourceHelper));

    internal static object GetColorKey(SolidColorBrush element)
    {
        return element.GetValue(ColorKeyProperty);
    }

    internal static void SetColorKey(SolidColorBrush element, object value)
    {
        element.SetValue(ColorKeyProperty, value);
    }
}
