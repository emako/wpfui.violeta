using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class GrowlPage : Wpf.Ui.Violeta.Controls.Page
{
    private const string DemoToken = "GrowlDemoToken";

    public GrowlPage()
    {
        InitializeComponent();
    }

    private void Info_Click(object sender, RoutedEventArgs e) => Growl.Info("This is an info message.");
    private void Success_Click(object sender, RoutedEventArgs e) => Growl.Success("Operation completed successfully.");
    private void Warning_Click(object sender, RoutedEventArgs e) => Growl.Warning("Please check your input.");
    private void Error_Click(object sender, RoutedEventArgs e) => Growl.Error("Something went wrong.");
    private void Fatal_Click(object sender, RoutedEventArgs e) => Growl.Fatal(new GrowlInfo { Message = "A fatal error occurred.", ShowDateTime = false });
    private void Ask_Click(object sender, RoutedEventArgs e)
        => Growl.Ask("Do you want to continue?", isConfirmed =>
        {
            Growl.Info(isConfirmed ? "Confirmed." : "Cancelled.");
            return true;
        });
    private void Clear_Click(object sender, RoutedEventArgs e) => Growl.Clear();

    private void InfoGlobal_Click(object sender, RoutedEventArgs e) => Growl.InfoGlobal("Global info message.");
    private void SuccessGlobal_Click(object sender, RoutedEventArgs e) => Growl.SuccessGlobal("Global success message.");
    private void WarningGlobal_Click(object sender, RoutedEventArgs e) => Growl.WarningGlobal("Global warning message.");
    private void ErrorGlobal_Click(object sender, RoutedEventArgs e) => Growl.ErrorGlobal("Global error message.");
    private void FatalGlobal_Click(object sender, RoutedEventArgs e) => Growl.FatalGlobal(new GrowlInfo { Message = "Global fatal error.", ShowDateTime = false });
    private void AskGlobal_Click(object sender, RoutedEventArgs e)
        => Growl.AskGlobal("Continue on desktop?", isConfirmed =>
        {
            Growl.InfoGlobal(isConfirmed ? "Confirmed." : "Cancelled.");
            return true;
        });
    private void ClearGlobal_Click(object sender, RoutedEventArgs e) => Growl.ClearGlobal();

    private void TokenInfo_Click(object sender, RoutedEventArgs e) => Growl.Info("Token-routed info.", DemoToken);
    private void TokenSuccess_Click(object sender, RoutedEventArgs e) => Growl.Success("Token-routed success.", DemoToken);
    private void TokenClear_Click(object sender, RoutedEventArgs e) => Growl.Clear(DemoToken);
}
