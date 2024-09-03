using System.Windows;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Test;

public partial class App : Application
{
    public App()
    {
        Splash.ShowAsync("pack://application:,,,/Wpf.Ui.Test;component/wpfui.png", 0.98d);
        InitializeComponent();
    }
}
