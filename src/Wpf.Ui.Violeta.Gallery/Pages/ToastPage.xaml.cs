using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Controls;
using Page = Wpf.Ui.Violeta.Controls.Compat.Page;

namespace Wpf.Ui.Violeta.Gallery.Pages;

public partial class ToastPage : Page
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

        var originalIsStacked = ToastConfig.IsStacked;
        ToastConfig.IsStacked = true;

        switch (icon)
        {
            case "Information":
                Toast.Information($"这是一条信息通知（{location}）", location);
                break;

            case "Success":
                Toast.Success($"操作成功（{location}）", location);
                break;

            case "Error":
                Toast.Error($"发生错误（{location}）", location);
                break;

            case "Warning":
                Toast.Warning($"请注意（{location}）", location);
                break;

            case "Question":
                Toast.Question($"是否继续操作？（{location}）", location);
                break;

            default:
                Toast.Show(null!, $"这是一条默认通知（{location}）", new ToastConfig { Location = location });
                break;
        }

        Task.Delay(100).ContinueWith(_ => ToastConfig.IsStacked = originalIsStacked);
    }

    private void ShowStacked_Click(object sender, RoutedEventArgs e)
    {
        var originalIsStacked = ToastConfig.IsStacked;
        ToastConfig.IsStacked = true;

        Toast.Information("第 1 条通知");
        Toast.Warning("第 2 条通知");
        Toast.Error("第 3 条通知");
        Toast.Success("第 4 条通知");
        Toast.Question("第 5 条通知");

        Task.Delay(100).ContinueWith(_ => ToastConfig.IsStacked = originalIsStacked);
    }

    private void ShowNonStacked_Click(object sender, RoutedEventArgs e)
    {
        var originalIsStacked = ToastConfig.IsStacked;
        ToastConfig.IsStacked = false;

        Toast.Information("非堆叠通知 1");
        Toast.Warning("非堆叠通知 2");
        Toast.Error("非堆叠通知 3");

        Task.Delay(100).ContinueWith(_ => ToastConfig.IsStacked = originalIsStacked);
    }

    private void ShowLimitedStack_Click(object sender, RoutedEventArgs e)
    {
        var originalMax = ToastConfig.MaxStacked;
        ToastConfig.MaxStacked = 2;

        Toast.Information("限制堆叠 1");
        Toast.Warning("限制堆叠 2");
        Toast.Error("限制堆叠 3（将覆盖）");
        Toast.Success("限制堆叠 4（将覆盖）");

        Task.Delay(100).ContinueWith(_ => ToastConfig.MaxStacked = originalMax);
    }
}
