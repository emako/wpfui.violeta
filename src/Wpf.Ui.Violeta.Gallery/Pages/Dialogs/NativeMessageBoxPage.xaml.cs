using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class NativeMessageBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public NativeMessageBoxPage()
    {
        InitializeComponent();
    }

    private nint OwnerHandle
    {
        get
        {
            Window? window = Window.GetWindow(this);
            return window is null ? 0 : new WindowInteropHelper(window).Handle;
        }
    }

    private void ShowNativeMessageBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        nint owner = OwnerHandle;
        var tag = btn.Tag?.ToString();

        NativeMessageBoxResult result = tag switch
        {
            "OK" => NativeMessageBox.Show(
                owner,
                "这是一个原生 Win32 MessageBox。",
                "NativeMessageBox — 确定",
                NativeMessageBoxButton.OK,
                NativeMessageBoxImage.Information),
            "YesNo" => NativeMessageBox.Show(
                owner,
                "是否继续？",
                "NativeMessageBox — 是/否",
                NativeMessageBoxButton.YesNo,
                NativeMessageBoxImage.Question,
                NativeMessageBoxResult.Yes),
            "OKCancel" => NativeMessageBox.Show(
                owner,
                "可能需要您的注意。\n点击确定继续。",
                "NativeMessageBox — 确定/取消",
                NativeMessageBoxButton.OKCancel,
                NativeMessageBoxImage.Warning,
                NativeMessageBoxResult.OK),
            "YesNoCancel" => NativeMessageBox.Show(
                owner,
                "关闭前是否保存更改？",
                "NativeMessageBox — 是/否/取消",
                NativeMessageBoxButton.YesNoCancel,
                NativeMessageBoxImage.Question,
                NativeMessageBoxResult.Cancel),
            _ => NativeMessageBoxResult.None,
        };

        NativeMessageBoxResultText.Text = $"结果：{result}";
    }
}
