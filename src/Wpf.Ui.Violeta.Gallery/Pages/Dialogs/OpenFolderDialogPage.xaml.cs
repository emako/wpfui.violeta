using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

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
            Title = multiselect ? LangKeys.Sample_509c20e53c.Tr() : LangKeys.Sample_8c19852c92.Tr(),
            Multiselect = multiselect,
        };

        bool? result = dialog.ShowDialog(OwnerHandle);
        OpenFolderDialogResultText.Text = result == true
            ? multiselect
                ? LangKeys.Format_SelectedFolders.Tr(dialog.FolderNames.Length, string.Join("；", dialog.FolderNames))
                : LangKeys.Format_Result.Tr(dialog.FolderName)
            : LangKeys.Sample_b97d90527e.Tr();
    }
}
