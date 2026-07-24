using System;

namespace Wpf.Ui.Violeta.Win32;

public static class OSThemeHelper
{
    public static bool AppsUseDarkTheme()
    {
        uint dataSize = sizeof(uint);
        int result = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "AppsUseLightTheme",
            AdvApi32.RRF_RT_REG_DWORD, IntPtr.Zero, out uint data, ref dataSize);

        if (result != 0)
            return true;

        return data == 0;
    }

    public static bool SystemUsesDarkTheme()
    {
        uint dataSize = sizeof(uint);
        int result = AdvApi32.RegGetValue(AdvApi32.HKEY_CURRENT_USER,
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "SystemUsesLightTheme",
            AdvApi32.RRF_RT_REG_DWORD, IntPtr.Zero, out uint data, ref dataSize);

        if (result != 0)
            return true;

        return data == 0;
    }
}
