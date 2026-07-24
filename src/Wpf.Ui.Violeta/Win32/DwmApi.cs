using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// DWM / User32 helpers for applying acrylic blur, rounded corners and composition effects
/// to arbitrary native windows (e.g. popup HWNDs).
/// </summary>
internal static class DwmApi
{
    public const int DWMWA_COLOR_DEFAULT = -1; // =4294967295U, =0xFFFFFFFF
    public const int DWMWA_COLOR_NONE = -2; // =4294967294U, =0xFFFFFFFE

    /// <summary>Flags used by the [DwmGetWindowAttribute](/windows/desktop/api/dwmapi/nf-dwmapi-dwmgetwindowattribute) and [DwmSetWindowAttribute](/windows/desktop/api/dwmapi/nf-dwmapi-dwmsetwindowattribute) functions.</summary>
    /// <remarks>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute">Learn more about this API from learn.microsoft.com</see>.</para>
    /// </remarks>
    internal enum DWMWINDOWATTRIBUTE : int
    {
        DWMWA_TRANSITIONS_FORCEDISABLED = 3,
        DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19,
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_WINDOW_CORNER_PREFERENCE = 33,
        DWMWA_CAPTION_COLOR = 35,
        DWMWA_SYSTEMBACKDROP_TYPE = 38,
        DWMWA_MICA_EFFECT = 1029,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Margins(int leftWidth, int rightWidth, int topHeight, int bottomHeight)
    {
        public int LeftWidth = leftWidth;
        public int RightWidth = rightWidth;
        public int TopHeight = topHeight;
        public int BottomHeight = bottomHeight;

        public Margins() : this(0, 0, 0, 0)
        {
        }
    }

    // ------------------------------------------------------------------
    // P/Invoke
    // ------------------------------------------------------------------

    [DllImport("dwmapi.dll")]
    internal static extern nint DwmExtendFrameIntoClientArea(nint hwnd, ref Margins margins);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(nint hwnd, DWMWINDOWATTRIBUTE attr, ref int pvAttr, int cbAttr);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int SetWindowCompositionAttribute(nint hwnd, ref WindowCompositionAttributeData data);

    // ------------------------------------------------------------------
    // Internal helpers
    // ------------------------------------------------------------------

    /// <summary>Converts a WPF <see cref="Color"/> to Win32 COLORREF (ABGR layout used by GradientColor).</summary>
    internal static int ToWin32Color(Color c) =>
        c.R | (c.G << 8) | (c.B << 16) | (c.A << 24);

    /// <summary>Enables or disables DWM window transition animations.</summary>
    internal static void SetTransitionsForceDisabled(nint hwnd, bool disabled)
    {
        int value = disabled ? 1 : 0;
        _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_TRANSITIONS_FORCEDISABLED, ref value, Marshal.SizeOf<int>());
    }

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
        _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref val, Marshal.SizeOf<int>());
    }

    /// <summary>Enables or disables the immersive dark mode frame rendering on the given HWND.</summary>
    internal static void SetImmersiveDarkMode(nint hwnd, bool dark)
    {
        int val = dark ? 1 : 0;
        _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref val, Marshal.SizeOf<int>());
    }

    /// <summary>Enables or disables the acrylic blur-behind composition effect.</summary>
    internal static void SetAcrylicComposition(nint hwnd, bool enable, Color? tintColor = null)
    {
        var accent = new AccentPolicy();
        if (enable)
        {
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            // Use supplied tint, or a near-transparent default so the effect is visible
            accent.GradientColor = tintColor.HasValue ? (uint)ToWin32Color(tintColor.Value) : 0x01000000;
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
    /// transparent WPF composition target + immersive dark mode + DWM frame extension + acrylic + corner rounding.
    /// When <paramref name="tintColor"/> is fully transparent the method selects a theme-appropriate default
    /// (#CC1C1C1C for dark, #CCF3F3F3 for light).
    /// </summary>
    internal static void ApplyPopupMaterial(nint hwnd, Color tintColor, WindowCornerPreference corner, bool isDark)
    {
        if (hwnd == 0) return;

        var source = HwndSource.FromHwnd(hwnd);
        if (source?.CompositionTarget is not null)
            source.CompositionTarget.BackgroundColor = Colors.Transparent;

        SetImmersiveDarkMode(hwnd, isDark);
        ExtendFrameIntoClientArea(hwnd);

        // When the caller has not set a custom tint (fully transparent), choose a sensible default
        // that matches the current light/dark theme so the acrylic surface looks natural.
        Color effectiveTint = tintColor.A == 0
            ? (isDark
                ? Color.FromArgb(0xCC, 0x1C, 0x1C, 0x1C)   // dark  ≈ Windows 11 dark acrylic
                : Color.FromArgb(0xCC, 0xF3, 0xF3, 0xF3))  // light ≈ Windows 11 light acrylic
            : tintColor;

        SetAcrylicComposition(hwnd, true, effectiveTint);
        SetWindowCorner(hwnd, corner);
    }
}
