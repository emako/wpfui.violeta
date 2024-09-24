using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

internal static class SHCore
{
    [DllImport("shcore.dll")]
    public static extern uint SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

    [DllImport("shcore.dll")]
    public static extern int GetDpiForMonitor(nint hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

    public enum PROCESS_DPI_AWARENESS
    {
        PROCESS_DPI_UNAWARE,
        PROCESS_SYSTEM_DPI_AWARE,
        PROCESS_PER_MONITOR_DPI_AWARE,
    }

    public enum MONITOR_DPI_TYPE
    {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI,
        MDT_RAW_DPI,
        MDT_DEFAULT = MDT_EFFECTIVE_DPI,
    }
}
