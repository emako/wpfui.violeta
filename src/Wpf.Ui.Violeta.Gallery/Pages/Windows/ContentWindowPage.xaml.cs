using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Windows;

public partial class ContentWindowPage : Wpf.Ui.Violeta.Controls.Page
{
    public ContentWindowPage()
    {
        InitializeComponent();
    }

    private void OpenContentWindow_Click(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        var dialog = ContentWindow.Create<DemoContentWindowControl>();
        dialog.Owner = owner;
        dialog.Width = 420;
        dialog.Height = 240;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialog.ShowDialog();
    }

    private sealed class DemoContentWindowControl : ContentWindowControl
    {
        public DemoContentWindowControl()
        {
            Title = "ContentWindow Demo";
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Children =
                {
                    new TextBlock
                    {
                        Text = "这是 ContentWindow + ContentWindowControl 的简单演示。",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 16),
                    },
                    new Button
                    {
                        Content = "确定并关闭",
                        HorizontalAlignment = HorizontalAlignment.Right,
                    },
                },
            };

            if (Content is StackPanel panel && panel.Children[1] is Button ok)
            {
                ok.Click += (_, _) => Owner?.OnResultCommandExecuted(ContentWindowResult.OK);
            }
        }
    }
}
