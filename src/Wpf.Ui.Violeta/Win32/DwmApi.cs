using System;
using System.Runtime.InteropServices;
using System.Windows;
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
        DWMWA_NCRENDERING_ENABLED = 1,
        DWMWA_NCRENDERING_POLICY = 2,
        DWMWA_TRANSITIONS_FORCEDISABLED = 3,
        DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19,
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_WINDOW_CORNER_PREFERENCE = 33,
        DWMWA_CAPTION_COLOR = 35,
        DWMWA_SYSTEMBACKDROP_TYPE = 38,
        DWMWA_MICA_EFFECT = 1029,
    }

    /// <summary>Values for <see cref="DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY"/>.</summary>
    internal enum DWMNCRENDERINGPOLICY : int
    {
        DWMNCRP_USEWINDOWSTYLE = 0,
        DWMNCRP_DISABLED = 1,
        DWMNCRP_ENABLED = 2,
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
    internal static uint ToWin32Color(Color c) =>
        (uint)(c.R | (c.G << 8) | (c.B << 16) | (c.A << 24));

    /// <summary>Enables or disables DWM window transition animations.</summary>
    internal static void SetTransitionsForceDisabled(nint hwnd, bool disabled)
    {
        int value = disabled ? 1 : 0;
        _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_TRANSITIONS_FORCEDISABLED, ref value, Marshal.SizeOf<int>());
    }

    /// <summary>
    /// Enables or disables DWM non-client rendering (including the system drop shadow).
    /// </summary>
    internal static void SetNcRenderingEnabled(nint hwnd, bool enabled)
    {
        int policy = (int)(enabled ? DWMNCRENDERINGPOLICY.DWMNCRP_ENABLED : DWMNCRENDERINGPOLICY.DWMNCRP_DISABLED);
        _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref policy, Marshal.SizeOf<int>());
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

    /// <summary>
    /// FluentWpfCore <c>MaterialApis.SetWindowComposition</c> — legacy acrylic for popups.
    /// </summary>
    internal static void SetAcrylicComposition(nint hwnd, bool enable, Color? tintColor = null)
    {
        var accent = new AccentPolicy();
        if (!enable)
        {
            accent.AccentState = AccentState.ACCENT_DISABLED;
        }
        else
        {
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            // FluentWpfCore: hexColor ?? 0x00000000
            accent.GradientColor = tintColor.HasValue ? ToWin32Color(tintColor.Value) : 0u;
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
    /// FluentWpfCore popup material: legacy acrylic by default; Win11 Mica / MicaAlt /
    /// TransientWindow (system acrylic) via <c>DWMWA_SYSTEMBACKDROP_TYPE</c>.
    /// System backdrops require a fully transparent composition target (no tint brush).
    /// </summary>
    /// <param name="systemBackdropType">
    /// <c>0</c> = legacy composition acrylic; otherwise a <c>DWMSBT_*</c> value
    /// (2 Mica, 3 TransientWindow, 4 Tabbed/MicaAlt).
    /// </param>
    internal static void ApplyPopupMaterial(
        nint hwnd,
        Color tintColor,
        WindowCornerPreference corner,
        bool isDark,
        int systemBackdropType = 0)
    {
        if (hwnd == 0) return;

        bool win11Backdrop = Environment.OSVersion.Version >= new Version(10, 0, 22621);
        bool useSystemBackdrop = systemBackdropType is 2 or 3 or 4;

        if (useSystemBackdrop && win11Backdrop)
        {
            // Match BackdropHelper.EnableDwmBlur / FluentWpfCore SetBackDropType path:
            // composition must be fully transparent — any opaque/tint fill hides Mica.
            SetAcrylicComposition(hwnd, enable: false);

            var hwndSource = HwndSource.FromHwnd(hwnd);
            if (hwndSource?.CompositionTarget is not null)
                hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;

            ClearPopupRootFill(hwndSource);

            // Win11 system backdrop: margin must be -1 (FluentWpfCore comment).
            var margins = new Margins(-1, -1, -1, -1);
            DwmExtendFrameIntoClientArea(hwnd, ref margins);

            SetImmersiveDarkMode(hwnd, isDark);

            int backdrop = systemBackdropType;
            _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, Marshal.SizeOf<int>());
        }
        else
        {
            var hwndSource = HwndSource.FromHwnd(hwnd);
            if (hwndSource?.CompositionTarget is not null)
                hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;

            SetImmersiveDarkMode(hwnd, isDark);

            if (win11Backdrop)
            {
                // DWMSBT_NONE = 1 (do not use WindowBackdropPreference.None == 0 / Auto).
                int none = 1;
                _ = DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref none, Marshal.SizeOf<int>());
            }

            ExtendFrameIntoClientArea(hwnd, margin: 1);

            Color compositionColor = tintColor.A == 0
                ? (isDark
                    ? Color.FromArgb(0x99, 0x28, 0x28, 0x28)
                    : Color.FromArgb(0x6C, 0xFF, 0xFF, 0xFF))
                : tintColor;

            SetAcrylicComposition(hwnd, enable: true, compositionColor);
        }

        SetWindowCorner(hwnd, corner);
    }

    /// <summary>
    /// Ensures PopupRoot does not paint an opaque fill over DWM system backdrop.
    /// </summary>
    private static void ClearPopupRootFill(HwndSource? source)
    {
        if (source?.RootVisual is not DependencyObject root)
            return;

        var bg = System.Windows.Controls.Control.BackgroundProperty;
        if (root is FrameworkElement fe)
        {
            try { fe.SetValue(bg, Brushes.Transparent); }
            catch { /* PopupRoot may not own Background */ }
        }

        if (System.Windows.Media.VisualTreeHelper.GetChildrenCount(root) > 0 &&
            System.Windows.Media.VisualTreeHelper.GetChild(root, 0) is FrameworkElement child)
        {
            try { child.SetValue(bg, Brushes.Transparent); }
            catch { /* ignore */ }
        }
    }
}
