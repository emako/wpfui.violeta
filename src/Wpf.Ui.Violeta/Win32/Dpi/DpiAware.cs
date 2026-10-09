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

    /// <summary>
    /// <see cref="DpiAwarenessContext.Unaware">-1</see>
    /// <see cref="DpiAwarenessContext.SystemAware">-2</see>
    /// <see cref="DpiAwarenessContext.PerMonitorAware">-3</see>
    /// <see cref="DpiAwarenessContext.PerMonitorAwareV2">-4</see>
    /// <see cref="DpiAwarenessContext.UnawareGdiScaled">-5</see>
    /// </summary>
    public enum DpiAwarenessContext
    {
        Unaware = -1,
        SystemAware = -2,
        PerMonitorAware = -3,
        PerMonitorAwareV2 = -4,
        UnawareGdiScaled = -5,
    }

    /// <summary>
    /// Sets the process DPI awareness context. Requires Windows 10 version 1703 (build 15063) or later
    /// for <see cref="DpiAwarenessContext.PerMonitorAwareV2"/>.
    /// Call this before any windows are created.
    /// </summary>
    /// <param name="dpiContext">
    /// Defaults to <see cref="DpiAwarenessContext.PerMonitorAwareV2"/>.
    /// </param>
    public static bool SetProcessDpiAwarenessContext(DpiAwarenessContext dpiContext = DpiAwarenessContext.PerMonitorAwareV2)
    {
        if (NTdll.RtlGetVersion(out NTdll.OSVERSIONINFOEX osv) == NTdll.NTStatus.STATUS_SUCCESS)
        {
            Version osVersion = new(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && osVersion >= new Version(10, 0, 15063))
            {
                if (User32.SetProcessDpiAwarenessContext((nint)dpiContext))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
