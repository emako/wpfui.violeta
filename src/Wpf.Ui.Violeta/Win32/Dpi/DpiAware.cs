using System;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Win32;

public static class DpiAware
{
    /// <summary>
    /// <see cref="DisableDpiAwarenessAttribute"/>
    /// </summary>
    /// <param name="awareness">
    /// <see cref="PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE">0</see>
    /// <see cref="PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE">1</see>
    /// <see cref="PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE">2 (default)</see>
    /// </param>
    /// <returns></returns>
    public static bool SetProcessDpiAwareness(int awareness = (int)PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE)
    {
        if (NTdll.RtlGetVersion(out NTdll.OSVERSIONINFOEX osv) == NTdll.NTStatus.STATUS_SUCCESS)
        {
            Version osVersion = new(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && osVersion >= new Version(6, 3))
            {
                if (SHCore.SetProcessDpiAwareness((PROCESS_DPI_AWARENESS)awareness) == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the current process DPI awareness. Requires Windows 8.1 or later.
    /// </summary>
    /// <param name="awareness">
    /// <see cref="SHCore.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE">0</see>
    /// <see cref="SHCore.PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE">1</see>
    /// <see cref="SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE">2</see>
    /// </param>
    public static bool GetProcessDpiAwareness(out int awareness)
    {
        awareness = 0;

        if (NTdll.RtlGetVersion(out NTdll.OSVERSIONINFOEX osv) == NTdll.NTStatus.STATUS_SUCCESS)
        {
            Version osVersion = new(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && osVersion >= new Version(6, 3))
            {
                if (SHCore.GetProcessDpiAwareness(0, out PROCESS_DPI_AWARENESS value) == 0)
                {
                    awareness = (int)value;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the process DPI awareness context. Requires Windows 10 version 1703 (build 15063) or later
    /// for <see cref="SHCore.DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2"/>.
    /// Call this before any windows are created.
    /// </summary>
    /// <param name="dpiContext">
    /// Defaults to <see cref="SHCore.DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2"/>.
    /// </param>
    public static bool SetProcessDpiAwarenessContext(int dpiContext = (int)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2)
    {
        if (NTdll.RtlGetVersion(out NTdll.OSVERSIONINFOEX osv) == NTdll.NTStatus.STATUS_SUCCESS)
        {
            Version osVersion = new(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && osVersion >= new Version(10, 0, 15063))
            {
                if (User32.SetProcessDpiAwarenessContext(dpiContext))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the current thread DPI awareness context. Requires Windows 10 version 1607 (build 14393) or later.
    /// <see cref="SHCore.DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2"/> is only reported on Windows 10 version 1703 or later.
    /// </summary>
    public static bool GetProcessDpiAwarenessContext(out int dpiContext)
    {
        dpiContext = default;

        if (NTdll.RtlGetVersion(out NTdll.OSVERSIONINFOEX osv) == NTdll.NTStatus.STATUS_SUCCESS)
        {
            Version osVersion = new(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && osVersion >= new Version(10, 0, 14393))
            {
                nint context = User32.GetThreadDpiAwarenessContext();
                if (context != 0 && TryMatchDpiAwarenessContext(context, out DPI_AWARENESS_CONTEXT dpiContextEnum))
                {
                    dpiContext = (int)dpiContextEnum;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryMatchDpiAwarenessContext(nint context, out DPI_AWARENESS_CONTEXT dpiContext)
    {
        DPI_AWARENESS_CONTEXT[] known =
        [
            DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2,
            DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE,
            DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE,
            DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE,
            DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED,
        ];

        foreach (DPI_AWARENESS_CONTEXT candidate in known)
        {
            if (User32.AreDpiAwarenessContextsEqual(context, (nint)candidate))
            {
                dpiContext = candidate;
                return true;
            }
        }

        dpiContext = default;
        return false;
    }
}
