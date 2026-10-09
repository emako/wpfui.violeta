using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

internal static class SHCore
{
    [DllImport("shcore.dll")]
    public static extern uint SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

    [DllImport("shcore.dll")]
    public static extern int GetProcessDpiAwareness(nint hprocess, out PROCESS_DPI_AWARENESS value);

    [DllImport("shcore.dll")]
    public static extern int GetDpiForMonitor(nint hMonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

    public enum MONITOR_DPI_TYPE
    {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI,
        MDT_RAW_DPI,
        MDT_DEFAULT = MDT_EFFECTIVE_DPI,
    }
}

public enum PROCESS_DPI_AWARENESS
{
    PROCESS_DPI_UNAWARE,
    PROCESS_SYSTEM_DPI_AWARE,
    PROCESS_PER_MONITOR_DPI_AWARE,
}

public enum DPI_AWARENESS_CONTEXT
{
    DPI_AWARENESS_CONTEXT_UNAWARE = -1,
    DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2,
    DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3,
    DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4,
    DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = -5,
}
