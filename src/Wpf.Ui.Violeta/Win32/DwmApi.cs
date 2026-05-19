using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>Win32 corner rounding preference (requires Windows 11+).</summary>
public enum WindowCornerPreference
{
    Default = 0,
    DoNotRound = 1,
    Round = 2,
    RoundSmall = 3,
}

/// <summary>
/// DWM / User32 helpers for applying acrylic blur, rounded corners and composition effects
/// to arbitrary native windows (e.g. popup HWNDs).
/// </summary>
internal static class DwmApi
{
    // ------------------------------------------------------------------
    // Enumerations
    // ------------------------------------------------------------------

    private enum DWMWINDOWATTRIBUTE
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        WINDOW_CORNER_PREFERENCE = 33,
        DWMWA_SYSTEMBACKDROP_TYPE = 38,
    }

    private enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5,
    }

    private enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19,
    }

    // ------------------------------------------------------------------
    // Structures
    // ------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public nint Data;
        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Margins
    {
        public int LeftWidth;
        public int RightWidth;
        public int TopHeight;
        public int BottomHeight;
    }

    // ------------------------------------------------------------------
    // P/Invoke
    // ------------------------------------------------------------------

    [DllImport("dwmapi.dll")]
    private static extern nint DwmExtendFrameIntoClientArea(nint hwnd, ref Margins margins);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint hwnd, DWMWINDOWATTRIBUTE attr, ref int pvAttr, int cbAttr);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowCompositionAttribute(nint hwnd, ref WindowCompositionAttributeData data);

    // ------------------------------------------------------------------
    // Internal helpers
    // ------------------------------------------------------------------

    /// <summary>Converts a WPF <see cref="Color"/> to Win32 COLORREF (ABGR layout used by GradientColor).</summary>
    internal static int ToWin32Color(Color c) =>
        c.R | (c.G << 8) | (c.B << 16) | (c.A << 24);

    /// <summary>Extends the DWM frame into the client area so WPF can paint over it transparently.</summary>
    internal static void ExtendFrameIntoClientArea(nint hwnd, int margin = 1)
    {
        var m = new Margins { LeftWidth = margin, TopHeight = margin, RightWidth = margin, BottomHeight = margin };
        DwmExtendFrameIntoClientArea(hwnd, ref m);
    }

    /// <summary>Sets the DWM window corner style (requires Windows 11+).</summary>
    internal static void SetWindowCorner(nint hwnd, WindowCornerPreference corner)
    {
        int val = (int)corner;
        _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.WINDOW_CORNER_PREFERENCE, ref val, Marshal.SizeOf<int>());
    }

    /// <summary>Enables or disables the acrylic blur-behind composition effect.</summary>
    internal static void SetAcrylicComposition(nint hwnd, bool enable, Color? tintColor = null)
    {
        var accent = new AccentPolicy();
        if (enable)
        {
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            // Use supplied tint, or a near-transparent default so the effect is visible
            accent.GradientColor = tintColor.HasValue ? ToWin32Color(tintColor.Value) : 0x01000000;
        }
        else
        {
            accent.AccentState = AccentState.ACCENT_DISABLED;
        }

        int size = Marshal.SizeOf<AccentPolicy>();
        nint ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(accent, ptr, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = size,
                Data = ptr,
            };
            _ = SetWindowCompositionAttribute(hwnd, ref data);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>
    /// Applies the full Fluent acrylic material to a popup HWND:
    /// transparent WPF composition target + DWM frame extension + acrylic + corner rounding.
    /// </summary>
    internal static void ApplyPopupMaterial(nint hwnd, Color tintColor, WindowCornerPreference corner)
    {
        if (hwnd == 0) return;

        var source = HwndSource.FromHwnd(hwnd);
        if (source?.CompositionTarget is not null)
            source.CompositionTarget.BackgroundColor = Colors.Transparent;

        ExtendFrameIntoClientArea(hwnd);
        SetAcrylicComposition(hwnd, true, tintColor);
        SetWindowCorner(hwnd, corner);
    }
}
