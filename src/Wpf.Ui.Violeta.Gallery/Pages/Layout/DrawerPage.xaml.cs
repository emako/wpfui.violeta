using System.Windows;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Layout;

public partial class DrawerPage : Wpf.Ui.Violeta.Controls.Page
{
    public DrawerPage()
    {
        InitializeComponent();
    }

    private void ShowDrawer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Wpf.Ui.Controls.Button btn)
        {
            return;
        }

        Drawer? target = btn.Tag?.ToString() switch
        {
            "Left" => LeftDrawer,
            "Top" => TopDrawer,
            "Right" => RightDrawer,
            "Bottom" => BottomDrawer,
            _ => null
        };

        if (target is null)
        {
            return;
        }

        bool shouldOpen = !target.IsOpen;
        CloseAll();
        target.IsOpen = shouldOpen;
    }

    private void CloseAll_Click(object sender, RoutedEventArgs e) => CloseAll();

    private void CloseAll()
    {
        LeftDrawer.IsOpen = false;
        TopDrawer.IsOpen = false;
        RightDrawer.IsOpen = false;
        BottomDrawer.IsOpen = false;
    }
}
