using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public static class Toast
{
    private static readonly Dictionary<Window, List<ToastControl>> _activeToasts = new();

    internal static void RegisterToast(Window window, ToastControl toast)
    {
        if (!_activeToasts.ContainsKey(window))
        {
            _activeToasts[window] = new List<ToastControl>();
        }
        _activeToasts[window].Add(toast);
    }

    internal static void UnregisterToast(Window window, ToastControl toast)
    {
        if (_activeToasts.ContainsKey(window))
        {
            _activeToasts[window].Remove(toast);
            if (_activeToasts[window].Count == 0)
            {
                _activeToasts.Remove(window);
            }
            // Update positions of remaining toasts
            UpdateToastPositions(window);
        }
    }

    internal static List<ToastControl> GetActiveToasts(Window window)
    {
        return _activeToasts.ContainsKey(window) ? _activeToasts[window] : new List<ToastControl>();
    }

    private static void UpdateToastPositions(Window window)
    {
        if (_activeToasts.ContainsKey(window))
        {
            foreach (var toast in _activeToasts[window])
            {
                toast.UpdatePosition();
            }
        }
    }
    public static void Information(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Information, location, offsetMargin, time));

    public static void Information(FrameworkElement owner, string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(owner, message, new ToastConfig(ToastIcon.Information, location, offsetMargin, time, isStacked, maxStacked));

    public static void Information(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Information, location, offsetMargin, time));

    public static void Information(string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(null!, message, new ToastConfig(ToastIcon.Information, location, offsetMargin, time, isStacked, maxStacked));

    public static void Success(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Success, location, offsetMargin, time));

    public static void Success(FrameworkElement owner, string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(owner, message, new ToastConfig(ToastIcon.Success, location, offsetMargin, time, isStacked, maxStacked));

    public static void Success(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Success, location, offsetMargin, time));

    public static void Success(string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(null!, message, new ToastConfig(ToastIcon.Success, location, offsetMargin, time, isStacked, maxStacked));

    public static void Error(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Error, location, offsetMargin, time));

    public static void Error(FrameworkElement owner, string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(owner, message, new ToastConfig(ToastIcon.Error, location, offsetMargin, time, isStacked, maxStacked));

    public static void Error(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Error, location, offsetMargin, time));

    public static void Error(string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(null!, message, new ToastConfig(ToastIcon.Error, location, offsetMargin, time, isStacked, maxStacked));

    public static void Warning(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Warning, location, offsetMargin, time));

    public static void Warning(FrameworkElement owner, string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(owner, message, new ToastConfig(ToastIcon.Warning, location, offsetMargin, time, isStacked, maxStacked));

    public static void Warning(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Warning, location, offsetMargin, time));

    public static void Warning(string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(null!, message, new ToastConfig(ToastIcon.Warning, location, offsetMargin, time, isStacked, maxStacked));

    public static void Question(FrameworkElement owner, string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(owner, message, new ToastConfig(ToastIcon.Question, location, offsetMargin, time));

    public static void Question(FrameworkElement owner, string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(owner, message, new ToastConfig(ToastIcon.Question, location, offsetMargin, time, isStacked, maxStacked));

    public static void Question(string message, ToastLocation location = ToastLocation.TopCenter, Thickness offsetMargin = default, int time = ToastConfig.NormalTime)
        => Show(null!, message, new ToastConfig(ToastIcon.Question, location, offsetMargin, time));

    public static void Question(string message, ToastLocation location, Thickness offsetMargin, int time, bool isStacked, int maxStacked = 5)
        => Show(null!, message, new ToastConfig(ToastIcon.Question, location, offsetMargin, time, isStacked, maxStacked));

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
