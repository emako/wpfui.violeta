using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class GoBackButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public GoBackButtonPage()
    {
        InitializeComponent();
    }

    private void GoBackButton_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Clicked — press animation matches TitleBar back button.";
    }
}
