using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class FlyoutPage : Wpf.Ui.Violeta.Controls.Page
{
    public FlyoutPage()
    {
        InitializeComponent();
    }

    private void ShowFlyoutInline_Click(object sender, RoutedEventArgs e)
    {
        Toast.Success("The cake is a lie!");
    }
}
