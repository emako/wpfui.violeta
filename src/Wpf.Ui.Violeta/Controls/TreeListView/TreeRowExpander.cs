using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

public class TreeRowExpander : ContentControl
{
    static TreeRowExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeRowExpander), new FrameworkPropertyMetadata(typeof(TreeRowExpander)));
    }
}
