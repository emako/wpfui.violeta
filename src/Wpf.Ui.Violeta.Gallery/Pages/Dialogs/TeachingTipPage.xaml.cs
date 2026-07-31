using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class TeachingTipPage : Wpf.Ui.Violeta.Controls.Page
{
    public TeachingTipPage()
    {
        InitializeComponent();
    }

    private void ShowTargetedTip_Click(object sender, RoutedEventArgs e)
    {
        TargetedTeachingTip.IsOpen = true;
    }

    private void ShowUntargetedTip_Click(object sender, RoutedEventArgs e)
    {
        UntargetedTeachingTip.IsOpen = true;
    }

    private void ShowHeroTip_Click(object sender, RoutedEventArgs e)
    {
        HeroTeachingTip.IsOpen = true;
    }
}
