using System.Runtime.InteropServices;

namespace Wpf.Ui.Violeta.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct WindowCompositionAttributeData
{
    public WindowCompositionAttribute Attribute;
    public nint Data;
    public int SizeOfData;
}

public enum WindowCompositionAttribute
{
    WCA_ACCENT_POLICY = 19,
}
