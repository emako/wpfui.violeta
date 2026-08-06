using System.Windows;
using System.Windows.Controls;
using MessageBox = Wpf.Ui.Violeta.Controls.MessageBox;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

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
            "Information" => MessageBox.Information(LangKeys.Sample_bc0a0553b3.Tr()),
            "Warning" => MessageBox.Warning(LangKeys.Sample_17395556d2.Tr()),
            "Question" => MessageBox.Question(LangKeys.Sample_8a3cbdcbba.Tr()),
            "Error" => MessageBox.Error(LangKeys.Sample_2e087c1ba6.Tr()),
            _ => System.Windows.MessageBoxResult.None,
        };

        MessageBoxResultText.Text = LangKeys.Format_Result.Tr(result);
    }
}
