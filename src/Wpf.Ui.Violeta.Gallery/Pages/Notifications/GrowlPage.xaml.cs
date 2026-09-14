using System.Windows;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class GrowlPage : Wpf.Ui.Violeta.Controls.Page
{
    private const string DemoToken = "GrowlDemoToken";

    public GrowlPage()
    {
        InitializeComponent();
    }

    private void Info_Click(object sender, RoutedEventArgs e) => Growl.Info(LangKeys.Sample_Growl_MsgInfo.Tr());
    private void Success_Click(object sender, RoutedEventArgs e) => Growl.Success(LangKeys.Sample_Growl_MsgSuccess.Tr());
    private void Warning_Click(object sender, RoutedEventArgs e) => Growl.Warning(LangKeys.Sample_Growl_MsgWarning.Tr());
    private void Error_Click(object sender, RoutedEventArgs e) => Growl.Error(LangKeys.Sample_Growl_MsgError.Tr());
    private void Fatal_Click(object sender, RoutedEventArgs e) => Growl.Fatal(new GrowlInfo { Message = LangKeys.Sample_Growl_MsgFatal.Tr(), ShowDateTime = false });
    private void Ask_Click(object sender, RoutedEventArgs e)
        => Growl.Ask(LangKeys.Sample_Growl_MsgAsk.Tr(), isConfirmed =>
        {
            Growl.Info(isConfirmed ? LangKeys.Sample_581eca24d1.Tr() : LangKeys.Sample_692fdb35e2.Tr());
            return true;
        });
    private void Clear_Click(object sender, RoutedEventArgs e) => Growl.Clear();

    private void InfoGlobal_Click(object sender, RoutedEventArgs e) => Growl.InfoGlobal(LangKeys.Sample_Growl_MsgInfoGlobal.Tr());
    private void SuccessGlobal_Click(object sender, RoutedEventArgs e) => Growl.SuccessGlobal(LangKeys.Sample_Growl_MsgSuccessGlobal.Tr());
    private void WarningGlobal_Click(object sender, RoutedEventArgs e) => Growl.WarningGlobal(LangKeys.Sample_Growl_MsgWarningGlobal.Tr());
    private void ErrorGlobal_Click(object sender, RoutedEventArgs e) => Growl.ErrorGlobal(LangKeys.Sample_Growl_MsgErrorGlobal.Tr());
    private void FatalGlobal_Click(object sender, RoutedEventArgs e) => Growl.FatalGlobal(new GrowlInfo { Message = LangKeys.Sample_Growl_MsgFatalGlobal.Tr(), ShowDateTime = false });
    private void AskGlobal_Click(object sender, RoutedEventArgs e)
        => Growl.AskGlobal(LangKeys.Sample_Growl_MsgAskGlobal.Tr(), isConfirmed =>
        {
            Growl.InfoGlobal(isConfirmed ? LangKeys.Sample_581eca24d1.Tr() : LangKeys.Sample_692fdb35e2.Tr());
            return true;
        });
    private void ClearGlobal_Click(object sender, RoutedEventArgs e) => Growl.ClearGlobal();

    private void TokenInfo_Click(object sender, RoutedEventArgs e) => Growl.Info(LangKeys.Sample_Growl_MsgTokenInfo.Tr(), DemoToken);
    private void TokenSuccess_Click(object sender, RoutedEventArgs e) => Growl.Success(LangKeys.Sample_Growl_MsgTokenSuccess.Tr(), DemoToken);
    private void TokenClear_Click(object sender, RoutedEventArgs e) => Growl.Clear(DemoToken);
}
