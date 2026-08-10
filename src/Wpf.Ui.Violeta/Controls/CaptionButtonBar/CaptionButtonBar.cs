using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// CaptionButtonBar will automatically change its style based on
/// whether the window is in the front end, and the buttons will
/// also control the status of the window.
/// </summary>
[TemplatePart(Name = nameof(MoreButton), Type = typeof(CaptionMoreButton))]
[TemplatePart(Name = nameof(HelpButton), Type = typeof(CaptionHelpButton))]
[TemplatePart(Name = nameof(MinimizeButton), Type = typeof(CaptionMinimizeButton))]
[TemplatePart(Name = nameof(MaximizeButton), Type = typeof(CaptionMaximizeButton))]
[TemplatePart(Name = nameof(CloseButton), Type = typeof(CaptionCloseButton))]
public partial class CaptionButtonBar : Control
{
    static CaptionButtonBar()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionButtonBar), new FrameworkPropertyMetadata(typeof(CaptionButtonBar)));

    public CaptionButtonBar()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public CaptionMoreButton? MoreButton { get; private set; }
    public CaptionHelpButton? HelpButton { get; private set; }
    public CaptionMinimizeButton? MinimizeButton { get; private set; }
    public CaptionMaximizeButton? MaximizeButton { get; private set; }
    public CaptionCloseButton? CloseButton { get; private set; }

    /// <summary>
    /// A trigger used to trigger the maximized/restore switch button
    /// </summary>
    public WindowState OwnerWindowState
    {
        get => (WindowState)GetValue(OwnerWindowStateProperty);
        private set => SetValue(OwnerWindowStateProperty, value);
    }

    public static readonly DependencyProperty OwnerWindowStateProperty =
    DependencyProperty.Register(
        nameof(OwnerWindowState),
        typeof(WindowState),
        typeof(CaptionButtonBar),
        new PropertyMetadata(WindowState.Normal));

    public event EventHandler? MoreButtonClick;

    public event EventHandler? MinimizeButtonClick;

    public event EventHandler? MaximizeButtonClick;

    public event EventHandler? CloseButtonClick;

    public event EventHandler? HelpButtonClick;

    public static readonly DependencyProperty MoreButtonCommandProperty =
        DependencyProperty.Register(
            nameof(MoreButtonCommand),
            typeof(ICommand),
            typeof(CaptionButtonBar),
            new PropertyMetadata(null));

    public ICommand? MoreButtonCommand
    {
        get => (ICommand?)GetValue(MoreButtonCommandProperty);
        set => SetValue(MoreButtonCommandProperty, value);
    }

    public static readonly DependencyProperty MoreButtonContextMenuProperty =
        DependencyProperty.Register(
            nameof(MoreButtonContextMenu),
            typeof(ContextMenu),
            typeof(CaptionButtonBar),
            new PropertyMetadata(null, OnMoreButtonContextMenuChanged));

    public ContextMenu? MoreButtonContextMenu
    {
        get => (ContextMenu?)GetValue(MoreButtonContextMenuProperty);
        set => SetValue(MoreButtonContextMenuProperty, value);
    }

    public static readonly DependencyProperty MinimizeButtonCommandProperty =
    DependencyProperty.Register(
        nameof(MinimizeButtonCommand),
        typeof(ICommand),
        typeof(CaptionButtonBar),
        new PropertyMetadata(null));

    public ICommand? MinimizeButtonCommand
    {
        get => (ICommand?)GetValue(MinimizeButtonCommandProperty);
        set => SetValue(MinimizeButtonCommandProperty, value);
    }

    public static readonly DependencyProperty MaximizeButtonCommandProperty =
        DependencyProperty.Register(
            nameof(MaximizeButtonCommand),
            typeof(ICommand),
            typeof(CaptionButtonBar),
            new PropertyMetadata(null));

    public ICommand? MaximizeButtonCommand
    {
        get => (ICommand?)GetValue(MaximizeButtonCommandProperty);
        set => SetValue(MaximizeButtonCommandProperty, value);
    }

    public static readonly DependencyProperty CloseButtonCommandProperty =
        DependencyProperty.Register(
            nameof(CloseButtonCommand),
            typeof(ICommand),
            typeof(CaptionButtonBar),
            new PropertyMetadata(null));

    public ICommand? CloseButtonCommand
    {
        get => (ICommand?)GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    public static readonly DependencyProperty HelpButtonCommandProperty =
        DependencyProperty.Register(
            nameof(HelpButtonCommand),
            typeof(ICommand),
            typeof(CaptionButtonBar),
            new PropertyMetadata(null));

    public ICommand? HelpButtonCommand
    {
        get => (ICommand?)GetValue(HelpButtonCommandProperty);
        set => SetValue(HelpButtonCommandProperty, value);
    }

    // ===== Visibility =====

    public static readonly DependencyProperty MoreButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MoreButtonVisibility),
            typeof(Visibility),
            typeof(CaptionButtonBar),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility MoreButtonVisibility
    {
        get => (Visibility)GetValue(MoreButtonVisibilityProperty);
        set => SetValue(MoreButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MinimizeButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MinimizeButtonVisibility),
            typeof(Visibility),
            typeof(CaptionButtonBar),
            new PropertyMetadata(Visibility.Visible));

    public Visibility MinimizeButtonVisibility
    {
        get => (Visibility)GetValue(MinimizeButtonVisibilityProperty);
        set => SetValue(MinimizeButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MaximizeButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MaximizeButtonVisibility),
            typeof(Visibility),
            typeof(CaptionButtonBar),
            new PropertyMetadata(Visibility.Visible));

    public Visibility MaximizeButtonVisibility
    {
        get => (Visibility)GetValue(MaximizeButtonVisibilityProperty);
        set => SetValue(MaximizeButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty CloseButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(CloseButtonVisibility),
            typeof(Visibility),
            typeof(CaptionButtonBar),
            new PropertyMetadata(Visibility.Visible));

    public Visibility CloseButtonVisibility
    {
        get => (Visibility)GetValue(CloseButtonVisibilityProperty);
        set => SetValue(CloseButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty HelpButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(HelpButtonVisibility),
            typeof(Visibility),
            typeof(CaptionButtonBar),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility HelpButtonVisibility
    {
        get => (Visibility)GetValue(HelpButtonVisibilityProperty);
        set => SetValue(HelpButtonVisibilityProperty, value);
    }

    // ===== IsEnabled =====

    public static readonly DependencyProperty IsMoreButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMoreButtonEnabled),
            typeof(bool),
            typeof(CaptionButtonBar),
            new PropertyMetadata(true));

    public bool IsMoreButtonEnabled
    {
        get => (bool)GetValue(IsMoreButtonEnabledProperty);
        set => SetValue(IsMoreButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsMinimizeButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMinimizeButtonEnabled),
            typeof(bool),
            typeof(CaptionButtonBar),
            new PropertyMetadata(true));

    public bool IsMinimizeButtonEnabled
    {
        get => (bool)GetValue(IsMinimizeButtonEnabledProperty);
        set => SetValue(IsMinimizeButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsMaximizeButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMaximizeButtonEnabled),
            typeof(bool),
            typeof(CaptionButtonBar),
            new PropertyMetadata(true));

    public bool IsMaximizeButtonEnabled
    {
        get => (bool)GetValue(IsMaximizeButtonEnabledProperty);
        set => SetValue(IsMaximizeButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsCloseButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsCloseButtonEnabled),
            typeof(bool),
            typeof(CaptionButtonBar),
            new PropertyMetadata(true));

    public bool IsCloseButtonEnabled
    {
        get => (bool)GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsHelpButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsHelpButtonEnabled),
            typeof(bool),
            typeof(CaptionButtonBar),
            new PropertyMetadata(true));

    public bool IsHelpButtonEnabled
    {
        get => (bool)GetValue(IsHelpButtonEnabledProperty);
        set => SetValue(IsHelpButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(CaptionButtonBar),
            new PropertyMetadata(true));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (MoreButton is not null)
        {
            MoreButton.Click -= OnMoreButtonClick;
        }

        if (HelpButton is not null)
        {
            HelpButton.Click -= OnHelpButtonClick;
        }

        if (MinimizeButton is not null)
        {
            MinimizeButton.Click -= OnMinimizeButtonClick;
        }

        if (MaximizeButton is not null)
        {
            MaximizeButton.Click -= OnMaximizeButtonClick;
        }

        if (CloseButton is not null)
        {
            CloseButton.Click -= OnCloseButtonClick;
        }

        MoreButton = (CaptionMoreButton)GetTemplateChild(nameof(MoreButton));
        HelpButton = (CaptionHelpButton)GetTemplateChild(nameof(HelpButton));
        MinimizeButton = (CaptionMinimizeButton)GetTemplateChild(nameof(MinimizeButton));
        MaximizeButton = (CaptionMaximizeButton)GetTemplateChild(nameof(MaximizeButton));
        CloseButton = (CaptionCloseButton)GetTemplateChild(nameof(CloseButton));

        MoreButton.Click += OnMoreButtonClick;
        HelpButton.Click += OnHelpButtonClick;
        MinimizeButton.Click += OnMinimizeButtonClick;
        MaximizeButton.Click += OnMaximizeButtonClick;
        CloseButton.Click += OnCloseButtonClick;

        ApplyMoreButtonContextMenu();
    }

    private static void OnMoreButtonContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CaptionButtonBar)d).ApplyMoreButtonContextMenu();
    }

    private void ApplyMoreButtonContextMenu()
    {
        if (MoreButton is null)
        {
            return;
        }

        MoreButton.ContextMenu = MoreButtonContextMenu;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _ownerWindow = Window.GetWindow(this);
        _ownerWindow.Activated += OnActivated;
        _ownerWindow.Deactivated += OnDeactivated;
        _ownerWindow.StateChanged += OnOwnerWindowStateChanged;

        _ownerHwndSource = HwndSource.FromHwnd(new WindowInteropHelper(_ownerWindow).Handle);
        _captionButtonHandler = new CaptionButtonHandler(_ownerHwndSource);
        _captionButtonHandler.Add(MoreButton!);
        _captionButtonHandler.Add(HelpButton!);
        _captionButtonHandler.Add(MinimizeButton!);
        _captionButtonHandler.Add(MaximizeButton!);
        _captionButtonHandler.Add(CloseButton!);

        nint hWnd = _ownerHwndSource.Handle;
        int style = User32.GetWindowLong(hWnd, User32.GWL_STYLE);
        style &= ~User32.WS_SYSMENU;
        _ = User32.SetWindowLong(hWnd, User32.GWL_STYLE, style);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _ownerWindow.StateChanged -= OnOwnerWindowStateChanged;
        _ownerWindow.Activated -= OnActivated;
        _ownerWindow.Deactivated -= OnDeactivated;
        _ownerWindow = null!;
    }

    private void OnOwnerWindowStateChanged(object? sender, EventArgs e)
    {
        OwnerWindowState = _ownerWindow.WindowState;
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        IsActive = true;
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        IsActive = false;
    }

    private void OnMoreButtonClick(object? sender, RoutedEventArgs e)
    {
        MoreButtonClick?.Invoke(this, e);

        if (MoreButtonContextMenu is { } menu && MoreButton is not null)
        {
            menu.PlacementTarget = MoreButton;
            menu.Placement = PlacementMode.Bottom;
            menu.HorizontalOffset = 0;
            menu.IsOpen = true;
        }
    }

    private void OnMinimizeButtonClick(object? sender, RoutedEventArgs e)
    {
        MinimizeButtonClick?.Invoke(this, e);
        SystemCommands.MinimizeWindow(_ownerWindow);
    }

    private void OnMaximizeButtonClick(object? sender, RoutedEventArgs e)
    {
        MaximizeButtonClick?.Invoke(this, e);
        if (_ownerWindow.WindowState is WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(_ownerWindow);
        }
        else
        {
            SystemCommands.MaximizeWindow(_ownerWindow);
        }
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseButtonClick?.Invoke(this, e);
        _ownerWindow.Close();
    }

    private void OnHelpButtonClick(object? sender, RoutedEventArgs e)
    {
        HelpButtonClick?.Invoke(this, EventArgs.Empty);
    }

    private Window _ownerWindow = null!;
    private HwndSource _ownerHwndSource = null!;
    private CaptionButtonHandler _captionButtonHandler = null!;
}
