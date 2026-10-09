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
}
