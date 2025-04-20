using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Wpf.Ui.Violeta.Resources;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Test;

internal partial class TrayIconManager
{
    private static TrayIconManager _instance = null!;

    private readonly TrayIconHost? _iconHost = null;

    private TrayIconManager()
    {
        using Win32Icon icon = new(ResourcesProvider.GetStream(new Uri("pack://application:,,,/Wpf.Ui.Test;component/Resources/Images/ProfilePicture.ico")));

        _iconHost = new TrayIconHost()
        {
            ToolTipText = "Wpf.Ui.Test",
            Icon = icon.Handle,
            Menu =
            [
                new TrayMenuItem()
                {
                    Header = Version,
                    IsEnabled = false,
                },
                new TraySeparator(),
                new TrayMenuItem()
                {
                    Header = "Restart",
                    Command = RestartCommand,
                },
                new TrayMenuItem()
                {
                    Header = "Exit",
                    Command = ExitCommand,
                }
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
}

internal partial class TrayIconManager : ObservableObject
{
    [ObservableProperty]
    private string version = $"v{Assembly.GetExecutingAssembly().GetName().Version!.ToString(4)}";

    [RelayCommand]
    private void ActivateOrRestoreMainWindow()
    {
        if (Application.Current.MainWindow is not null)
        {
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
    }

    [RelayCommand]
    private void Restart()
    {
        try
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo()
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
                fileName += ".exe";

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}
