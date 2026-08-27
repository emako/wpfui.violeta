using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LiteObservableLanguages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Gallery.Globalization;
using Wpf.Ui.Violeta.Gallery.Resources.Localization;

namespace Wpf.Ui.Violeta.Gallery;

public partial class App : Application
{
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

        MuiLanguageManager.SetLanguage(MuiLanguageManager.LanguageDefault);

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
