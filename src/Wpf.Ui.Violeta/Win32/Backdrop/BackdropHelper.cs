using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Win32;

public static class BackdropHelper
{
    public static void EnableBlur(Window window)
    {
        var accent = new AccentPolicy();
        var accentStructSize = Marshal.SizeOf(accent);
        accent.AccentState = AccentState.AccentEnableBlurbehind;
        accent.AccentFlags = 2;
        accent.GradientColor = 0x99FFFFFF;

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WcaAccentPolicy,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

        _ = User32.SetWindowCompositionAttribute(new WindowInteropHelper(window).Handle, ref data);

        Marshal.FreeHGlobal(accentPtr);
    }

    public static void EnableAcrylicBlur(Window window, Color tintColor, bool isDarkTheme, double tintOpacity = 0.7d)
    {
        window.Background = Brushes.Transparent;

        var hwnd = new WindowInteropHelper(window).EnsureHandleSafe();

        if (!window.AllowsTransparency && hwnd != IntPtr.Zero && HwndSource.FromHwnd(hwnd) is HwndSource hwndSource)
        {
            hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
        }

        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            var captionColor = -2;
            _ = DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.CAPTION_COLOR, ref captionColor, Marshal.SizeOf<int>());
        }

        SetImmersiveDarkMode(hwnd, isDarkTheme);

        var margins = new DwmApi.Margins(-1, -1, -1, -1);
        DwmApi.DwmExtendFrameIntoClientArea(hwnd, ref margins);

        var accent = new AccentPolicy();
        var accentStructSize = Marshal.SizeOf(accent);
        accent.AccentState = AccentState.AccentEnableAcrylicblurbehind;
        accent.GradientColor = ToAbgr(tintColor, tintOpacity);

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WcaAccentPolicy,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

        _ = User32.SetWindowCompositionAttribute(hwnd, ref data);

        Marshal.FreeHGlobal(accentPtr);
    }

    public static void DisableDwmBlur(Window window)
    {
        var accent = new AccentPolicy();
        var accentStructSize = Marshal.SizeOf(accent);
        accent.AccentState = AccentState.AccentDisabled;

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WcaAccentPolicy,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

        var hwnd = new WindowInteropHelper(window).EnsureHandleSafe();
        _ = User32.SetWindowCompositionAttribute(hwnd, ref data);

        Marshal.FreeHGlobal(accentPtr);

        var margins = new DwmApi.Margins();
        DwmApi.DwmExtendFrameIntoClientArea(hwnd, ref margins);

        if (Environment.OSVersion.Version >= new Version(10, 0, 21996))
        {
            var micaEnabled = 0;
            _ = DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.MICA_EFFECT, ref micaEnabled, Marshal.SizeOf<int>());
        }

        if (Environment.OSVersion.Version >= new Version(10, 0, 22523))
        {
            var backdropType = (int)SystembackdropType.None;
            _ = DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, Marshal.SizeOf<int>());
        }

        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            var captionColor = -1;
            _ = DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.CAPTION_COLOR, ref captionColor, Marshal.SizeOf<int>());
        }

        SetImmersiveDarkMode(hwnd, false);

        // Restore system rounded corners
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            int cornerPreference = (int)WindowCornerPreference.Round;
            _ = DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.WINDOW_CORNER_PREFERENCE, ref cornerPreference, Marshal.SizeOf<int>());
        }
    }

    private static void SetImmersiveDarkMode(nint hwnd, bool enabled)
    {
        if (hwnd == IntPtr.Zero || Environment.OSVersion.Version < new Version(10, 0, 17763))
            return;

        var darkMode = enabled ? 1 : 0;
        var attribute = Environment.OSVersion.Version < new Version(10, 0, 18985)
            ? DwmApi.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE_OLD
            : DwmApi.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;

        _ = DwmApi.DwmSetWindowAttribute(hwnd, attribute, ref darkMode, Marshal.SizeOf<int>());
    }

    private static void EnableDwmBlur(Window window, bool isDarkTheme, uint dwAttribute, int pvAttribute)
    {
        // Mica will handle the color
        window.Background = Brushes.Transparent;

        var hwnd = new WindowInteropHelper(window).EnsureHandleSafe();

        if (!window.AllowsTransparency && hwnd != IntPtr.Zero && HwndSource.FromHwnd(hwnd) is HwndSource hwndSource)
        {
            hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
        }

        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            var captionColor = -2;
            _ = DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.CAPTION_COLOR, ref captionColor, Marshal.SizeOf<int>());
        }

        var isDarkThemeInt = isDarkTheme ? 1 : 0;
        _ = DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref isDarkThemeInt, Marshal.SizeOf<int>());

        var margins = new DwmApi.Margins(-1, -1, -1, -1);
        DwmApi.DwmExtendFrameIntoClientArea(hwnd, ref margins);

        var val = pvAttribute;
        _ = DwmApi.DwmSetWindowAttribute(hwnd, (DwmApi.DWMWINDOWATTRIBUTE)dwAttribute, ref val, Marshal.SizeOf<int>());
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public nint Data;
        public int SizeOfData;
    }

    public enum WindowCompositionAttribute
    {
        WcaAccentPolicy = 19,
    }

    private enum AccentState
    {
        AccentDisabled = 0,
        AccentEnableGradient = 1,
        AccentEnableTransparentgradient = 2,
        AccentEnableBlurbehind = 3,
        AccentEnableAcrylicblurbehind = 4,
        AccentInvalidState = 5,
    }

    private static uint ToAbgr(Color color, double alphaScale)
    {
        return (uint)(color.R << 0 |
            color.G << 8 |
            color.B << 16 |
            (int)(color.A * alphaScale) << 24);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public uint GradientColor;
        public readonly int AnimationId;
    }
}

file static class WindowInteropHelperExtension
{
    extension(WindowInteropHelper windowInteropHelper)
    {
        public nint EnsureHandleSafe()
        {
            try
            {
                return windowInteropHelper?.EnsureHandle() ?? IntPtr.Zero;
            }
            catch (Exception e)
            {
                // Returning 0 is fine, since this error usually only occurs when the window is already closed or being disposed.
                Debug.WriteLine(e.ToString());
                return IntPtr.Zero;
            }
        }
    }
}
