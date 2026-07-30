using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.BasicInput;

public partial class RatingPage : Wpf.Ui.Violeta.Controls.Page
{
    public RatingPage()
    {
        InitializeComponent();
    }

    private void DisableFirstRating_Checked(object sender, RoutedEventArgs e) => FirstRating.IsEnabled = false;

    private void DisableFirstRating_Unchecked(object sender, RoutedEventArgs e) => FirstRating.IsEnabled = true;

    private void DisableSecondRating_Checked(object sender, RoutedEventArgs e) => SecondRating.IsEnabled = false;

    private void DisableSecondRating_Unchecked(object sender, RoutedEventArgs e) => SecondRating.IsEnabled = true;
}
