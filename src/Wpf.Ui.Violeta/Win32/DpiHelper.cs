namespace Wpf.Ui.Violeta.Win32;

internal static class DpiHelper
{
    public static float ScaleX => GetScale().X;
    public static float ScaleY => GetScale().Y;

    private static (float X, float Y) GetScale()
    {
        nint hdc = User32.GetDC(0);
        float scaleX = User32.GetDeviceCaps(hdc, User32.DeviceCap.LOGPIXELSX);
        float scaleY = User32.GetDeviceCaps(hdc, User32.DeviceCap.LOGPIXELSY);
        _ = User32.ReleaseDC(0, hdc);
        return new(scaleX / 96f, scaleY / 96f);
    }
}
