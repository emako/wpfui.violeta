using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class RadioButtonPage : Wpf.Ui.Violeta.Controls.Page
{
    public RadioButtonPage()
    {
        InitializeComponent();
    }

    private void DisableRadioButtons_Checked(object sender, RoutedEventArgs e) => RadioGroup.IsEnabled = false;

    private void DisableRadioButtons_Unchecked(object sender, RoutedEventArgs e) => RadioGroup.IsEnabled = true;
}
