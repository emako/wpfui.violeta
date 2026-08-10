using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class NotificationPage : Wpf.Ui.Violeta.Controls.Page
{
    public NotificationPage()
    {
        InitializeComponent();
    }

    private void ShowInformation_Click(object sender, RoutedEventArgs e)
        => Notification.Information("Wpf.Ui.Violeta", "This is an information balloon tip.");

    private void ShowWarning_Click(object sender, RoutedEventArgs e)
        => Notification.Warning("Wpf.Ui.Violeta", "This is a warning balloon tip.");

    private void ShowError_Click(object sender, RoutedEventArgs e)
        => Notification.Error("Wpf.Ui.Violeta", "This is an error balloon tip.");

    private void ShowNone_Click(object sender, RoutedEventArgs e)
        => Notification.Show("Wpf.Ui.Violeta", "This is a balloon tip without an icon.");
}
