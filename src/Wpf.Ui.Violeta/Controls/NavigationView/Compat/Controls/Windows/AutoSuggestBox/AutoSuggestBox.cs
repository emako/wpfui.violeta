using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat;

// Minimal compatibility shim used by NavigationView.AutoSuggestBox dependency.
public class AutoSuggestBox : Control
{
    static AutoSuggestBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSuggestBox), new FrameworkPropertyMetadata(typeof(AutoSuggestBox)));
    }
}

