using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class HyperlinkButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public HyperlinkButtonPage()
    {
        InitializeComponent();
    }

    private void DisableHyperlink_Checked(object sender, RoutedEventArgs e) => Hyperlink.IsEnabled = false;

    private void DisableHyperlink_Unchecked(object sender, RoutedEventArgs e) => Hyperlink.IsEnabled = true;
}
