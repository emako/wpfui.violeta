using System.Runtime.InteropServices;
using System.Security;

namespace Wpf.Ui.Violeta.Win32;

internal static class DpiHelper
{
    public static float ScaleX => GetScale().X;
    public static float ScaleY => GetScale().Y;

    private static (float X, float Y) GetScale()
    {
        nint hdc = GetDC(0);
        float scaleX = GetDeviceCaps(hdc, DeviceCap.LOGPIXELSX);
        float scaleY = GetDeviceCaps(hdc, DeviceCap.LOGPIXELSY);
        _ = ReleaseDC(0, hdc);
        return new(scaleX / 96f, scaleY / 96f);
    }

    [SecurityCritical]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern nint GetDC(nint hWnd);

    [SecurityCritical]
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport("gdi32.dll", SetLastError = false, ExactSpelling = true)]
    private static extern int GetDeviceCaps(nint hdc, DeviceCap nIndex);

    private enum DeviceCap
    {
        LOGPIXELSX = 88,
        LOGPIXELSY = 90,
    }
}
