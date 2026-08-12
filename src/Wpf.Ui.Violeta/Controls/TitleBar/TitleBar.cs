using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = nameof(PART_Icon), Type = typeof(Image))]
[TemplatePart(Name = nameof(PART_Title), Type = typeof(TextBlock))]
[TemplatePart(Name = nameof(PART_CustomHeaderContentControl), Type = typeof(ContentControl))]
[TemplatePart(Name = nameof(PART_CenterContentPresenter), Type = typeof(ContentPresenter))]
[TemplatePart(Name = nameof(PART_CustomFooterContentControl), Type = typeof(ContentControl))]
[TemplatePart(Name = nameof(PART_CaptionButtonBar), Type = typeof(CaptionButtonBar))]
public partial class TitleBar : ContentControl
{
    static TitleBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBar), new FrameworkPropertyMetadata(typeof(TitleBar)));
    }

    public TitleBar()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public event EventHandler? MoreButtonClick;

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
        typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
            new PropertyMetadata(null)
        );

    public ICommand? HelpButtonCommand
    {
        get => (ICommand?)GetValue(HelpButtonCommandProperty);
        set => SetValue(HelpButtonCommandProperty, value);
    }

    public static readonly DependencyProperty MoreButtonCommandProperty =
        DependencyProperty.Register(
            nameof(MoreButtonCommand),
            typeof(ICommand),
            typeof(TitleBar),
            new PropertyMetadata(null)
        );

    public ICommand? MoreButtonCommand
    {
        get => (ICommand?)GetValue(MoreButtonCommandProperty);
        set => SetValue(MoreButtonCommandProperty, value);
    }

    public static readonly DependencyProperty MoreButtonContextMenuProperty =
        DependencyProperty.Register(
            nameof(MoreButtonContextMenu),
            typeof(ContextMenu),
            typeof(TitleBar),
            new PropertyMetadata(null)
        );

    public ContextMenu? MoreButtonContextMenu
    {
        get => (ContextMenu?)GetValue(MoreButtonContextMenuProperty);
        set => SetValue(MoreButtonContextMenuProperty, value);
    }

    public static readonly DependencyProperty BackButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(BackButtonVisibility),
            typeof(Visibility),
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
            new PropertyMetadata(Visibility.Collapsed)
        );

    public Visibility HelpButtonVisibility
    {
        get => (Visibility)GetValue(HelpButtonVisibilityProperty);
        set => SetValue(HelpButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MoreButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MoreButtonVisibility),
            typeof(Visibility),
            typeof(TitleBar),
            new PropertyMetadata(Visibility.Collapsed)
        );

    public Visibility MoreButtonVisibility
    {
        get => (Visibility)GetValue(MoreButtonVisibilityProperty);
        set => SetValue(MoreButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty IsBackButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsBackButtonEnabled),
            typeof(bool),
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
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
            typeof(TitleBar),
            new PropertyMetadata(true)
        );

    public bool IsHelpButtonEnabled
    {
        get => (bool)GetValue(IsHelpButtonEnabledProperty);
        set => SetValue(IsHelpButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsMoreButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMoreButtonEnabled),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true)
        );

    public bool IsMoreButtonEnabled
    {
        get => (bool)GetValue(IsMoreButtonEnabledProperty);
        set => SetValue(IsMoreButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true)
        );

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the title bar dims when the owner window is inactive.
    /// Default is <c>true</c>.
    /// </summary>
    public static readonly DependencyProperty IsInactiveAppearanceEnabledProperty =
        DependencyProperty.Register(
            nameof(IsInactiveAppearanceEnabled),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true, OnIsInactiveAppearanceEnabledChanged)
        );

    public bool IsInactiveAppearanceEnabled
    {
        get => (bool)GetValue(IsInactiveAppearanceEnabledProperty);
        set => SetValue(IsInactiveAppearanceEnabledProperty, value);
    }

    private static void OnIsInactiveAppearanceEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TitleBar)d).UpdateIsActiveFromOwnerWindow();
    }

    public static readonly DependencyProperty CustomHeaderProperty =
        DependencyProperty.Register(
            nameof(CustomHeader),
            typeof(object),
            typeof(TitleBar),
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
            typeof(TitleBar),
            new PropertyMetadata(null)
        );

    public object? CustomFooter
    {
        get => GetValue(CustomFooterProperty);
        set => SetValue(CustomFooterProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(ImageSource),
            typeof(TitleBar),
            new PropertyMetadata(null)
        );

    /// <summary>
    /// Gets or sets the icon displayed in the title bar (typically 16×16).
    /// </summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IsIconVisibleProperty =
        DependencyProperty.Register(
            nameof(IsIconVisible),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the title bar icon is visible when <see cref="Icon"/> is set.
    /// </summary>
    public bool IsIconVisible
    {
        get => (bool)GetValue(IsIconVisibleProperty);
        set => SetValue(IsIconVisibleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(TitleBar),
            new PropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Gets or sets the title text displayed in the title bar.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty IsTitleVisibleProperty =
        DependencyProperty.Register(
            nameof(IsTitleVisible),
            typeof(bool),
            typeof(TitleBar),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the title text is visible.
    /// </summary>
    public bool IsTitleVisible
    {
        get => (bool)GetValue(IsTitleVisibleProperty);
        set => SetValue(IsTitleVisibleProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_Icon = GetTemplateChild(nameof(PART_Icon)) as Image;
        PART_Title = GetTemplateChild(nameof(PART_Title)) as TextBlock;
        PART_CustomHeaderContentControl = (ContentControl)GetTemplateChild(nameof(PART_CustomHeaderContentControl));
        PART_CenterContentPresenter = (ContentPresenter)GetTemplateChild(nameof(PART_CenterContentPresenter));
        PART_CustomFooterContentControl = (ContentControl)GetTemplateChild(nameof(PART_CustomFooterContentControl));

        PART_BackButton = (TitleBarButton)GetTemplateChild(nameof(PART_BackButton));
        PART_PaneToggleButton = (TitleBarButton)GetTemplateChild(nameof(PART_PaneToggleButton));
        PART_CaptionButtonBar = (CaptionButtonBar)GetTemplateChild(nameof(PART_CaptionButtonBar));

        PART_BackButton.Click += OnBackButtonClick;
        PART_PaneToggleButton.Click += OnPaneToggleButtonClick;
        PART_CaptionButtonBar.MoreButtonClick += OnMoreButtonClick;
        PART_CaptionButtonBar.MinimizeButtonClick += OnMinimizeButtonClick;
        PART_CaptionButtonBar.MaximizeButtonClick += OnMaximizeButtonClick;
        PART_CaptionButtonBar.CloseButtonClick += OnCloseButtonClick;
        PART_CaptionButtonBar.HelpButtonClick += OnHelpButtonClick;
    }

    private void OnMoreButtonClick(object? sender, EventArgs e)
    {
        MoreButtonClick?.Invoke(this, e);
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

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _ownerWindow = Window.GetWindow(this);
        _ownerWindow.Activated += OnOwnerWindowActivated;
        _ownerWindow.Deactivated += OnOwnerWindowDeactivated;
        UpdateIsActiveFromOwnerWindow();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _ownerWindow.Activated -= OnOwnerWindowActivated;
        _ownerWindow.Deactivated -= OnOwnerWindowDeactivated;
    }

    private void OnOwnerWindowActivated(object? sender, EventArgs e)
    {
        UpdateIsActiveFromOwnerWindow();
    }

    private void OnOwnerWindowDeactivated(object? sender, EventArgs e)
    {
        UpdateIsActiveFromOwnerWindow();
    }

    private void UpdateIsActiveFromOwnerWindow()
    {
        if (!IsInactiveAppearanceEnabled)
        {
            IsActive = true;
            return;
        }

        IsActive = _ownerWindow?.IsActive ?? true;
    }

    private void OnBackButtonClick(object? sender, RoutedEventArgs e)
    {
        BackButtonClick?.Invoke(this, e);
    }

    private void OnPaneToggleButtonClick(object? sender, RoutedEventArgs e)
    {
        PaneToggleButtonClick?.Invoke(this, e);
    }

    private Window _ownerWindow = null!;

    private Image? PART_Icon;
    private TextBlock? PART_Title;
    private ContentControl PART_CustomHeaderContentControl = null!;
    private ContentPresenter PART_CenterContentPresenter = null!;
    private ContentControl PART_CustomFooterContentControl = null!;

    private TitleBarButton PART_BackButton = null!;
    private TitleBarButton PART_PaneToggleButton = null!;
    private CaptionButtonBar PART_CaptionButtonBar = null!;
}
