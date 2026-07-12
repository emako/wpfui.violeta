using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Controls;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.Violeta.Gallery.Pages;

public partial class SettingsPage : Wpf.Ui.Violeta.Controls.Page
{
    public SettingsPage()
    {
        InitializeComponent();
        Loaded += SettingsPage_OnLoaded;
    }

    private void SettingsPage_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        var assembly = typeof(Wpf.Ui.Violeta.Controls.Page).Assembly;
        var version = assembly.GetName().Version;
        LibraryVersionText.Text = version is null
            ? "Wpf.Ui.Violeta"
            : $"Wpf.Ui.Violeta {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        var target = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName;
        RuntimeVersionText.Text = target ?? System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    }

    private void ThemeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var theme = ThemeComboBox.SelectedIndex switch
        {
            0 => ApplicationTheme.Unknown,
            1 => ApplicationTheme.Dark,
            2 => ApplicationTheme.Light,
            _ => ApplicationTheme.Dark,
        };

        if (theme == ApplicationTheme.Unknown)
        {
            ApplicationThemeManager.ApplySystemTheme();
        }
        else
        {
            ApplicationThemeManager.Apply(theme);
        }
    }
}
