using System.Windows;
using System.Windows.Controls;

namespace ToolBarOverflowRepro;

public partial class ToolsPage : UserControl
{
    public ToolsPage()
    {
        InitializeComponent();
        menuItemOpenExportPage.Click += (_, _) => Navigate(exportPage);
        buttonBack.Click += (_, _) => Navigate(toolsHome);
    }

    public void Navigate(UIElement target)
    {
        foreach (UIElement child in toolsHost.Children)
        {
            child.Visibility = ReferenceEquals(child, target) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
