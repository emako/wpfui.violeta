using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// CaptionButtonBar 会自行根据所在窗口是否在前台改变样式，按钮也会操作所在窗口的状态。
/// </summary>
[TemplatePart(Name = nameof(MinimizeButton), Type = typeof(CaptionMinimizeButton))]
[TemplatePart(Name = nameof(MaximizeButton), Type = typeof(CaptionMaximizeButton))]
[TemplatePart(Name = nameof(CloseButton), Type = typeof(CaptionCloseButton))]
[TemplatePart(Name = nameof(HelpButton), Type = typeof(CaptionHelpButton))]
public partial class CaptionButtonBar : Control
{
    static CaptionButtonBar()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionButtonBar), new FrameworkPropertyMetadata(typeof(CaptionButtonBar)));

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public CaptionButtonBar()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public CaptionMinimizeButton MinimizeButton { get; private set; }
    public CaptionMaximizeButton MaximizeButton { get; private set; }
    public CaptionCloseButton CloseButton { get; private set; }
    public CaptionHelpButton HelpButton { get; private set; }

    /// <summary>
    /// 用于触发切换最大化/还原按钮的触发器
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

    public event EventHandler? MinimizeButtonClick;

    public event EventHandler? MaximizeButtonClick;

    public event EventHandler? CloseButtonClick;

    public event EventHandler? HelpButtonClick;

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

        MinimizeButton = (CaptionMinimizeButton)GetTemplateChild(nameof(MinimizeButton));
        MaximizeButton = (CaptionMaximizeButton)GetTemplateChild(nameof(MaximizeButton));
        CloseButton = (CaptionCloseButton)GetTemplateChild(nameof(CloseButton));
        HelpButton = (CaptionHelpButton)GetTemplateChild(nameof(HelpButton));

        MinimizeButton.Click += OnMinimizeButtonClick;
        MaximizeButton.Click += OnMaximizeButtonClick;
        CloseButton.Click += OnCloseButtonClick;
        HelpButton.Click += OnHelpButtonClick;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _ownerWindow = Window.GetWindow(this);
        _ownerWindow.Activated += OnActivated;
        _ownerWindow.Deactivated += OnDeactivated;
        _ownerWindow.StateChanged += OnOwnerWindowStateChanged;

        _ownerHwndSource = HwndSource.FromHwnd(new WindowInteropHelper(_ownerWindow).Handle);
        _captionButtonHandler = new CaptionButtonHandler(_ownerHwndSource);
        _captionButtonHandler.Add(MinimizeButton);
        _captionButtonHandler.Add(MaximizeButton);
        _captionButtonHandler.Add(CloseButton);
        _captionButtonHandler.Add(HelpButton);

        nint hWnd = _ownerHwndSource.Handle;
        int style = User32.GetWindowLong(hWnd, User32.GWL_STYLE);
        style &= ~WS_SYSMENU;
        _ = User32.SetWindowLong(hWnd, User32.GWL_STYLE, style);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _ownerWindow.StateChanged -= OnOwnerWindowStateChanged;
        _ownerWindow.Activated -= OnActivated;
        _ownerWindow.StateChanged -= OnOwnerWindowStateChanged;
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

    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
    {
        MinimizeButtonClick?.Invoke(this, e);
        _ownerWindow.WindowState = WindowState.Minimized;
    }

    private void OnMaximizeButtonClick(object sender, RoutedEventArgs e)
    {
        MaximizeButtonClick?.Invoke(this, e);
        if (_ownerWindow.WindowState is WindowState.Maximized)
        {
            _ownerWindow.WindowState = WindowState.Normal;
        }
        else
        {
            _ownerWindow.WindowState = WindowState.Maximized;
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        CloseButtonClick?.Invoke(this, e);
        _ownerWindow.Close();
    }

    private void OnHelpButtonClick(object sender, RoutedEventArgs e)
    {
        HelpButtonClick?.Invoke(this, EventArgs.Empty);
    }

    private Window _ownerWindow;
    private HwndSource _ownerHwndSource;
    private CaptionButtonHandler _captionButtonHandler;

    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_SYSMENU = 0x00080000;
}
