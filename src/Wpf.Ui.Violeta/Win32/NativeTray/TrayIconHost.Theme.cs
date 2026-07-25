using System;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Win32;

public partial class TrayIconHost
{
    protected void SetThemeMode(TrayThemeMode theme = TrayThemeMode.System)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT
            && NTdll.GetOSVersion().Build >= 18362)
        {
            if (theme == TrayThemeMode.System)
            {
                if (ThemeManager.SystemUsesDarkTheme())
                {
                    _ = UxTheme.SetPreferredAppMode(UxTheme.PreferredAppMode.ForceDark);
                    UxTheme.FlushMenuThemes();
                }

                UserPreferenceChanged -= OnUserPreferenceChangedEventHandler;
                UserPreferenceChanged += OnUserPreferenceChangedEventHandler;
            }
            else if (theme == TrayThemeMode.Dark)
            {
                UserPreferenceChanged -= OnUserPreferenceChangedEventHandler;
                _ = UxTheme.SetPreferredAppMode(UxTheme.PreferredAppMode.ForceDark);
                UxTheme.FlushMenuThemes();
            }
            else if (theme == TrayThemeMode.Light)
            {
                UserPreferenceChanged -= OnUserPreferenceChangedEventHandler;
                _ = UxTheme.SetPreferredAppMode(UxTheme.PreferredAppMode.ForceLight);
                UxTheme.FlushMenuThemes();
            }
        }
    }

    protected static void OnUserPreferenceChangedEventHandler(object? sender, EventArgs e)
    {
        if (ThemeManager.SystemUsesDarkTheme())
        {
            _ = UxTheme.SetPreferredAppMode(UxTheme.PreferredAppMode.ForceDark);
            UxTheme.FlushMenuThemes();
        }
        else
        {
            _ = UxTheme.SetPreferredAppMode(UxTheme.PreferredAppMode.ForceLight);
            UxTheme.FlushMenuThemes();
        }
    }
}
