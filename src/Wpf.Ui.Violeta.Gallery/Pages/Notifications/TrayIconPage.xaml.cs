using System.Windows;
using Wpf.Ui.Violeta.Win32;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class TrayIconPage : Wpf.Ui.Violeta.Controls.Page
{
    private bool _syncingTwink;

    public TrayIconPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SyncTwinkToggle();
    }

    private void SyncTwinkToggle()
    {
        _syncingTwink = true;
        try
        {
            TwinkToggle.IsChecked = TrayIconManager.IsTwink;
        }
        finally
        {
            _syncingTwink = false;
        }
    }

    private void TwinkToggle_OnChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _syncingTwink)
            return;

        TrayIconManager.IsTwink = TwinkToggle.IsChecked == true;
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
