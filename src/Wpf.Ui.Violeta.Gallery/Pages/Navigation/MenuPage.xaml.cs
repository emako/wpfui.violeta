using System.Windows;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Navigation;

public partial class MenuPage : Page
{
    public MenuPage()
    {
        InitializeComponent();
    }

    private void OnOpenContextMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { ContextMenu: { } menu } host)
        {
            return;
        }

        menu.PlacementTarget = host;
        menu.Placement = PlacementMode.Bottom;
        menu.HorizontalOffset = 0;
        menu.VerticalOffset = 1;
        menu.IsOpen = true;
    }
}
