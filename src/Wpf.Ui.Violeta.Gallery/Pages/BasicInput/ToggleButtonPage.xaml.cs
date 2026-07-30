using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class ToggleButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public ToggleButtonPage()
    {
        InitializeComponent();
    }

    private void DisableToggleButton_Checked(object sender, RoutedEventArgs e) =>
        ToggleButtonControl.IsEnabled = false;

    private void DisableToggleButton_Unchecked(object sender, RoutedEventArgs e) =>
        ToggleButtonControl.IsEnabled = true;
}
