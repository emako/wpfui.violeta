using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = nameof(PART_CustomHeaderContentControl), Type = typeof(ContentControl))]
[TemplatePart(Name = nameof(PART_CenterContentPresenter), Type = typeof(ContentPresenter))]
[TemplatePart(Name = nameof(PART_CustomFooterContentControl), Type = typeof(ContentControl))]
[TemplatePart(Name = nameof(PART_CaptionButtonBar), Type = typeof(CaptionButtonBar))]
public partial class NativeLikeTitleBar : ContentControl
{
    static NativeLikeTitleBar()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(NativeLikeTitleBar), new FrameworkPropertyMetadata(typeof(NativeLikeTitleBar)));

    public NativeLikeTitleBar()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public event EventHandler? BackButtonClick;

    public event EventHandler? PaneToggleButtonClick;

    public event EventHandler? MinimizeButtonClick;

    public event EventHandler? MaximizeButtonClick;

    public event EventHandler? CloseButtonClick;

    public event EventHandler? HelpButtonClick;

    public static readonly DependencyProperty BackButtonCommandProperty =
    DependencyProperty.Register(
        nameof(BackButtonCommand),
        typeof(ICommand),
        typeof(NativeLikeTitleBar),
        new PropertyMetadata(null)
    );

    public ICommand? BackButtonCommand
    {
        get => (ICommand?)GetValue(BackButtonCommandProperty);
        set => SetValue(BackButtonCommandProperty, value);
    }

    public static readonly DependencyProperty PaneToggleButtonCommandProperty =
        DependencyProperty.Register(
            nameof(PaneToggleButtonCommand),
            typeof(ICommand),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(null)
        );

    public ICommand? PaneToggleButtonCommand
    {
        get => (ICommand?)GetValue(PaneToggleButtonCommandProperty);
        set => SetValue(PaneToggleButtonCommandProperty, value);
    }

    public static readonly DependencyProperty MinimizeButtonCommandProperty =
        DependencyProperty.Register(
            nameof(MinimizeButtonCommand),
            typeof(ICommand),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(null)
        );

    public ICommand? MinimizeButtonCommand
    {
        get => (ICommand?)GetValue(MinimizeButtonCommandProperty);
        set => SetValue(MinimizeButtonCommandProperty, value);
    }

    public static readonly DependencyProperty MaximizeButtonCommandProperty =
        DependencyProperty.Register(
            nameof(MaximizeButtonCommand),
            typeof(ICommand),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(null)
        );

    public ICommand? MaximizeButtonCommand
    {
        get => (ICommand?)GetValue(MaximizeButtonCommandProperty);
        set => SetValue(MaximizeButtonCommandProperty, value);
    }

    public static readonly DependencyProperty CloseButtonCommandProperty =
        DependencyProperty.Register(
            nameof(CloseButtonCommand),
            typeof(ICommand),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(null)
        );

    public ICommand? CloseButtonCommand
    {
        get => (ICommand?)GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    public static readonly DependencyProperty HelpButtonCommandProperty =
        DependencyProperty.Register(
            nameof(HelpButtonCommand),
            typeof(ICommand),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(null)
        );

    public ICommand? HelpButtonCommand
    {
        get => (ICommand?)GetValue(HelpButtonCommandProperty);
        set => SetValue(HelpButtonCommandProperty, value);
    }

    public static readonly DependencyProperty BackButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(BackButtonVisibility),
            typeof(Visibility),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(Visibility.Visible)
        );

    public Visibility BackButtonVisibility
    {
        get => (Visibility)GetValue(BackButtonVisibilityProperty);
        set => SetValue(BackButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty PaneToggleButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(PaneToggleButtonVisibility),
            typeof(Visibility),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(Visibility.Visible)
        );

    public Visibility PaneToggleButtonVisibility
    {
        get => (Visibility)GetValue(PaneToggleButtonVisibilityProperty);
        set => SetValue(PaneToggleButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MinimizeButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MinimizeButtonVisibility),
            typeof(Visibility),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(Visibility.Visible)
        );

    public Visibility MinimizeButtonVisibility
    {
        get => (Visibility)GetValue(MinimizeButtonVisibilityProperty);
        set => SetValue(MinimizeButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MaximizeButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MaximizeButtonVisibility),
            typeof(Visibility),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(Visibility.Visible)
        );

    public Visibility MaximizeButtonVisibility
    {
        get => (Visibility)GetValue(MaximizeButtonVisibilityProperty);
        set => SetValue(MaximizeButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty CloseButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(CloseButtonVisibility),
            typeof(Visibility),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(Visibility.Visible)
        );

    public Visibility CloseButtonVisibility
    {
        get => (Visibility)GetValue(CloseButtonVisibilityProperty);
        set => SetValue(CloseButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty HelpButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(HelpButtonVisibility),
            typeof(Visibility),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(Visibility.Collapsed)
        );

    public Visibility HelpButtonVisibility
    {
        get => (Visibility)GetValue(HelpButtonVisibilityProperty);
        set => SetValue(HelpButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty IsBackButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsBackButtonEnabled),
            typeof(bool),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(true)
        );

    public bool IsBackButtonEnabled
    {
        get => (bool)GetValue(IsBackButtonEnabledProperty);
        set => SetValue(IsBackButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsPaneToggleButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsPaneToggleButtonEnabled),
            typeof(bool),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(true)
        );

    public bool IsPaneToggleButtonEnabled
    {
        get => (bool)GetValue(IsPaneToggleButtonEnabledProperty);
        set => SetValue(IsPaneToggleButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsMinimizeButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMinimizeButtonEnabled),
            typeof(bool),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(true)
        );

    public bool IsMinimizeButtonEnabled
    {
        get => (bool)GetValue(IsMinimizeButtonEnabledProperty);
        set => SetValue(IsMinimizeButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsMaximizeButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMaximizeButtonEnabled),
            typeof(bool),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(true)
        );

    public bool IsMaximizeButtonEnabled
    {
        get => (bool)GetValue(IsMaximizeButtonEnabledProperty);
        set => SetValue(IsMaximizeButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsCloseButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsCloseButtonEnabled),
            typeof(bool),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(true)
        );

    public bool IsCloseButtonEnabled
    {
        get => (bool)GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsHelpButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsHelpButtonEnabled),
            typeof(bool),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(true)
        );

    public bool IsHelpButtonEnabled
    {
        get => (bool)GetValue(IsHelpButtonEnabledProperty);
        set => SetValue(IsHelpButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(true)
        );

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly DependencyProperty CustomHeaderProperty =
        DependencyProperty.Register(
            nameof(CustomHeader),
            typeof(object),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(null)
        );

    public object? CustomHeader
    {
        get => GetValue(CustomHeaderProperty);
        set => SetValue(CustomHeaderProperty, value);
    }

    public static readonly DependencyProperty CustomFooterProperty =
        DependencyProperty.Register(
            nameof(CustomFooter),
            typeof(object),
            typeof(NativeLikeTitleBar),
            new PropertyMetadata(null)
        );

    public object? CustomFooter
    {
        get => GetValue(CustomFooterProperty);
        set => SetValue(CustomFooterProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_CustomHeaderContentControl = (ContentControl)GetTemplateChild(nameof(PART_CustomHeaderContentControl));
        PART_CenterContentPresenter = (ContentPresenter)GetTemplateChild(nameof(PART_CenterContentPresenter));
        PART_CustomFooterContentControl = (ContentControl)GetTemplateChild(nameof(PART_CustomFooterContentControl));

        PART_BackButton = (TitleBarButton)GetTemplateChild(nameof(PART_BackButton));
        PART_PaneToggleButton = (TitleBarButton)GetTemplateChild(nameof(PART_PaneToggleButton));
        PART_CaptionButtonBar = (CaptionButtonBar)GetTemplateChild(nameof(PART_CaptionButtonBar));

        PART_BackButton.Click += OnBackButtonClick;
        PART_PaneToggleButton.Click += OnPaneToggleButtonClick;
        PART_CaptionButtonBar.MinimizeButtonClick += OnMinimizeButtonClick;
        PART_CaptionButtonBar.MaximizeButtonClick += OnMaximizeButtonClick;
        PART_CaptionButtonBar.CloseButtonClick += OnCloseButtonClick;
        PART_CaptionButtonBar.HelpButtonClick += OnHelpButtonClick;
    }

    private void OnHelpButtonClick(object? sender, EventArgs e)
    {
        HelpButtonClick?.Invoke(this, e);
    }

    private void OnMinimizeButtonClick(object? sender, EventArgs e)
    {
        MinimizeButtonClick?.Invoke(this, e);
    }

    private void OnMaximizeButtonClick(object? sender, EventArgs e)
    {
        MaximizeButtonClick?.Invoke(this, e);
    }

    private void OnCloseButtonClick(object? sender, EventArgs e)
    {
        CloseButtonClick?.Invoke(this, e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _ownerWindow = Window.GetWindow(this);
        _ownerWindow.Activated += OnOwnerWindowActivated;
        _ownerWindow.Deactivated += OnOwnerWindowDeactivated;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _ownerWindow.Activated -= OnOwnerWindowActivated;
        _ownerWindow.Deactivated -= OnOwnerWindowDeactivated;
    }

    private void OnOwnerWindowActivated(object? sender, EventArgs e)
    {
        IsActive = true;
    }

    private void OnOwnerWindowDeactivated(object? sender, EventArgs e)
    {
        IsActive = false;
    }

    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        BackButtonClick?.Invoke(this, e);
    }

    private void OnPaneToggleButtonClick(object sender, RoutedEventArgs e)
    {
        PaneToggleButtonClick?.Invoke(this, e);
    }

    private Window _ownerWindow;

    private ContentControl PART_CustomHeaderContentControl;
    private ContentPresenter PART_CenterContentPresenter;
    private ContentControl PART_CustomFooterContentControl;

    private TitleBarButton PART_BackButton;
    private TitleBarButton PART_PaneToggleButton;
    private CaptionButtonBar PART_CaptionButtonBar;
}
