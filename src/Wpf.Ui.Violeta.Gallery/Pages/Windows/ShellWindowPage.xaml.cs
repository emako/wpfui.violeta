using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Windows;

public partial class ShellWindowPage : Wpf.Ui.Violeta.Controls.Page
{
    public ShellWindowPage()
    {
        InitializeComponent();
    }

    private void OpenShellWindow_Click(object sender, RoutedEventArgs e)
    {
        var window = new ShellWindow
        {
            Title = "ShellWindow Demo",
            Width = 480,
            Height = 320,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            Content = new TextBlock
            {
                Text = "这是一个简单的 ShellWindow 演示窗口。",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
            },
        };
        window.Show();
    }
}
