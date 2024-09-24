using System;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Win32;

public static class DpiAware
{
    /// <summary>
    /// <see cref="DisableDpiAwarenessAttribute"/>
    /// </summary>
    public static bool SetProcessDpiAwareness()
    {
        if (NTdll.RtlGetVersion(out NTdll.OSVERSIONINFOEX osv) == NTdll.NTStatus.STATUS_SUCCESS)
        {
            Version osVersion = new(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && osVersion >= new Version(6, 3))
            {
                if (SHCore.SetProcessDpiAwareness(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE) == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
