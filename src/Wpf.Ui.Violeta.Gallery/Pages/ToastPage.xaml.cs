using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages;

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
                Toast.Show(null!, $"这是一条信息通知（{location}）", CreateStackedConfig(ToastIcon.Information, location));
                break;

            case "Success":
                Toast.Show(null!, $"操作成功（{location}）", CreateStackedConfig(ToastIcon.Success, location));
                break;

            case "Error":
                Toast.Show(null!, $"发生错误（{location}）", CreateStackedConfig(ToastIcon.Error, location));
                break;

            case "Warning":
                Toast.Show(null!, $"请注意（{location}）", CreateStackedConfig(ToastIcon.Warning, location));
                break;

            case "Question":
                Toast.Show(null!, $"是否继续操作？（{location}）", CreateStackedConfig(ToastIcon.Question, location));
                break;

            default:
                Toast.Show(null!, $"这是一条默认通知（{location}）", CreateStackedConfig(ToastIcon.None, location));
                break;
        }
    }

    private static ToastConfig CreateStackedConfig(ToastIcon icon, ToastLocation location)
        => new(icon, location, default, ToastConfig.NormalTime) { IsStacked = true };

    private void ShowStacked_Click(object sender, RoutedEventArgs e)
    {
        Toast.IsStacked = true;

        Toast.Information("第 1 条通知");
        Toast.Warning("第 2 条通知");
        Toast.Error("第 3 条通知");
        Toast.Success("第 4 条通知");
        Toast.Question("第 5 条通知");
    }

    private void ShowNonStacked_Click(object sender, RoutedEventArgs e)
    {
        Toast.Show(null!, "非堆叠通知 1", new ToastConfig(ToastIcon.Information, ToastLocation.TopCenter, default, ToastConfig.NormalTime) { IsStacked = false });
        Toast.Show(null!, "非堆叠通知 2", new ToastConfig(ToastIcon.Warning, ToastLocation.TopCenter, default, ToastConfig.NormalTime) { IsStacked = false });
        Toast.Show(null!, "非堆叠通知 3", new ToastConfig(ToastIcon.Error, ToastLocation.TopCenter, default, ToastConfig.NormalTime) { IsStacked = false });
    }

    private void ShowLimitedStack_Click(object sender, RoutedEventArgs e)
    {
        var originalMax = ToastConfig.MaxStacked;
        ToastConfig.MaxStacked = 2;
        Toast.IsStacked = true;

        Toast.Information("限制堆叠 1");
        Toast.Warning("限制堆叠 2");
        Toast.Error("限制堆叠 3（将覆盖）");
        Toast.Success("限制堆叠 4（将覆盖）");

        Task.Delay(ToastConfig.SlowTime * 4).ContinueWith(_ => ToastConfig.MaxStacked = originalMax);
    }
}
