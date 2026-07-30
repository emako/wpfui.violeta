using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Feedback;

public partial class SplashPage : Wpf.Ui.Violeta.Controls.Page
{
    public SplashPage()
    {
        InitializeComponent();
    }

    private void ShowSplash_Click(object sender, RoutedEventArgs e)
    {
        Splash.ShowAsync(
            "pack://application:,,,/Wpf.Ui.Violeta.Gallery;component/Resources/Images/wpfui.png",
            0.98d,
            actived: splash =>
            {
                splash.Hint = new TextBlock
                {
                    Text = "Gallery Splash demo...",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.White,
                    FontSize = 14d,
                };
            });
    }

    private void CloseSplash_Click(object sender, RoutedEventArgs e)
    {
        Splash.CloseAsync(forced: true);
    }
}
