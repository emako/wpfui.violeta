using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Resources;

namespace Wpf.Ui.Test;

public partial class App : Application
{
    public App()
    {
        Splash.ShowAsync("pack://application:,,,/Wpf.Ui.Test;component/wpfui.png", 0.98d);
        InitializeComponent();

        DispatcherUnhandledException += (object s, DispatcherUnhandledExceptionEventArgs e) =>
        {
            Debug.WriteLine("Application.DispatcherUnhandledException " + e.Exception?.ToString() ?? string.Empty);
            ExceptionReport.Show(e.Exception!);
            e.Handled = true;
        };

        string sampleMd = ResourcesProvider.GetString(@"pack://application:,,,/Resources/Strings/Sample.md");
    }
}
