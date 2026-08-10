using System.Windows;
using System.Windows.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;
using Button = Wpf.Ui.Controls.Button;
using ControlAppearance = Wpf.Ui.Controls.ControlAppearance;

namespace Wpf.Ui.Violeta.Gallery.Pages.Windows;

public partial class ContentWindowPage : Wpf.Ui.Violeta.Controls.Page
{
    public ContentWindowPage()
    {
        InitializeComponent();
    }

    private void OpenContentWindow_Click(object sender, RoutedEventArgs e)
    {
        var dialog = ContentWindow.Create<DemoContentWindowControl>();
        dialog.Owner = Window.GetWindow(this);
        dialog.Width = 460;
        dialog.Height = 280;
        dialog.CanKeyDownResult = true;
        dialog.MinimizeButtonVisibility = Visibility.Visible;
        dialog.MaximizeButtonVisibility = Visibility.Visible;
        dialog.ShowDialog();
    }

    private sealed class DemoContentWindowControl : ContentWindowControl
    {
        public DemoContentWindowControl()
        {
            Title = "ContentWindow Demo";

            var message = new TextBlock
            {
                Text = LangKeys.Sample_6d80adabe3.Tr(),
                TextWrapping = TextWrapping.Wrap,
            };

            var cancel = new Button
            {
                Content = LangKeys.Sample_625fb26b4b.Tr(),
                Appearance = ControlAppearance.Secondary,
                Margin = new Thickness(0, 0, 8, 0),
                MinWidth = 80,
            };
            cancel.Click += (_, _) => Owner?.OnResultCommandExecuted(ContentWindowResult.Cancel);

            var ok = new Button
            {
                Content = LangKeys.Sample_12cd9a94dc.Tr(),
                Appearance = ControlAppearance.Primary,
                MinWidth = 80,
            };
            ok.Click += (_, _) => Owner?.OnResultCommandExecuted(ContentWindowResult.OK);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0),
                Children = { cancel, ok },
            };

            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Children = { message, buttons },
            };
        }
    }
}
