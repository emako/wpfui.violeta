using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Threading;

public static class ApplicationDispatcher
{
    public static Window MainWindow => Application.Current.Dispatcher.Invoke(() => Application.Current.MainWindow);

    public static void Invoke(Action callback, params object[] args)
    {
        _ = Application.Current?.Dispatcher.Invoke(callback, args);
    }

    public static void Invoke(Action<Window> callback)
    {
        _ = Application.Current?.Dispatcher.Invoke(callback, MainWindow);
    }

    public static T Invoke<T>(Func<T> action)
    {
        T t = default!;
        Application.Current?.Dispatcher.Invoke(() => t = action());
        return t;
    }

    public static void BeginInvoke(Action callback, params object[] args)
    {
        _ = Application.Current?.Dispatcher.BeginInvoke(callback, args);
    }

    public static void BeginInvoke(Action<Window> callback)
    {
        _ = Application.Current?.Dispatcher.BeginInvoke(callback, MainWindow);
    }
}
