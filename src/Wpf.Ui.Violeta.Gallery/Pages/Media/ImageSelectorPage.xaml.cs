using System.Windows;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Media;

public partial class ImageSelectorPage
{
    public ImageSelectorPage()
    {
        InitializeComponent();
    }

    private void OnImageSelected(object sender, RoutedEventArgs e)
    {
        if (sender is ImageSelector { Uri: { } uri })
        {
            StatusText.Text = LangKeys.Format_Selected.Tr(uri.LocalPath);
        }
    }

    private void OnImageUnselected(object sender, RoutedEventArgs e)
    {
        StatusText.Text = LangKeys.Sample_ImageSelector_StatusCleared.Tr();
    }

    private void OnDemoImageSelected(object sender, RoutedEventArgs e) => OnImageSelected(sender, e);

    private void OnDemoImageUnselected(object sender, RoutedEventArgs e) => OnImageUnselected(sender, e);
}
