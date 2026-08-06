using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

namespace Wpf.Ui.Violeta.Gallery.Pages.Notifications;

public partial class ToastPage : Wpf.Ui.Violeta.Controls.Page
{
    public ToastPage()
    {
        InitializeComponent();
    }

    private void ShowToast_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        var parts = btn.Tag?.ToString()?.Split('|') ?? [];
        if (parts.Length != 2)
        {
            return;
        }

        var icon = parts[0];
        var location = Enum.TryParse<ToastLocation>(parts[1], out var loc) ? loc : ToastLocation.TopCenter;

        switch (icon)
        {
            case "Information":
                Toast.Show(null!, LangKeys.Format_ToastInfo.Tr(location), CreateStackedConfig(ToastIcon.Information, location));
                break;

            case "Success":
                Toast.Show(null!, LangKeys.Format_ToastSuccess.Tr(location), CreateStackedConfig(ToastIcon.Success, location));
                break;

            case "Error":
                Toast.Show(null!, LangKeys.Format_ToastError.Tr(location), CreateStackedConfig(ToastIcon.Error, location));
                break;

            case "Warning":
                Toast.Show(null!, LangKeys.Format_ToastWarning.Tr(location), CreateStackedConfig(ToastIcon.Warning, location));
                break;

            case "Question":
                Toast.Show(null!, LangKeys.Format_ToastQuestion.Tr(location), CreateStackedConfig(ToastIcon.Question, location));
                break;

            default:
                Toast.Show(null!, LangKeys.Format_ToastDefault.Tr(location), CreateStackedConfig(ToastIcon.None, location));
                break;
        }
    }

    private static ToastConfig CreateStackedConfig(ToastIcon icon, ToastLocation location)
        => new(icon, location, default, ToastConfig.NormalTime) { IsStacked = true };

    private void ShowStacked_Click(object sender, RoutedEventArgs e)
    {
        Toast.IsStacked = true;

        Toast.Information(LangKeys.Sample_5239b15bba.Tr());
        Toast.Warning(LangKeys.Sample_0ea7e63346.Tr());
        Toast.Error(LangKeys.Sample_37922953b5.Tr());
        Toast.Success(LangKeys.Sample_e1fc97da7b.Tr());
        Toast.Question(LangKeys.Sample_d47ea33b91.Tr());
    }

    private void ShowNonStacked_Click(object sender, RoutedEventArgs e)
    {
        Toast.Show(null!, LangKeys.Sample_0288ddbb33.Tr(), new ToastConfig(ToastIcon.Information, ToastLocation.TopCenter, default, ToastConfig.NormalTime) { IsStacked = false });
        Toast.Show(null!, LangKeys.Sample_e16428ce2a.Tr(), new ToastConfig(ToastIcon.Warning, ToastLocation.TopCenter, default, ToastConfig.NormalTime) { IsStacked = false });
        Toast.Show(null!, LangKeys.Sample_9983a4a85f.Tr(), new ToastConfig(ToastIcon.Error, ToastLocation.TopCenter, default, ToastConfig.NormalTime) { IsStacked = false });
    }

    private void ShowLimitedStack_Click(object sender, RoutedEventArgs e)
    {
        var originalMax = ToastConfig.MaxStacked;
        ToastConfig.MaxStacked = 2;
        Toast.IsStacked = true;

        Toast.Information(LangKeys.Sample_5bc98065dc.Tr());
        Toast.Warning(LangKeys.Sample_5453af82cf.Tr());
        Toast.Error(LangKeys.Sample_ea0a332c59.Tr());
        Toast.Success(LangKeys.Sample_89bfb00829.Tr());

        Task.Delay(ToastConfig.SlowTime * 4).ContinueWith(_ => ToastConfig.MaxStacked = originalMax);
    }
}
