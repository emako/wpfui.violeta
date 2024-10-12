using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct RECT(int left, int top, int right, int bottom)
{
    public int left = left;
    public int top = top;
    public int right = right;
    public int bottom = bottom;

    public int Width => right - left;

    public int Height => bottom - top;

    public void Offset(int dx, int dy)
    {
        left += dx;
        top += dy;
        right += dx;
        bottom += dy;
    }

    public bool IsEmpty => left >= right || top >= bottom;
}