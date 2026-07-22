using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Wpf.Ui.Violeta.Win32;

internal static class TaskDialogNativeMethods
{
    /// <summary>
    /// Windows 10 version 1809 (October 2018 Update). Dark-mode APIs such as
    /// <c>AppsUseLightTheme</c>, DWM immersive dark mode, and <c>DarkMode_*</c> uxtheme classes
    /// require at least this build.
    /// </summary>
    public const int MinimumTaskDialogDarkModeBuildNumber = 17763;

    public static bool IsWindowsVistaOrLater
        => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 0, 6000);

    public static bool IsWindowsXPOrLater
        => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(5, 1, 2600);

    /// <summary>
    /// Gets the Windows build number reported by the kernel, or 0 when it cannot be determined.
    /// </summary>
    public static int WindowsBuildNumber => WindowsBuildNumberCore.Value;

    /// <summary>
    /// Gets a value indicating whether the current operating system supports task dialog dark mode.
    /// </summary>
    public static bool SupportsTaskDialogDarkMode
        => WindowsBuildNumber >= MinimumTaskDialogDarkModeBuildNumber;

    private static readonly Lazy<int> WindowsBuildNumberCore = new(GetWindowsBuildNumber);

    private static int GetWindowsBuildNumber()
    {
        RtlOsVersionInfoEx version = new()
        {
            dwOSVersionInfoSize = (uint)Marshal.SizeOf<RtlOsVersionInfoEx>(),
        };

        return RtlGetVersion(ref version) == 0 ? (int)version.dwBuildNumber : 0;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RtlOsVersionInfoEx
    {
        public uint dwOSVersionInfoSize;
        public uint dwMajorVersion;
        public uint dwMinorVersion;
        public uint dwBuildNumber;
        public uint dwPlatformId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;

        public ushort wServicePackMajor;
        public ushort wServicePackMinor;
        public ushort wSuiteMask;
        public byte wProductType;
        public byte wReserved;
    }

    [DllImport("ntdll.dll")]
    private static extern int RtlGetVersion(ref RtlOsVersionInfoEx lpVersionInformation);

    #region Task Dialogs

    public const int WM_USER = 0x400;
    public const int WM_GETICON = 0x007F;
    public const int WM_SETICON = 0x0080;
    public const int ICON_SMALL = 0;

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern nint GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern int GetWindowThreadProcessId(nint hWnd, out int lpdwProcessId);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern int GetCurrentThreadId();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern nint SendMessage(nint hwnd, int wMsg, nint wParam, nint lParam);

    [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
    [DllImport("comctl32.dll", PreserveSig = false)]
    public static extern void TaskDialogIndirect(
        [In] ref TASKDIALOGCONFIG pTaskConfig,
        out int pnButton,
        out int pnRadioButton,
        [MarshalAs(UnmanagedType.Bool)] out bool pfVerificationFlagChecked);

    public delegate uint TaskDialogCallback(nint hwnd, uint uNotification, nint wParam, nint lParam, nint dwRefData);

    public enum TaskDialogNotifications
    {
        Created = 0,
        Navigated = 1,
        ButtonClicked = 2,
        HyperlinkClicked = 3,
        Timer = 4,
        Destroyed = 5,
        RadioButtonClicked = 6,
        DialogConstructed = 7,
        VerificationClicked = 8,
        Help = 9,
        ExpandoButtonClicked = 10,
    }

    [Flags]
    public enum TaskDialogCommonButtonFlags
    {
        OkButton = 0x0001,
        YesButton = 0x0002,
        NoButton = 0x0004,
        CancelButton = 0x0008,
        RetryButton = 0x0010,
        CloseButton = 0x0020,
    }

    [Flags]
    public enum TaskDialogFlags
    {
        EnableHyperLinks = 0x0001,
        UseHIconMain = 0x0002,
        UseHIconFooter = 0x0004,
        AllowDialogCancellation = 0x0008,
        UseCommandLinks = 0x0010,
        UseCommandLinksNoIcon = 0x0020,
        ExpandFooterArea = 0x0040,
        ExpandedByDefault = 0x0080,
        VerificationFlagChecked = 0x0100,
        ShowProgressBar = 0x0200,
        ShowMarqueeProgressBar = 0x0400,
        CallbackTimer = 0x0800,
        PositionRelativeToWindow = 0x1000,
        RtlLayout = 0x2000,
        NoDefaultRadioButton = 0x4000,
        CanBeMinimized = 0x8000,
    }

    public enum TaskDialogMessages
    {
        NavigatePage = WM_USER + 101,
        ClickButton = WM_USER + 102,
        SetMarqueeProgressBar = WM_USER + 103,
        SetProgressBarState = WM_USER + 104,
        SetProgressBarRange = WM_USER + 105,
        SetProgressBarPos = WM_USER + 106,
        SetProgressBarMarquee = WM_USER + 107,
        SetElementText = WM_USER + 108,
        ClickRadioButton = WM_USER + 110,
        EnableButton = WM_USER + 111,
        EnableRadioButton = WM_USER + 112,
        ClickVerification = WM_USER + 113,
        UpdateElementText = WM_USER + 114,
        SetButtonElevationRequiredState = WM_USER + 115,
        UpdateIcon = WM_USER + 116,
    }

    public enum TaskDialogElements
    {
        Content,
        ExpandedInformation,
        Footer,
        MainInstruction,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TASKDIALOG_BUTTON
    {
        public int nButtonID;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszButtonText;
    }

    /// <remarks>
    /// Pack = 4 keeps cbSize at 160 on x64, matching comctl32 expectations.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TASKDIALOGCONFIG
    {
        public uint cbSize;
        public nint hwndParent;
        public nint hInstance;
        public TaskDialogFlags dwFlags;
        public TaskDialogCommonButtonFlags dwCommonButtons;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszWindowTitle;

        public nint hMainIcon;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszMainInstruction;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszContent;

        public uint cButtons;
        public nint pButtons;
        public int nDefaultButton;
        public uint cRadioButtons;
        public nint pRadioButtons;
        public int nDefaultRadioButton;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszVerificationText;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszExpandedInformation;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszExpandedControlText;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszCollapsedControlText;

        public nint hFooterIcon;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pszFooter;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public TaskDialogCallback? pfCallback;

        public nint lpCallbackData;
        public uint cxWidth;
    }

    #endregion Task Dialogs

    #region Activation Context

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern ActivationContextSafeHandle CreateActCtx(ref ACTCTX actctx);

    [DllImport("kernel32.dll")]
    public static extern void ReleaseActCtx(nint hActCtx);

    [DllImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ActivateActCtx(ActivationContextSafeHandle hActCtx, out nint lpCookie);

    [DllImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeactivateActCtx(uint dwFlags, nint lpCookie);

    public struct ACTCTX
    {
        public int cbSize;
        public uint dwFlags;
        public string lpSource;
        public ushort wProcessorArchitecture;
        public ushort wLangId;
        public string? lpAssemblyDirectory;
        public string? lpResourceName;
        public string? lpApplicationName;
    }

    #endregion Activation Context
}

internal sealed class ActivationContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public ActivationContextSafeHandle()
        : base(true)
    {
    }

    protected override bool ReleaseHandle()
    {
        TaskDialogNativeMethods.ReleaseActCtx(handle);
        return true;
    }
}
