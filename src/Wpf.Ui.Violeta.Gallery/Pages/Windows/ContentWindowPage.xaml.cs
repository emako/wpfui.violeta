using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

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
                        Text = LangKeys.Sample_6d80adabe3.Tr(),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 16),
                    },
                    new Button
                    {
                        Content = LangKeys.Sample_12cd9a94dc.Tr(),
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
