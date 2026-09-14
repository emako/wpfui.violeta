using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Media;

public partial class AnimationPathPage : Page
{
    public AnimationPathPage()
    {
        InitializeComponent();
    }

    private void OnTogglePlayingClick(object sender, System.Windows.RoutedEventArgs e)
    {
        DemoPath.IsPlaying = !DemoPath.IsPlaying;
    }
}
