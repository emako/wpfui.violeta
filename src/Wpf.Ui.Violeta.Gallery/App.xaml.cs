using LiteObservableLanguages;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Gallery.Globalization;
using Wpf.Ui.Violeta.Gallery.Resources.Localization;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Gallery;

public partial class App : Application
{
    static App()
    {
        DpiAware.DisableDpiAwareness();

        if (DpiAware.SetProcessDpiAwarenessContext((int)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
        {
            Debug.WriteLine($"[DpiAware] SetProcessDpiAwarenessContext: {DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2} ({(int)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2})");
        }
        else
        {
            Debug.WriteLine($"[DpiAware] Failed to call SetProcessDpiAwarenessContext");
        }

        if (DpiAware.GetProcessDpiAwareness(out int awareness))
        {
            Debug.WriteLine($"[DpiAware] GetProcessDpiAwareness: {(PROCESS_DPI_AWARENESS)awareness} ({awareness})");
        }
        else
        {
            Debug.WriteLine($"[DpiAware] Failed to call GetProcessDpiAwareness");
        }

        if (DpiAware.GetProcessDpiAwarenessContext(out int dpiContext))
        {
            Debug.WriteLine($"[DpiAware] GetProcessDpiAwarenessContext: {(DPI_AWARENESS_CONTEXT)dpiContext} ({dpiContext})");
        }
        else
        {
            Debug.WriteLine($"[DpiAware] Failed to call GetProcessDpiAwarenessContext");
        }
    }

    public App()
    {
        SystemMenuThemeManager.Apply();
        TrayIconManager.Start();

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Locale.Default
            .UseResourceManager(SH.ResourceManager)
            .UseFallback(new CultureInfo("en-US"));

        LocaleManager.SetLanguage(LocaleManager.LanguageDefault);

        ThemeManager.Apply(ApplicationTheme.Dark);

        base.OnStartup(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogCrash(e.Exception);
        e.Handled = true;
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogCrash(ex);
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogCrash(e.Exception);
        e.SetObserved();
    }

    private static void LogCrash(Exception ex)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "wpfui-violeta-gallery-crash.txt");
            File.AppendAllText(path, $"[{DateTime.Now:o}] {ex}{Environment.NewLine}");
        }
        catch { }
    }
}
