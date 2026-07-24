using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Applies the chosen backdrop effect to the selected window.
/// </summary>
public static class WindowBackdrop
{
    public const double DefaultCaptionHeight = 32d;
    public const double DefaultResizeBorderThickness = 6d;

    /// <summary>
    /// Checks whether the selected backdrop type is supported on current platform.
    /// </summary>
    /// <returns><see langword="true"/> if the selected backdrop type is supported on current platform.</returns>
    public static bool IsSupported(WindowBackdropPreference backdropType)
    {
        // Win XP = 5.1
        // Vista = 6.0
        // Win7 = 6.1
        // Win8 / 8.1 = 6.2 / 6.3
        // Win10 / 11 = 10.0
        return backdropType switch
        {
            WindowBackdropPreference.Auto => Environment.OSVersion.Version >= new Version(10, 0, 22523),
            WindowBackdropPreference.Mica => Environment.OSVersion.Version >= new Version(10, 0, 22000),
            WindowBackdropPreference.Acrylic => Environment.OSVersion.Version >= new Version(10, 0, 0),
            WindowBackdropPreference.Tabbed => Environment.OSVersion.Version >= new Version(10, 0, 22523),
            WindowBackdropPreference.Acrylic10 => Environment.OSVersion.Version >= new Version(10, 0, 22000),
            WindowBackdropPreference.Acrylic11 => Environment.OSVersion.Version >= new Version(10, 0, 22523),
            WindowBackdropPreference.None => true,
            _ => false,
        };
    }

    public static bool ApplyBackdrop(this Window window, WindowBackdropPreference backdrop, bool? isDarkTheme = null)
    {
        bool result = false;
        bool isDark = isDarkTheme ?? OSThemeHelper.AppsUseDarkTheme();

        switch (backdrop)
        {
            case WindowBackdropPreference.None:
                {
                    SetGlassFrameThickness(window, new Thickness(1d));
                    BackdropHelper.DisableDwmBlur(window); // Fix white flash in dark mode
                    window.Background = (Brush)window.FindResource("MainWindowBackgroundNoTransparent");
                    result = true;
                }
                break;

            case WindowBackdropPreference.Auto:
            case WindowBackdropPreference.Mica:
            default:
                if (App.IsWin11)
                {
                    if (Environment.OSVersion.Version >= new Version(10, 0, 22523))
                    {
                        // -1 extends glass over the full client area so Mica is visible.
                        SetGlassFrameThickness(window, new Thickness(-1d));
                        BackdropHelper.EnableBackdropMicaBlur(window, isDark);
                        window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                        result = true;
                    }
                    else
                    {
                        SetGlassFrameThickness(window, new Thickness(-1d));
                        BackdropHelper.EnableMicaBlur(window, isDark);
                        window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                        result = true;
                    }
                }
                else if (App.IsWin10)
                {
                    SetGlassFrameThickness(window, new Thickness(-1d));
                    BackdropHelper.EnableBlur(window);
                    window.Background = (Brush)window.FindResource("MainWindowBackground");
                    result = true;
                }
                else
                {
                    window.Background = (Brush)window.FindResource("MainWindowBackgroundNoTransparent");
                }

                break;

            case WindowBackdropPreference.Acrylic:
                if (App.IsWin11 && Environment.OSVersion.Version >= new Version(10, 0, 22523))
                {
                    SetGlassFrameThickness(window, new Thickness(-1d));
                    BackdropHelper.EnableBackdropAcrylicBlur(window, isDark);
                    window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                    result = true;
                }
                else if (App.IsWin10)
                {
                    SetGlassFrameThickness(window, new Thickness(0d));
                    BackdropHelper.EnableAcrylicBlur(window, Acrylic10Helper.GetAcrylicTintColor(), isDark);
                    window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                    result = true;
                }
                else
                {
                    window.Background = (Brush)window.FindResource("MainWindowBackgroundNoTransparent");
                }

                break;

            case WindowBackdropPreference.Acrylic10:
                if (App.IsWin10 || App.IsWin11)
                {
                    SetGlassFrameThickness(window, new Thickness(0d));
                    BackdropHelper.DisableDwmBlur(window); // Restore rounded corners on Windows 11
                    BackdropHelper.EnableAcrylicBlur(window, Acrylic10Helper.GetAcrylic10TintColor(), isDark, Acrylic10Helper.GetAcrylic10TintOpacity());
                    window.Background = Acrylic10Helper.GetAcrylic10TintLuminosityOpacityBackground(isDark);
                    result = true;
                }
                else
                {
                    window.Background = (Brush)window.FindResource("MainWindowBackgroundNoTransparent");
                }

                break;

            case WindowBackdropPreference.Acrylic11:
                if (App.IsWin11 && Environment.OSVersion.Version >= new Version(10, 0, 22523))
                {
                    SetGlassFrameThickness(window, new Thickness(-1d));
                    BackdropHelper.EnableBackdropAcrylicBlur(window, isDark);
                    window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                    result = true;
                }
                else if (App.IsWin11)
                {
                    SetGlassFrameThickness(window, new Thickness(0d));
                    BackdropHelper.DisableDwmBlur(window); // Restore rounded corners on Windows 11
                    BackdropHelper.EnableAcrylicBlur(window, Acrylic10Helper.GetAcrylicTintColor(), isDark);
                    window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                    result = true;
                }
                else if (App.IsWin10)
                {
                    SetGlassFrameThickness(window, new Thickness(0d));
                    BackdropHelper.EnableAcrylicBlur(window, Acrylic10Helper.GetAcrylicTintColor(), isDark);
                    window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                    result = true;
                }
                else
                {
                    window.Background = (Brush)window.FindResource("MainWindowBackgroundNoTransparent");
                }

                break;

            case WindowBackdropPreference.Tabbed:
                if (App.IsWin11 && Environment.OSVersion.Version >= new Version(10, 0, 22523))
                {
                    SetGlassFrameThickness(window, new Thickness(-1d));
                    BackdropHelper.EnableBackdropTabbedBlur(window, isDark);
                    window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);
                    result = true;
                }
                else if (App.IsWin10)
                {
                    SetGlassFrameThickness(window, new Thickness(-1d));
                    BackdropHelper.EnableBlur(window);
                    window.Background = (Brush)window.FindResource("MainWindowBackground");
                    result = true;
                }
                else
                {
                    window.Background = (Brush)window.FindResource("MainWindowBackgroundNoTransparent");
                }

                break;
        }

