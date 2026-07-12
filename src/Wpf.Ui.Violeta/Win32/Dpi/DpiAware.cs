using System;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Win32;

public static class DpiAware
{
    /// <summary>
    /// <see cref="DisableDpiAwarenessAttribute"/>
    /// </summary>
    /// <param name="awareness">
    /// <see cref="SHCore.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE">0</see>
    /// <see cref="SHCore.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE">1</see>
    /// <see cref="SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE">2 (default)</see>
    /// </param>
    /// <returns></returns>
    public static bool SetProcessDpiAwareness(int awareness = (int)SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE)
    {
        if (NTdll.RtlGetVersion(out NTdll.OSVERSIONINFOEX osv) == NTdll.NTStatus.STATUS_SUCCESS)
        {
            Version osVersion = new(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && osVersion >= new Version(6, 3))
            {
                if (SHCore.SetProcessDpiAwareness((SHCore.PROCESS_DPI_AWARENESS)awareness) == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
