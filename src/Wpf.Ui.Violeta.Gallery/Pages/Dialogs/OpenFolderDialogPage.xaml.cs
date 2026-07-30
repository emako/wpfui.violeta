using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class OpenFolderDialogPage : Wpf.Ui.Violeta.Controls.Page
{
    public OpenFolderDialogPage()
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

    private void ShowOpenFolderDialog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        bool multiselect = button.Tag?.ToString() == "Multiple";
        OpenFolderDialog dialog = new()
        {
            Description = multiselect ? "请选择一个或多个文件夹。" : "请选择一个文件夹。",
            UseDescriptionForTitle = true,
            Multiselect = multiselect,
        };

        bool? result = dialog.ShowDialog(OwnerHandle);
        OpenFolderDialogResultText.Text = result == true
            ? multiselect
                ? $"结果：已选择 {dialog.SelectedPaths.Length} 个文件夹 — {string.Join("；", dialog.SelectedPaths)}"
                : $"结果：{dialog.SelectedPath}"
            : "结果：已取消";
    }
}
