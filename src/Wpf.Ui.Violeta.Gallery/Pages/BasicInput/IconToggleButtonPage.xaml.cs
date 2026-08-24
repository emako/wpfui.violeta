using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class IconToggleButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public IconToggleButtonPage()
    {
        InitializeComponent();
    }

    private void DisableIconToggle_Checked(object sender, RoutedEventArgs e)
    {
        DemoIconToggle.IsEnabled = false;
        DemoSampleIconToggle.IsEnabled = false;
    }

    private void DisableIconToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        DemoIconToggle.IsEnabled = true;
        DemoSampleIconToggle.IsEnabled = true;
    }
}
