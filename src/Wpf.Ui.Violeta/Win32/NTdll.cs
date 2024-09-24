using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Wpf.Ui.Violeta.Win32;

internal static class NTdll
{
    [SecurityCritical]
    [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int RtlGetVersion(out OSVERSIONINFOEX versionInfo);

    public static Version GetOSVersion()
    {
        if (RtlGetVersion(out OSVERSIONINFOEX osv) == 0)
        {
            return new Version(osv.MajorVersion, osv.MinorVersion, osv.BuildNumber, osv.PlatformId);
        }

        return Environment.OSVersion.Version;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OSVERSIONINFOEX
    {
        public int OSVersionInfoSize;
        public int MajorVersion;
        public int MinorVersion;
        public int BuildNumber;
        public int PlatformId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string CSDVersion;

        public ushort ServicePackMajor;
        public ushort ServicePackMinor;
        public short SuiteMask;
        public byte ProductType;
        public byte Reserved;
    }

    public static class NTStatus
    {
        public const int STATUS_SUCCESS = 0;
    }
}
