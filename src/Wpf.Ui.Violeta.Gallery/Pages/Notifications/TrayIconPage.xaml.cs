using System.Windows;
using Wpf.Ui.Violeta.Win32;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

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
            LangKeys.Sample_9dba3a086c.Tr(),
            ToolTipIcon.Info);
    }

    private void HideToTray_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is { } window)
        {
            window.Hide();
            TrayIconManager.ShowNotification(
                "Wpf.Ui.Violeta Gallery",
                LangKeys.Sample_3676039437.Tr(),
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
