using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class PendingBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public PendingBoxPage()
    {
        InitializeComponent();
    }

    private async void ShowPendingBox_Click(object sender, RoutedEventArgs e)
    {
        using IPendingHandler pending = PendingBox.Show(LangKeys.Sample_0ac491da7b.Tr(), LangKeys.Sample_7c1efe79cc.Tr());
        await Task.Delay(3000);
    }

    private async void ShowPendingBoxWithCancel_Click(object sender, RoutedEventArgs e)
    {
        using IPendingHandler pending = PendingBox.Show(LangKeys.Sample_dd3316267a.Tr(), LangKeys.Sample_5d459d550a.Tr(), isShowCancel: true);
        await Task.Delay(3000);
    }
}
