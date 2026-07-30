using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using ContentDialog = Wpf.Ui.Violeta.Controls.ContentDialog;
using ContentDialogButton = Wpf.Ui.Violeta.Controls.ContentDialogButton;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class ContentDialogPage : Wpf.Ui.Violeta.Controls.Page
{
    public ContentDialogPage()
    {
        InitializeComponent();
    }

    private async void ShowContentDialog_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new()
        {
            Title = "示例 ContentDialog",
            Content = "这是 Violeta 提供的 ContentDialog，支持主按钮、次按钮和关闭按钮。",
            CloseButtonText = "关闭",
            PrimaryButtonText = "确认",
            SecondaryButtonText = "稍后",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();
        ContentDialogResultText.Text = $"结果：{result}";
    }

    private async void ShowContentDialogCustom_Click(object sender, RoutedEventArgs e)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        stack.Children.Add(new TextBlock { Text = "请输入您的名称：", Margin = new Thickness(0, 0, 0, 8) });
        stack.Children.Add(new Wpf.Ui.Controls.TextBox { PlaceholderText = "名称...", MinWidth = 200 });

        ContentDialog dialog = new()
        {
            Title = "自定义内容",
            Content = stack,
            CloseButtonText = "取消",
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();
        ContentDialogResultText.Text = $"结果：{result}";
    }
}
