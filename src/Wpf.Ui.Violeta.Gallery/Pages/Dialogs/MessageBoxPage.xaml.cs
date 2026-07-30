using System.Windows;
using System.Windows.Controls;
using MessageBox = Wpf.Ui.Violeta.Controls.MessageBox;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class MessageBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public MessageBoxPage()
    {
        InitializeComponent();
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
}
