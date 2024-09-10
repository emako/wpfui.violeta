using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

public static class ExceptionReport
{
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

        _ = Application.Current.Dispatcher.Invoke(() => _ = new ExceptionWindow(e, appName, appVersion) { Owner = owner }.ShowDialog());
    }

    public static async Task ShowAsync(Exception e, string? appName = null, string? appVersion = null)
    {
        Window? owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;

        await Application.Current.Dispatcher.BeginInvoke(() => _ = new ExceptionWindow(e, appName, appVersion) { Owner = owner }.ShowDialog());
    }
}
