using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A custom WinUI Window with more convenience methods.
/// </summary>
public partial class ShellWindow : Window
{
    /// <summary>
    /// Gets contains helper for accessing this window handle.
    /// </summary>
    protected WindowInteropHelper InteropHelper
        => field ??= new WindowInteropHelper(this);

    public static readonly DependencyProperty WindowCornerPreferenceProperty = DependencyProperty.Register(
        nameof(WindowCornerPreference),
        typeof(WindowCornerPreference),
        typeof(ShellWindow),
        new PropertyMetadata(WindowCornerPreference.Round, OnWindowCornerPreferenceChanged)
    );

    public static readonly DependencyProperty WindowBackdropTypeProperty = DependencyProperty.Register(
        nameof(WindowBackdropType),
        typeof(WindowBackdropType),
        typeof(ShellWindow),
        new PropertyMetadata(WindowBackdropType.None, OnWindowBackdropTypeChanged)
    );

    public static readonly DependencyProperty ExtendsContentIntoTitleBarProperty =
        DependencyProperty.Register(
            nameof(ExtendsContentIntoTitleBar),
            typeof(bool),
            typeof(ShellWindow),
            new PropertyMetadata(false, OnExtendsContentIntoTitleBarChanged)
        );

    /// <summary>
    /// Gets or sets a value determining corner preference for current <see cref="Window"/>.
    /// </summary>
    public WindowCornerPreference WindowCornerPreference
    {
        get => (WindowCornerPreference)GetValue(WindowCornerPreferenceProperty);
        set => SetValue(WindowCornerPreferenceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value determining preferred backdrop type for current <see cref="Window"/>.
    /// </summary>
    public WindowBackdropType WindowBackdropType
    {
        get => (WindowBackdropType)GetValue(WindowBackdropTypeProperty);
        set => SetValue(WindowBackdropTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the default title bar of the window should be hidden to create space for app content.
    /// </summary>
    public bool ExtendsContentIntoTitleBar
    {
        get => (bool)GetValue(ExtendsContentIntoTitleBarProperty);
        set => SetValue(ExtendsContentIntoTitleBarProperty, value);
    }

    public ShellWindow()
    {
        SetResourceReference(StyleProperty, typeof(ShellWindow));
    }

    static ShellWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ShellWindow),
            new FrameworkPropertyMetadata(typeof(ShellWindow))
        );
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        OnCornerPreferenceChanged(default, WindowCornerPreference);
        OnExtendsContentIntoTitleBarChanged(default, ExtendsContentIntoTitleBar);
        OnBackdropTypeChanged(default, WindowBackdropType);

        base.OnSourceInitialized(e);
    }

    /// <summary>
    /// Private <see cref="WindowCornerPreference"/> property callback.
    /// </summary>
    private static void OnWindowCornerPreferenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ShellWindow window)
        {
            return;
        }

        if (e.OldValue == e.NewValue)
        {
            return;
        }

        window.OnCornerPreferenceChanged(
            (WindowCornerPreference)e.OldValue,
            (WindowCornerPreference)e.NewValue
        );
    }

    /// <summary>
    /// This virtual method is called when <see cref="WindowCornerPreference"/> is changed.
    /// </summary>
    protected virtual void OnCornerPreferenceChanged(WindowCornerPreference oldValue, WindowCornerPreference newValue)
    {
        if (InteropHelper.Handle == IntPtr.Zero)
        {
            return;
        }

        _ = UnsafeNativeMethods.ApplyWindowCornerPreference(InteropHelper.Handle, newValue);
    }

    /// <summary>
    /// Private <see cref="WindowBackdropType"/> property callback.
    /// </summary>
    private static void OnWindowBackdropTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ShellWindow window)
        {
            return;
        }

        if (e.OldValue == e.NewValue)
        {
            return;
        }

        window.OnBackdropTypeChanged((WindowBackdropType)e.OldValue, (WindowBackdropType)e.NewValue);
    }

    /// <summary>
    /// This virtual method is called when <see cref="WindowBackdropType"/> is changed.
    /// </summary>
    protected virtual void OnBackdropTypeChanged(WindowBackdropType oldValue, WindowBackdropType newValue)
    {
        if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.HighContrast)
        {
            newValue = WindowBackdropType.None;
        }

        if (InteropHelper.Handle == IntPtr.Zero)
        {
            return;
        }

        if (newValue == WindowBackdropType.None)
        {
            _ = WindowBackdrop.RemoveBackdrop(this);

            return;
        }

        if (!ExtendsContentIntoTitleBar)
        {
            throw new InvalidOperationException(
                $"Cannot apply backdrop effect if {nameof(ExtendsContentIntoTitleBar)} is false."
            );
        }

        if (WindowBackdrop.IsSupported(newValue) && WindowBackdrop.RemoveBackground(this))
        {
            _ = WindowBackdrop.ApplyBackdrop(this, newValue);

            _ = WindowBackdrop.RemoveTitlebarBackground(this);
        }
    }

    /// <summary>
    /// Private <see cref="ExtendsContentIntoTitleBar"/> property callback.
    /// </summary>
    private static void OnExtendsContentIntoTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ShellWindow window)
        {
            return;
        }

        if (e.OldValue == e.NewValue)
        {
            return;
        }

        window.OnExtendsContentIntoTitleBarChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    /// <summary>
    /// This virtual method is called when <see cref="ExtendsContentIntoTitleBar"/> is changed.
    /// </summary>
    protected virtual void OnExtendsContentIntoTitleBarChanged(bool oldValue, bool newValue)
    {
        // AllowsTransparency = true;
        SetCurrentValue(WindowStyleProperty, WindowStyle.SingleBorderWindow);

        WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = default,
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                UseAeroCaptionButtons = false,
            }
        );

        // WindowStyleProperty.OverrideMetadata(typeof(FluentWindow), new FrameworkPropertyMetadata(WindowStyle.SingleBorderWindow));
        // AllowsTransparencyProperty.OverrideMetadata(typeof(FluentWindow), new FrameworkPropertyMetadata(false));
        _ = UnsafeNativeMethods.RemoveWindowTitlebarContents(this);
    }
}

/// <summary>
/// Reflective access to `Wpf.Ui.Interop.UnsafeNativeMethods` methods.
/// </summary>
file static class UnsafeNativeMethods
{
    private static readonly Type? _interopType
        = Type.GetType("Wpf.Ui.Interop.UnsafeNativeMethods, Wpf.Ui");

    /// <summary>
    /// Tries to set the corner preference of the selected window.
    /// </summary>
    /// <param name="handle">Selected window handle.</param>
    /// <param name="cornerPreference">Window corner preference.</param>
    /// <returns><see langword="true"/> if invocation of native Windows function succeeds.</returns>
    public static bool ApplyWindowCornerPreference(nint handle, object cornerPreference)
    {
        if (_interopType == null) return false;

        MethodInfo? method = _interopType.GetMethod("ApplyWindowCornerPreference", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) return false;
        return (bool)method.Invoke(null, [handle, cornerPreference])!;
    }

    /// <summary>
    /// Tries to remove titlebar from selected <see cref="Window"/>.
    /// </summary>
    /// <param name="window">The window to which the effect is to be applied.</param>
    /// <returns><see langword="true"/> if invocation of native Windows function succeeds.</returns>
    public static bool RemoveWindowTitlebarContents(Window? window)
    {
        if (_interopType == null) return false;

        MethodInfo? method = _interopType.GetMethod(
            "RemoveWindowTitlebarContents",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            [typeof(Window)],
            null
        );
        if (method == null) return false;
        return (bool)method.Invoke(null, [window])!;
    }
}
