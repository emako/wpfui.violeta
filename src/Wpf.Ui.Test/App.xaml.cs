using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Resources;

namespace Wpf.Ui.Test;

public partial class App : Application
{
    public App()
    {
#if false
        I18N.Culture
            //= System.Threading.Thread.CurrentThread.CurrentUICulture
            //= System.Threading.Thread.CurrentThread.CurrentCulture
            = new System.Globalization.CultureInfo("zh-CN");
#endif

        SystemMenuThemeManager.Apply();
        TrayIconManager.Start();
        Splash.ShowAsync("pack://application:,,,/Wpf.Ui.Test;component/wpfui.png", 0.98d, actived: splash =>
        {
            // Support string for Hint property.
            // splash?.Hint = "Loading Wpf.Ui.Test...";

            // Support content for Hint property.
            splash?.Hint = new TextBlock()
            {
                Text = "Loading Wpf.Ui.Test...",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 14d,
            };
        });
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
