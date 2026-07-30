using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class TrayIconPage : Wpf.Ui.Violeta.Controls.Page
{
    public TrayIconPage()
    {
        InitializeComponent();
    }

    private void SimulateTrayBalloon_Click(object sender, RoutedEventArgs e)
    {
        Toast.Information("托盘气球：应用已在后台运行");
    }

    private void SimulateTrayMenu_Click(object sender, RoutedEventArgs e)
    {
        Toast.Success("托盘菜单：已执行「显示主窗口」");
    }
}
