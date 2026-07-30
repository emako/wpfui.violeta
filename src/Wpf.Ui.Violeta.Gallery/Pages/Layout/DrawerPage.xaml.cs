using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Layout;

public partial class DrawerPage : Wpf.Ui.Violeta.Controls.Page
{
    public DrawerPage()
    {
        InitializeComponent();
    }

    private void ShowDrawer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        CloseAll();

        switch (btn.Tag?.ToString())
        {
            case "Left":
                LeftDrawer.IsOpen = true;
                break;
            case "Top":
                TopDrawer.IsOpen = true;
                break;
            case "Right":
                RightDrawer.IsOpen = true;
                break;
            case "Bottom":
                BottomDrawer.IsOpen = true;
                break;
        }
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
