#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Runtime.InteropServices;
using System.Text;

namespace Wpf.Ui.Violeta.Controls.Compat;

public static class PackagedAppHelper
{
    private const long APPMODEL_ERROR_NO_PACKAGE = 15700L;

    public static bool IsPackagedApp
    {
        get
        {
            if (OSVersionHelper.IsWindows8OrGreater)
            {
                int length = 0;
                var sb = new StringBuilder(0);
                GetCurrentPackageFullName(ref length, sb);

                sb.Length = length;
                int result = GetCurrentPackageFullName(ref length, sb);

                return result != APPMODEL_ERROR_NO_PACKAGE;
            }

            return false;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);
}
