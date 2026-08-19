using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Gallery.Pages.Settings;

public partial class SettingsPage : Wpf.Ui.Violeta.Controls.Page
{
    private static bool _followSystemAccent = true;
    private static Color? _customAccentColor;
    private static bool _themeChangedHooked;

    private bool _syncingAppearance;
    private bool _syncingLanguage;
    private bool _syncingCloseToTray;
    private bool _syncingAccent;

    public SettingsPage()
    {
        InitializeComponent();
        EnsureThemeChangedHook();
        Loaded += SettingsPage_OnLoaded;
    }

    private static void EnsureThemeChangedHook()
    {
        if (_themeChangedHooked)
        {
            return;
        }

        ThemeManager.Changed += OnApplicationThemeChanged;
        _themeChangedHooked = true;
    }

    private static void OnApplicationThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        if (_followSystemAccent || _customAccentColor is not { } color)
        {
            return;
        }

        var theme = currentApplicationTheme is ApplicationTheme.Unknown or ApplicationTheme.HighContrast
            ? ThemeManager.GetAppTheme()
            : currentApplicationTheme;

        ApplicationAccentColorManager.Apply(color, theme, systemGlassColor: false, systemAccentColor: false);
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

        SyncCloseToTray();
        SyncAccentFromState();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        SyncAppearanceFromShell();
        SyncLanguageFromShell();
        SyncCloseToTray();
        SyncAccentFromState();
    }

    private void SyncCloseToTray()
    {
        _syncingCloseToTray = true;
        try
        {
            CloseToTrayToggle.IsChecked = TrayIconManager.MinimizeToTrayOnClose;
        }
        finally
        {
            _syncingCloseToTray = false;
        }
    }

    private void CloseToTrayToggle_OnChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _syncingCloseToTray)
        {
            return;
        }

        TrayIconManager.MinimizeToTrayOnClose = CloseToTrayToggle.IsChecked == true;
    }

    private void SyncAccentFromState()
    {
        _syncingAccent = true;
        try
        {
            FollowSystemAccentToggle.IsChecked = _followSystemAccent;
            AccentSwatchPanel.IsEnabled = !_followSystemAccent;
            SelectAccentSwatch(_customAccentColor);
        }
        finally
        {
            _syncingAccent = false;
        }
    }

    private void SelectAccentSwatch(Color? color)
    {
        foreach (var swatch in AccentSwatchPanel.Children.OfType<Swatch>())
        {
            swatch.IsSelected = color is { } selected
                && TryGetSwatchColor(swatch, out var swatchColor)
                && ColorsEqual(swatchColor, selected);
        }
    }

    private void FollowSystemAccentToggle_OnChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _syncingAccent)
        {
            return;
        }

        _followSystemAccent = FollowSystemAccentToggle.IsChecked == true;
        AccentSwatchPanel.IsEnabled = !_followSystemAccent;

        if (_followSystemAccent)
        {
            _customAccentColor = null;
            SelectAccentSwatch(null);
            ApplicationAccentColorManager.ApplySystemAccent();
            return;
        }

        if (_customAccentColor is { } color)
        {
            ApplyCustomAccent(color);
            SelectAccentSwatch(color);
            return;
        }

        if (AccentSwatchPanel.Children.OfType<Swatch>().FirstOrDefault() is { } first
            && TryGetSwatchColor(first, out var firstColor))
        {
            ApplyCustomAccent(firstColor);
            SelectAccentSwatch(firstColor);
        }
    }

    private void AccentSwatch_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _syncingAccent || sender is not Swatch clicked)
        {
            return;
        }

        if (!TryGetSwatchColor(clicked, out var color))
        {
            return;
        }

        _syncingAccent = true;
        try
        {
            _followSystemAccent = false;
            FollowSystemAccentToggle.IsChecked = false;
            AccentSwatchPanel.IsEnabled = true;
        }
        finally
        {
            _syncingAccent = false;
        }

        ApplyCustomAccent(color);
        SelectAccentSwatch(color);
    }

    private static void ApplyCustomAccent(Color color)
    {
        _customAccentColor = color;
        ApplicationAccentColorManager.Apply(
            color,
            ThemeManager.GetAppTheme(),
            systemGlassColor: false,
            systemAccentColor: false);
    }

    private static bool TryGetSwatchColor(Swatch swatch, out Color color)
    {
        if (swatch.Color is SolidColorBrush brush)
        {
            color = brush.Color;
            return true;
        }

        if (swatch.Value is string hex && TryParseColor(hex, out color))
        {
            return true;
        }

        color = default;
        return false;
    }

    private static bool TryParseColor(string value, out Color color)
    {
        try
        {
            color = (Color)ColorConverter.ConvertFromString(value)!;
            return true;
        }
        catch
        {
            color = default;
            return false;
        }
    }

    private static bool ColorsEqual(Color left, Color right)
    {
        return left.R == right.R && left.G == right.G && left.B == right.B;
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
            ThemeManager.ApplySystemTheme();
        }
        else
        {
            ThemeManager.Apply(theme);
        }

        if (!_followSystemAccent && _customAccentColor is { } color)
        {
            ApplyCustomAccent(color);
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
