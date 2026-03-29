using System.Windows;
using System.Windows.Controls;

namespace iNKORE.UI.WPF.Modern.Controls.Primitives
{
    public class TitleBarControl : Control
    {
        public static readonly DependencyProperty InsideTitleBarProperty =
            DependencyProperty.RegisterAttached(
                "InsideTitleBar",
                typeof(bool),
                typeof(TitleBarControl),
                new PropertyMetadata(false));

        internal static bool GetInsideTitleBar(UIElement element)
        {
            return (bool)element.GetValue(InsideTitleBarProperty);
        }

        internal static void SetInsideTitleBar(UIElement element, bool value)
        {
            element.SetValue(InsideTitleBarProperty, value);
        }
    }
}