        return result;
    }

    public static bool RemoveBackdrop(this Window window)
    {
        SetGlassFrameThickness(window, new Thickness(1d));
        BackdropHelper.DisableDwmBlur(window); // Fix white flash in dark mode
        window.Background = (Brush)window.FindResource("MainWindowBackgroundNoTransparent");
        return true;
    }

    /// <summary>
    /// Tries to remove background from <see cref="Window"/> and it's composition area.
    /// </summary>
    /// <param name="window">Window to manipulate.</param>
    /// <returns><see langword="true"/> if operation was successful.</returns>
    public static bool RemoveBackground(this Window window)
    {
        if (window is null)
            return false;

        // Remove background from visual root
        window.SetCurrentValue(Control.BackgroundProperty, Brushes.Transparent);

        nint hwnd = new WindowInteropHelper(window).EnsureHandle();

        if (hwnd == IntPtr.Zero)
            return false;

        HwndSource? windowSource = HwndSource.FromHwnd(hwnd);

        // Remove background from client area
        if (windowSource?.Handle != IntPtr.Zero && windowSource?.CompositionTarget != null)
        {
            windowSource.CompositionTarget.BackgroundColor = Colors.Transparent;
        }

        return true;
    }

    public static bool RemoveTitlebarBackground(this Window window)
    {
        if (window is null)
            return false;

        nint hwnd = new WindowInteropHelper(window).Handle;

        if (hwnd == IntPtr.Zero)
            return false;

        HwndSource? windowSource = HwndSource.FromHwnd(hwnd);

        // Remove background from client area
        if (windowSource?.Handle != IntPtr.Zero && windowSource?.CompositionTarget != null)
        {
            // NOTE: https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute
            // Specifying DWMWA_COLOR_DEFAULT (value 0xFFFFFFFF) for the color will reset the window back to using the system's default behavior for the caption color.
            int titlebarPvAttribute = DwmApi.DWMWA_COLOR_NONE;

            return DwmApi.DwmSetWindowAttribute(windowSource.Handle, DwmApi.DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, ref titlebarPvAttribute, Marshal.SizeOf<int>()) == HRESULT.S_OK;
        }

        return true;
    }

    /// <summary>
    /// Tries to remove titlebar from selected <see cref="Window"/>.
    /// </summary>
    /// <param name="window">The window to which the effect is to be applied.</param>
    /// <returns><see langword="true"/> if invocation of native Windows function succeeds.</returns>
    public static bool RemoveWindowTitlebarContents(this Window window)
    {
        if (window == null)
            return false;

        if (window.IsLoaded)
        {
            nint hwnd = new WindowInteropHelper(window).Handle;
            return RemoveWindowTitlebarContents(hwnd);
        }

        window.Loaded += (_1, _2) =>
        {
            nint hwnd = new WindowInteropHelper(window).Handle;
            _ = RemoveWindowTitlebarContents(hwnd);
        };

        return true;

        static bool RemoveWindowTitlebarContents(nint hwnd)
        {
            int style = User32.GetWindowLong(hwnd, User32.GWL_STYLE);
            style &= ~User32.WS_SYSMENU;
            int result = User32.SetWindowLong(hwnd, User32.GWL_STYLE, style);
            return result > 0;
        }
    }

    /// <summary>
    /// Updates <see cref="WindowChrome.GlassFrameThickness"/>, cloning a frozen chrome instance when needed.
    /// </summary>
    public static void SetGlassFrameThickness(this Window window, Thickness thickness)
    {
        WindowChrome? chrome = WindowChrome.GetWindowChrome(window);
        if (chrome is null)
        {
            return;
        }

        if (chrome.IsFrozen)
        {
            chrome = (WindowChrome)chrome.Clone();
            chrome.GlassFrameThickness = thickness;
            WindowChrome.SetWindowChrome(window, chrome);
            return;
        }

        chrome.GlassFrameThickness = thickness;
    }

    /// <summary>
    /// Configures <see cref="WindowChrome"/> for the current backdrop preference.
    /// System backdrop materials require <see cref="WindowChrome.GlassFrameThickness"/> of <c>-1</c>.
    /// </summary>
    public static bool SetWindowChrome(this Window window, WindowBackdropPreference backdrop = WindowBackdropPreference.Mica)
    {
        double captionHeight = window.TryFindResource("MainWindowCaptionHeight") is double height
            ? height
            : DefaultCaptionHeight;

        Thickness resizeBorder = window.TryFindResource("MainWindowResizeThickness") is Thickness thickness
            ? thickness
            : new Thickness(DefaultResizeBorderThickness);

        if (window.ResizeMode == ResizeMode.NoResize)
        {
            resizeBorder = default;
        }

        // GlassFrameThickness = -1 extends the DWM glass frame over the entire client area,
        // which is required for Mica / Acrylic / Tabbed system backdrops to be visible.
        Thickness glassFrameThickness = backdrop == WindowBackdropPreference.None
            ? new Thickness(1d)
            : new Thickness(-1d);

        WindowChrome.SetWindowChrome(
            window,
            new WindowChrome
            {
                CaptionHeight = captionHeight,
                CornerRadius = default,
                GlassFrameThickness = glassFrameThickness,
                ResizeBorderThickness = resizeBorder,
                UseAeroCaptionButtons = false,
            }
        );

        return true;
    }
}

file static class App
{
    public static readonly bool IsWin11 = Environment.OSVersion.Version >= new Version(10, 0, 21996);
    public static readonly bool IsWin10 = !IsWin11 && Environment.OSVersion.Version >= new Version(10, 0);
}
