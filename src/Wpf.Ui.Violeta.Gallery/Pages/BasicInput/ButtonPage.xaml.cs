using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class ButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public ButtonPage()
    {
        InitializeComponent();
    }

    private void DisableButton_Checked(object sender, RoutedEventArgs e) => StandardButton.IsEnabled = false;

    private void DisableButton_Unchecked(object sender, RoutedEventArgs e) => StandardButton.IsEnabled = true;

    private void DisableUiButton_Checked(object sender, RoutedEventArgs e) => UiButton.IsEnabled = false;

    private void DisableUiButton_Unchecked(object sender, RoutedEventArgs e) => UiButton.IsEnabled = true;
}
