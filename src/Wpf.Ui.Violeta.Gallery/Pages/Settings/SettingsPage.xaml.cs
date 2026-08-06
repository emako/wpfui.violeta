using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Gallery.Pages.Settings;

public partial class SettingsPage : Wpf.Ui.Violeta.Controls.Page
{
    private bool _syncingAppearance;
    private bool _syncingLanguage;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += SettingsPage_OnLoaded;
    }

    private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var assembly = typeof(Wpf.Ui.Violeta.Controls.Page).Assembly;
        var version = assembly.GetName().Version;
        LibraryVersionText.Text = version is null
            ? "Wpf.Ui.Violeta"
            : $"Wpf.Ui.Violeta {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        var target = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName;
        RuntimeVersionText.Text = target ?? System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        SyncAppearanceFromShell();
        SyncLanguageFromShell();
    }

    private void SyncAppearanceFromShell()
    {
        _syncingAppearance = true;
        try
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                ThemeComboBox.SelectedIndex = mainWindow.ThemeComboBoxSelectedIndex;
                SelectBackdropItem(mainWindow.WindowBackdropType);
            }
            else if (Window.GetWindow(this) is ShellWindow shell)
            {
                SelectBackdropItem(shell.WindowBackdropType);
            }

            UpdateBackdropItemAvailability();
        }
        finally
        {
            _syncingAppearance = false;
        }
    }

    private void SyncLanguageFromShell()
    {
        var language = Window.GetWindow(this) is MainWindow mainWindow
            ? mainWindow.LanguageComboBoxSelectedTag
            : MuiLanguageManager.Language;

        SelectLanguageItem(language);
    }

    private void SelectLanguageItem(string language)
    {
        _syncingLanguage = true;
        try
        {
            for (var i = 0; i < LanguageComboBox.Items.Count; i++)
            {
                if (LanguageComboBox.Items[i] is ComboBoxItem { Tag: string tag }
                    && string.Equals(tag, language, StringComparison.OrdinalIgnoreCase))
                {
                    LanguageComboBox.SelectedIndex = i;
                    return;
                }
            }
        }
        finally
        {
            _syncingLanguage = false;
        }
    }

    private void UpdateBackdropItemAvailability()
    {
        foreach (var item in BackdropComboBox.Items)
        {
            if (item is not ComboBoxItem comboItem || comboItem.Tag is not string tag)
            {
                continue;
            }

            if (!Enum.TryParse<WindowBackdropPreference>(tag, out var preference))
            {
                continue;
            }

            comboItem.IsEnabled = WindowBackdrop.IsSupported(preference);
        }
    }

    private void SelectBackdropItem(WindowBackdropPreference preference)
    {
        for (var i = 0; i < BackdropComboBox.Items.Count; i++)
        {
            if (BackdropComboBox.Items[i] is ComboBoxItem { Tag: string tag }
                && Enum.TryParse<WindowBackdropPreference>(tag, out var itemPreference)
                && itemPreference == preference)
            {
                BackdropComboBox.SelectedIndex = i;
                return;
            }
        }
    }

    private void LanguageComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _syncingLanguage)
        {
            return;
        }

        if (LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string language })
        {
            return;
        }

        MuiLanguageManager.SetLanguage(language);

        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.SyncLanguageComboBox(language);
        }
    }

    private void ThemeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _syncingAppearance)
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

        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.SyncThemeComboBox(ThemeComboBox.SelectedIndex);
        }
    }

    private void BackdropComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _syncingAppearance)
        {
            return;
        }

        if (BackdropComboBox.SelectedItem is not ComboBoxItem { Tag: string tag }
            || !Enum.TryParse<WindowBackdropPreference>(tag, out var preference))
        {
            return;
        }

        if (!WindowBackdrop.IsSupported(preference))
        {
            return;
        }

        if (Window.GetWindow(this) is ShellWindow shell)
        {
            shell.WindowBackdropType = preference;
        }
    }
}
