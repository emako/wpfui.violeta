using System.Windows;
using System.Windows.Controls;

namespace iNKORE.UI.WPF.Modern.Controls
{
    // Minimal compatibility shim used by NavigationView.AutoSuggestBox dependency.
    public class AutoSuggestBox : Control
    {
        static AutoSuggestBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSuggestBox), new FrameworkPropertyMetadata(typeof(AutoSuggestBox)));
        }
    }
}
