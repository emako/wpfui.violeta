using Microsoft.Win32;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Appearance;

public static class ThemeManager
{
    public static ApplicationTheme GetSystemTheme()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        object? registryValueObject = key?.GetValue("AppsUseLightTheme");

        if (registryValueObject == null)
        {
            return ApplicationTheme.Light;
        }

        var registryValue = (int)registryValueObject;

        return registryValue > 0 ? ApplicationTheme.Light : ApplicationTheme.Dark;
    }

    public static void Apply(ApplicationTheme theme)
    {
        if (theme == ApplicationTheme.Unknown)
        {
            // To change `Unknown` to `System`.
            theme = GetSystemTheme();
        }

        if (ApplicationThemeManager.GetAppTheme() != theme)
        {
            ApplicationThemeManager.Apply(theme, WindowBackdropType.Mica, true);
        }
    }
}
