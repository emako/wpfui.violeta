using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct RECT(int left, int top, int right, int bottom)
{
    public int Left = left;
    public int Top = top;
    public int Right = right;
    public int Bottom = bottom;

    public int Width => Right - Left;

    public int Height => Bottom - Top;

    public void Offset(int dx, int dy)
    {
        Left += dx;
        Top += dy;
        Right += dx;
        Bottom += dy;
    }

    public bool IsEmpty => Left >= Right || Top >= Bottom;
}
