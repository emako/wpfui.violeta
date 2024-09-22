using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

public class TreeModelRowExpander : ContentControl
{
    static TreeModelRowExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeModelRowExpander), new FrameworkPropertyMetadata(typeof(TreeModelRowExpander)));
    }
}
