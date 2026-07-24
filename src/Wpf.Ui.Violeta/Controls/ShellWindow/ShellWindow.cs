using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;

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
        typeof(WindowBackdropPreference),
        typeof(ShellWindow),
        new PropertyMetadata(WindowBackdropPreference.None, OnWindowBackdropTypeChanged)
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
    public WindowBackdropPreference WindowBackdropType
    {
        get => (WindowBackdropPreference)GetValue(WindowBackdropTypeProperty);
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
            return;

        if (e.OldValue == e.NewValue)
            return;

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

        _ = CornerHelper.SetWindowCorner(InteropHelper.Handle, newValue);
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

        window.OnBackdropTypeChanged((WindowBackdropPreference)e.OldValue, (WindowBackdropPreference)e.NewValue);
    }

    /// <summary>
    /// This virtual method is called when <see cref="WindowBackdropType"/> is changed.
    /// </summary>
    protected virtual void OnBackdropTypeChanged(WindowBackdropPreference oldValue, WindowBackdropPreference newValue)
    {
        if (InteropHelper.Handle == IntPtr.Zero)
        {
            return;
        }

        _ = WindowBackdrop.SetWindowChrome(this, newValue);

        if (newValue == WindowBackdropPreference.None)
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
            return;

        if (e.OldValue == e.NewValue)
            return;

        window.OnExtendsContentIntoTitleBarChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    /// <summary>
    /// This virtual method is called when <see cref="ExtendsContentIntoTitleBar"/> is changed.
    /// </summary>
    protected virtual void OnExtendsContentIntoTitleBarChanged(bool oldValue, bool newValue)
    {
        SetCurrentValue(WindowStyleProperty, WindowStyle.SingleBorderWindow);
        _ = WindowBackdrop.RemoveWindowTitlebarContents(this);
    }
}
