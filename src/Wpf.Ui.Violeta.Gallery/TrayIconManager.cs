using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Wpf.Ui.Violeta.Resources;
using Wpf.Ui.Violeta.Win32;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery;

internal partial class TrayIconManager
{
    private static TrayIconManager? _instance;

    private readonly TrayIconHost? _iconHost;
    private bool _isExitRequested;

    private TrayIconManager()
    {
        using Win32Icon icon = new(ResourcesProvider.GetStream(
            new Uri("pack://application:,,,/Wpf.Ui.Violeta.Gallery;component/Resources/Images/wpfui.ico")));

        _iconHost = new TrayIconHost
        {
            ToolTipText = "Wpf.Ui.Violeta Gallery",
            Icon = icon.Handle,
            Menu =
            [
                new TrayMenuItem
                {
                    Header = Version,
                    IsEnabled = false,
                },
                new TraySeparator(),
                new TrayMenuItem
                {
                    Header = LangKeys.Sample_655a0e3730.Tr(),
                    Command = ActivateOrRestoreMainWindowCommand,
                    IsBold = true,
                },
                new TrayMenuItem
                {
                    Header = LangKeys.Sample_c054855df3.Tr(),
                    Command = ShowSampleNotificationCommand,
                },
                new TraySeparator(),
                new TrayMenuItem
                {
                    Header = LangKeys.Sample_01b4e06f39.Tr(),
                    Command = RestartCommand,
                },
                new TrayMenuItem
                {
                    Header = LangKeys.Sample_c3992269b4.Tr(),
                    Command = ExitCommand,
                },
            ],
        };

        _iconHost.LeftDoubleClick += (_, _) => ActivateOrRestoreMainWindow();
    }

    public static TrayIconManager GetInstance()
    {
        return _instance ??= new TrayIconManager();
    }

    public static void Start()
    {
        _ = GetInstance();
    }

    public static bool IsExitRequested => GetInstance()._isExitRequested;

    /// <summary>
    /// When true, closing the main window hides to the tray instead of exiting.
    /// Default is false; enable from Settings.
    /// </summary>
    public static bool MinimizeToTrayOnClose { get; set; }

    public static void ShowNotification(
        string title,
        string content,
        ToolTipIcon icon = default,
        int timeout = 5000,
        Action? clickEvent = null,
        Action? closeEvent = null)
    {
        var iconHost = GetInstance()._iconHost;
        if (iconHost is null)
        {
            return;
        }

        iconHost.ShowBalloonTip(timeout, title, content, icon);
        iconHost.BalloonTipClicked += OnIconOnBalloonTipClicked;
        iconHost.BalloonTipClosed += OnIconOnBalloonTipClosed;

        void OnIconOnBalloonTipClicked(object? sender, EventArgs e)
        {
            clickEvent?.Invoke();
            iconHost.BalloonTipClicked -= OnIconOnBalloonTipClicked;
        }

        void OnIconOnBalloonTipClosed(object? sender, EventArgs e)
        {
            closeEvent?.Invoke();
            iconHost.BalloonTipClosed -= OnIconOnBalloonTipClosed;
        }
    }
}

internal partial class TrayIconManager : ObservableObject
{
    [ObservableProperty]
    public partial string Version { get; set; } = $"v{Assembly.GetExecutingAssembly().GetName().Version!.ToString(4)}";

    [RelayCommand]
    private void ActivateOrRestoreMainWindow()
    {
        if (Application.Current.MainWindow is null)
        {
            return;
        }

        if (Application.Current.MainWindow.IsVisible)
        {
            Application.Current.MainWindow.Hide();
        }
        else
        {
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.Activate();
        }
    }

    [RelayCommand]
    private void ShowSampleNotification()
    {
        ShowNotification(
            "Wpf.Ui.Violeta Gallery",
            LangKeys.Sample_e2524ddfb6.Tr(),
            ToolTipIcon.Info,
            clickEvent: ActivateOrRestoreMainWindow);
    }

    [RelayCommand]
    private void Restart()
    {
        try
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetExecutablePath(),
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = true,
                },
            };
            process.Start();
        }
        catch (Win32Exception)
        {
            return;
        }

        Process.GetCurrentProcess().Kill();

        static string GetExecutablePath()
        {
            string fileName = AppDomain.CurrentDomain.FriendlyName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName += ".exe";
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }

    [RelayCommand]
    private void Exit()
    {
        _isExitRequested = true;
        Application.Current.Shutdown();
    }
}
