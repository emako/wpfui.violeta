using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using ContentDialog = Wpf.Ui.Violeta.Controls.ContentDialog;
using ContentDialogButton = Wpf.Ui.Violeta.Controls.ContentDialogButton;
using MessageBox = Wpf.Ui.Violeta.Controls.MessageBox;
using Page = Wpf.Ui.Violeta.Controls.Compat.Page;

namespace Wpf.Ui.Violeta.Gallery.Pages;

public partial class DialogPage : Page
{
    public DialogPage()
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

    private void ShowMessageBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        var tag = btn.Tag?.ToString();
        System.Windows.MessageBoxResult result;

        result = tag switch
        {
            "Information" => MessageBox.Information("这是一条信息消息。"),
            "Warning" => MessageBox.Warning("请注意，这是一条警告。"),
            "Question" => MessageBox.Question("这是一个问题，您确认吗？"),
            "Error" => MessageBox.Error("发生了一个错误，请稍后重试。"),
            _ => System.Windows.MessageBoxResult.None,
        };

        MessageBoxResultText.Text = $"结果：{result}";
    }

    private async void ShowPendingBox_Click(object sender, RoutedEventArgs e)
    {
        using IPendingHandler pending = PendingBox.Show("正在加载数据...", "请稍候");
        await Task.Delay(3000);
    }

    private async void ShowPendingBoxWithCancel_Click(object sender, RoutedEventArgs e)
    {
        using IPendingHandler pending = PendingBox.Show("正在执行操作，请稍候...", "处理中", isShowCancel: true);
        await Task.Delay(3000);
    }
}
