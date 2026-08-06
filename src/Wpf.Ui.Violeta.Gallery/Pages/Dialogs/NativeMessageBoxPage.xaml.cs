using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

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
                LangKeys.Sample_cab56120f1.Tr(),
                LangKeys.Sample_5bd8feb5d7.Tr(),
                NativeMessageBoxButton.OK,
                NativeMessageBoxImage.Information),
            "YesNo" => NativeMessageBox.Show(
                owner,
                LangKeys.Sample_af204660c7.Tr(),
                LangKeys.Sample_3a4efdbeba.Tr(),
                NativeMessageBoxButton.YesNo,
                NativeMessageBoxImage.Question,
                NativeMessageBoxResult.Yes),
            "OKCancel" => NativeMessageBox.Show(
                owner,
                LangKeys.Sample_NativeAttentionBody.Tr(),
                LangKeys.Sample_6f0a3ce47c.Tr(),
                NativeMessageBoxButton.OKCancel,
                NativeMessageBoxImage.Warning,
                NativeMessageBoxResult.OK),
            "YesNoCancel" => NativeMessageBox.Show(
                owner,
                LangKeys.Sample_0f5bb4f6e5.Tr(),
                LangKeys.Sample_dc94b4096c.Tr(),
                NativeMessageBoxButton.YesNoCancel,
                NativeMessageBoxImage.Question,
                NativeMessageBoxResult.Cancel),
            _ => NativeMessageBoxResult.None,
        };

        NativeMessageBoxResultText.Text = LangKeys.Format_Result.Tr(result);
    }
}
