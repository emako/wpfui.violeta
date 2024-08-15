using System.Linq;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public static class Toast
{
    public static void Information(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Information, location, offsetMargin, time));

    public static void Information(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Information, location, offsetMargin, time));

    public static void Success(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Success, location, offsetMargin, time));

    public static void Success(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Success, location, offsetMargin, time));

    public static void Error(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Error, location, offsetMargin, time));

    public static void Error(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Error, location, offsetMargin, time));

    public static void Warning(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Warning, location, offsetMargin, time));

    public static void Warning(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Warning, location, offsetMargin, time));

    public static void Question(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Question, location, offsetMargin, time));

    public static void Question(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Question, location, offsetMargin, time));

    public static void Show(FrameworkElement owner, string message, ToastConfig? options = null)
    {
        ToastControl toast = new(
            owner ?? Application.Current.Windows.OfType<Window>()
                .Where(win => win.IsActive)
                .FirstOrDefault()!,
            message, options);
        toast.ShowCore();
    }
}
