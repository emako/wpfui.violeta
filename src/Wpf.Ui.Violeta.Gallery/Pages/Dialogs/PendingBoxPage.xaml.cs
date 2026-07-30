using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class PendingBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public PendingBoxPage()
    {
        InitializeComponent();
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
