using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

public static class ExceptionReport
{
    /// <summary>
    /// For safety only use `Application.Current.Dispatcher` for show dialog.
    /// It means the temporary STA thread will be skipped.
    /// </summary>
    public static bool IsOnlyApplicationDispatcher { get; set; } = true;

    public static void HandleOnUnhandledException(this Application app)
    {
        app.DispatcherUnhandledException -= OnApplicationUnhandledException;
        app.DispatcherUnhandledException += OnApplicationUnhandledException;
    }

    private static void OnApplicationUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        Debug.WriteLine("[ExceptionReport] Application.DispatcherUnhandledException " + e.Exception.ToString());
        Show(e.Exception);
    }

    public static void Show(Exception e, string? appName = null, string? appVersion = null)
    {
        Window? owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;

        Dispatcher dispatcher = Application.Current.Dispatcher;

        if (!IsOnlyApplicationDispatcher)
        {
            dispatcher = owner?.Dispatcher ?? Application.Current.Dispatcher;
        }

        _ = dispatcher.Invoke(() => _ = new ExceptionWindow(e, appName, appVersion) { Owner = owner }.ShowDialog());
    }

    public static async Task ShowAsync(Exception e, string? appName = null, string? appVersion = null)
    {
        Window? owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;

        Dispatcher dispatcher = Application.Current.Dispatcher;

        if (!IsOnlyApplicationDispatcher)
        {
            dispatcher = owner?.Dispatcher ?? Application.Current.Dispatcher;
        }

        await dispatcher.BeginInvoke(() => _ = new ExceptionWindow(e, appName, appVersion) { Owner = owner }.ShowDialog());
    }
}
