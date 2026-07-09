using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

public static class Toast
{
    private static readonly Dictionary<Window, List<ToastControl>> _activeToasts = [];

    /// <summary>
    /// Process-wide default for <see cref="ToastConfig.IsStacked"/> when convenience helpers build a config
    /// or when <see cref="Show(FrameworkElement, string, ToastConfig?)"/> is called without options.
    /// </summary>
    /// <remarks>
    /// Snapshotted onto each <see cref="ToastControl"/> at creation time; changing this later does not
    /// affect toasts that are already visible.
    /// </remarks>
    public static bool IsStacked { get; set; }

    internal static void RegisterToast(Window window, ToastControl toast)
    {
        if (!_activeToasts.TryGetValue(window, out List<ToastControl>? value))
        {
            value = [];
            _activeToasts[window] = value;
        }

        value.Add(toast);
    }

    internal static void UnregisterToast(Window window, ToastControl toast)
    {
        if (!_activeToasts.TryGetValue(window, out var toasts))
        {
            return;
        }

        if (!toast.IsRegistered)
        {
            return;
        }

        toast.IsRegistered = false;

        var location = toast.Location;
        var shouldReposition = toast.IsStacked;

        toasts.Remove(toast);
        if (toasts.Count == 0)
        {
            _activeToasts.Remove(window);
            return;
        }

        if (shouldReposition || toasts.Any(t => t.Location == location && t.IsStacked))
        {
            UpdateToastPositions(window, location);
        }
    }

    internal static List<ToastControl> GetActiveToasts(Window window)
    {
        return _activeToasts.TryGetValue(window, out List<ToastControl>? value) ? value : [];
    }

    private static void UpdateToastPositions(Window window, ToastLocation location)
    {
        if (!_activeToasts.TryGetValue(window, out var toasts))
        {
            return;
        }

        foreach (var toast in toasts.Where(t => t.Location == location))
        {
            window.Dispatcher.BeginInvoke(() => toast.UpdatePosition(forceReopen: true), DispatcherPriority.Render);
        }
    }

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
        options ??= new ToastConfig { IsStacked = IsStacked };

        ToastControl toast = new(
            owner ?? Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(win => win.IsActive)!,
            message, options);
        toast.ShowCore();
    }
}
