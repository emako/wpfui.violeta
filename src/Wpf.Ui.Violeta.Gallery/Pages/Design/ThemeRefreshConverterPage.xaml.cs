using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Gallery.Pages.Design;

public partial class ThemeRefreshConverterPage : Wpf.Ui.Violeta.Controls.Page
{
    public ThemeRefreshConverterPage()
    {
        InitializeComponent();
    }

    private void OnToggleThemeClick(object sender, RoutedEventArgs e)
    {
        ApplicationTheme current = ThemeManager.GetAppTheme();
        ApplicationTheme next = current == ApplicationTheme.Light
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light;
        ThemeManager.Apply(next);
    }
}
