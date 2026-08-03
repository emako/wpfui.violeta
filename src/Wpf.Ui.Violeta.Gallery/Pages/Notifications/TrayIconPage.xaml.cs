using System.Windows;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class TrayIconPage : Wpf.Ui.Violeta.Controls.Page
{
    public TrayIconPage()
    {
        InitializeComponent();
    }

    private void ShowTrayBalloon_Click(object sender, RoutedEventArgs e)
    {
        TrayIconManager.ShowNotification(
            "Wpf.Ui.Violeta Gallery",
            "这是来自 TrayIconHost 的真实托盘气球通知。",
            ToolTipIcon.Info);
    }

    private void HideToTray_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is { } window)
        {
            window.Hide();
            TrayIconManager.ShowNotification(
                "Wpf.Ui.Violeta Gallery",
                "主窗口已隐藏。双击托盘图标可重新打开。",
                ToolTipIcon.Info);
        }
    }

    private void ShowFromTray_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is { } window)
        {
            window.Show();
            window.Activate();
        }
    }
}
