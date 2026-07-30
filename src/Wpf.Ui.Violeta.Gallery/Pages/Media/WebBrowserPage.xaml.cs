using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.Media;

public partial class WebBrowserPage : Wpf.Ui.Violeta.Controls.Page
{
    public WebBrowserPage()
    {
        InitializeComponent();
    }

    private void WebBrowserPage_Loaded(object sender, RoutedEventArgs e)
    {
        DemoWebBrowser.Navigate(new Uri("https://example.com"));
    }
}
