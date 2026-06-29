using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Wpf.Ui.Violeta.Win32;

internal static class Ole32
{
    [DllImport("ole32.dll")]
    public static extern int CreateStreamOnHGlobal(nint hGlobal, bool fDeleteOnRelease, out IStream ppstm);
}
