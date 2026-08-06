using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A horizontal toolbar that moves overflowing children into a Compat <see cref="Flyout"/> instead of a Menu.
/// Child instances are reparented visually as-is (not converted to <see cref="MenuItem"/>).
/// </summary>
[TemplatePart(Name = PartToolBarPanel, Type = typeof(ToolBarPanel))]
[TemplatePart(Name = PartOverflowButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PartToolBarOverflowPanel, Type = typeof(ToolBarOverflowPanel))]
[StyleTypedProperty(Property = nameof(OverflowButtonStyle), StyleTargetType = typeof(ButtonBase))]
public class ToolBar : ItemsControl
{
    public const string PartToolBarPanel = "PART_ToolBarPanel";
    public const string PartOverflowButton = "PART_OverflowButton";
    public const string PartToolBarOverflowPanel = "PART_ToolBarOverflowPanel";

    private const string OverflowFlyoutPresenterStyleKey = "DefaultToolBarOverflowFlyoutPresenterStyle";

    private ToolBarPanel? _toolBarPanel;
    private ButtonBase? _overflowButton;
    private ToolBarOverflowPanel? _overflowPanel;
    private Flyout? _overflowFlyout;
    private bool _isUpdatingOverflowOpen;
    private bool _isRealizingContainers;
    private readonly RoutedEventHandler _overflowItemClickHandler;
    private readonly MouseButtonEventHandler _overflowItemMouseUpHandler;

    static ToolBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolBar), new FrameworkPropertyMetadata(typeof(ToolBar)));
    }

    public ToolBar()
    {
        _overflowItemClickHandler = OnOverflowItemClick;
        _overflowItemMouseUpHandler = OnOverflowItemMouseUp;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
    }

    #region ShowOverflowMenu

    public static readonly DependencyProperty ShowOverflowMenuProperty = DependencyProperty.Register(
        nameof(ShowOverflowMenu),
        typeof(bool),
        typeof(ToolBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnShowOverflowMenuChanged));

    /// <summary>
    /// When true (default), overflowing items are shown in a flyout reachable from the overflow button.
    /// When false, overflowing items are removed from the primary bar but no overflow UI is shown.
    /// </summary>
    public bool ShowOverflowMenu
    {
        get => (bool)GetValue(ShowOverflowMenuProperty);
        set => SetValue(ShowOverflowMenuProperty, value);
    }

    private static void OnShowOverflowMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToolBar toolBar)
        {
            if (!(bool)e.NewValue)
            {
                toolBar.IsOverflowOpen = false;
            }

            toolBar._toolBarPanel?.InvalidateMeasure();
        }
    }

    #endregion ShowOverflowMenu

    #region OverflowFlyoutAutoCloseMode

    public static readonly DependencyProperty OverflowFlyoutAutoCloseModeProperty = DependencyProperty.Register(
        nameof(OverflowFlyoutAutoCloseMode),
        typeof(ToolBarOverflowFlyoutAutoCloseMode),
        typeof(ToolBar),
        new PropertyMetadata(ToolBarOverflowFlyoutAutoCloseMode.Default, OnOverflowFlyoutAutoCloseModeChanged));

    /// <summary>
    /// Controls when the overflow flyout auto-closes after interacting with overflow items.
    /// </summary>
    public ToolBarOverflowFlyoutAutoCloseMode OverflowFlyoutAutoCloseMode
    {
        get => (ToolBarOverflowFlyoutAutoCloseMode)GetValue(OverflowFlyoutAutoCloseModeProperty);
        set => SetValue(OverflowFlyoutAutoCloseModeProperty, value);
    }

    private static void OnOverflowFlyoutAutoCloseModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToolBar { IsOverflowOpen: true } toolBar)
        {
            toolBar.ApplyOverflowPopupStaysOpen();
        }
    }

    #endregion OverflowFlyoutAutoCloseMode

    #region HasOverflowItems

    private static readonly DependencyPropertyKey HasOverflowItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasOverflowItems),
            typeof(bool),
            typeof(ToolBar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasOverflowItemsProperty =
        HasOverflowItemsPropertyKey.DependencyProperty;

    /// <summary>True when at least one child is marked as an overflow item.</summary>
    public bool HasOverflowItems
    {
        get => (bool)GetValue(HasOverflowItemsProperty);
        private set => SetValue(HasOverflowItemsPropertyKey, value);
    }

    internal void SetHasOverflowItems(bool value) => HasOverflowItems = value;

    #endregion HasOverflowItems

    #region IsOverflowOpen

    public static readonly DependencyProperty IsOverflowOpenProperty = DependencyProperty.Register(
        nameof(IsOverflowOpen),
        typeof(bool),
        typeof(ToolBar),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnIsOverflowOpenChanged));

    /// <summary>Gets or sets whether the overflow flyout is open.</summary>
    public bool IsOverflowOpen
    {
        get => (bool)GetValue(IsOverflowOpenProperty);
        set => SetValue(IsOverflowOpenProperty, value);
    }

    private static void OnIsOverflowOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ToolBar)d).UpdateOverflowFlyoutState();
    }

    #endregion IsOverflowOpen

    #region OverflowPanelWrapWidth

    public static readonly DependencyProperty OverflowPanelWrapWidthProperty = DependencyProperty.Register(
        nameof(OverflowPanelWrapWidth),
        typeof(double),
        typeof(ToolBar),
        new FrameworkPropertyMetadata(0d, OnOverflowPanelWrapWidthChanged));

    /// <summary>
    /// Wrap width for the overflow panel. Values &lt;= 0 produce a single vertical column.
    /// </summary>
    public double OverflowPanelWrapWidth
    {
        get => (double)GetValue(OverflowPanelWrapWidthProperty);
        set => SetValue(OverflowPanelWrapWidthProperty, value);
    }

    private static void OnOverflowPanelWrapWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToolBar { _overflowPanel: { } panel })
        {
            panel.WrapWidth = (double)e.NewValue;
        }
    }

    #endregion OverflowPanelWrapWidth

    #region ItemSpacing

    public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.Register(
        nameof(ItemSpacing),
        typeof(double),
        typeof(ToolBar),
        new FrameworkPropertyMetadata(6d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnItemSpacingChanged));

    /// <summary>
    /// Gap between adjacent items in the primary bar and the overflow flyout.
    /// </summary>
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    private static void OnItemSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToolBar toolBar)
        {
            toolBar._toolBarPanel?.InvalidateMeasure();
            toolBar._overflowPanel?.InvalidateMeasure();
        }
    }

    #endregion ItemSpacing

    #region OverflowButtonStyle

    public static readonly DependencyProperty OverflowButtonStyleProperty = DependencyProperty.Register(
        nameof(OverflowButtonStyle),
        typeof(Style),
        typeof(ToolBar),
        new PropertyMetadata(null));

    public Style? OverflowButtonStyle
    {
        get => (Style?)GetValue(OverflowButtonStyleProperty);
        set => SetValue(OverflowButtonStyleProperty, value);
    }

    #endregion OverflowButtonStyle

    #region OverflowMode (attached)

    public static readonly DependencyProperty OverflowModeProperty = DependencyProperty.RegisterAttached(
        "OverflowMode",
        typeof(ToolBarOverflowMode),
        typeof(ToolBar),
        new FrameworkPropertyMetadata(ToolBarOverflowMode.AsNeeded, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    public static ToolBarOverflowMode GetOverflowMode(DependencyObject element)
    {
        _ = element ?? throw new ArgumentNullException(nameof(element));

        return (ToolBarOverflowMode)element.GetValue(OverflowModeProperty);
    }

    public static void SetOverflowMode(DependencyObject element, ToolBarOverflowMode value)
    {
        _ = element ?? throw new ArgumentNullException(nameof(element));

        element.SetValue(OverflowModeProperty, value);
    }

    #endregion OverflowMode (attached)

    #region IsOverflowItem (attached, read-only)

    private static readonly DependencyPropertyKey IsOverflowItemPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "IsOverflowItem",
            typeof(bool),
            typeof(ToolBar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsOverflowItemProperty =
        IsOverflowItemPropertyKey.DependencyProperty;

    public static bool GetIsOverflowItem(DependencyObject element)
    {
        _ = element ?? throw new ArgumentNullException(nameof(element));

        return (bool)element.GetValue(IsOverflowItemProperty);
    }

    internal static void SetIsOverflowItem(DependencyObject element, bool value)
    {
        element.SetValue(IsOverflowItemPropertyKey, value);
    }

    #endregion IsOverflowItem (attached, read-only)

    public override void OnApplyTemplate()
    {
        DetachOverflowButton();
        DetachOverflowPanelHandlers();
        DetachFlyout();

        base.OnApplyTemplate();

        _toolBarPanel = GetTemplateChild(PartToolBarPanel) as ToolBarPanel;
        _overflowButton = GetTemplateChild(PartOverflowButton) as ButtonBase;
        _overflowPanel = GetTemplateChild(PartToolBarOverflowPanel) as ToolBarOverflowPanel
            ?? _overflowPanel
            ?? new ToolBarOverflowPanel();

        _toolBarPanel?.ToolBar = this;
        _overflowPanel.ToolBar = this;
        _overflowPanel.WrapWidth = OverflowPanelWrapWidth;

        EnsureOverflowFlyout();
        AttachOverflowButton();
        AttachOverflowPanelHandlers();
        EnsureContainersRealized();
        UpdateOverflowFlyoutState();
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is FrameworkElement;

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        EnsureContainersRealized();
        _toolBarPanel?.InvalidateMeasure();
        _overflowPanel?.InvalidateMeasure();
    }

    /// <summary>
    /// Realizes item containers and parents unrealized ones onto the primary panel (visual tree only).
    /// </summary>
    internal void EnsureContainersRealized()
    {
        if (_isRealizingContainers || _toolBarPanel is null)
        {
            return;
        }

        _isRealizingContainers = true;
        try
        {
            var generator = (IItemContainerGenerator)ItemContainerGenerator;
            var keep = new HashSet<UIElement>();

            if (Items.Count > 0)
            {
                using (generator.StartAt(new GeneratorPosition(-1, 0), GeneratorDirection.Forward, true))
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (generator.GenerateNext(out bool newlyRealized) is not UIElement container)
                        {
                            continue;
                        }

                        if (newlyRealized)
                        {
                            generator.PrepareItemContainer(container);
                        }

                        keep.Add(container);

                        if (VisualTreeHelper.GetParent(container) is null)
                        {
                            _toolBarPanel.Children.Add(container);
                        }
                    }
                }
            }

            _toolBarPanel.RemoveOrphans(keep);
            _overflowPanel?.RemoveOrphans(keep);
        }
        finally
        {
            _isRealizingContainers = false;
        }
    }

    private void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            EnsureContainersRealized();
            _toolBarPanel?.InvalidateMeasure();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        EnsureOverflowFlyout();
        EnsureContainersRealized();
        _toolBarPanel?.InvalidateMeasure();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        IsOverflowOpen = false;
        DetachOverflowPanelHandlers();
        DetachFlyout();
    }

    private void EnsureOverflowFlyout()
    {
        if (_overflowPanel is null)
        {
            return;
        }

        if (_overflowFlyout is null)
        {
            _overflowFlyout = new Flyout
            {
                Placement = FlyoutPlacementMode.BottomEdgeAlignedRight,
                Content = _overflowPanel,
            };
            _overflowFlyout.Opened += OnOverflowFlyoutOpened;
            _overflowFlyout.Closed += OnOverflowFlyoutClosed;
        }
        else if (!ReferenceEquals(_overflowFlyout.Content, _overflowPanel))
        {
            _overflowFlyout.Content = _overflowPanel;
        }

        // Compat Flyout binds presenter Style to FlyoutPresenterStyle; a null binding clears DefaultStyleKey.
        _overflowFlyout.FlyoutPresenterStyle ??=
                TryFindResource(OverflowFlyoutPresenterStyleKey) as Style
                ?? CreateFallbackFlyoutPresenterStyle();
    }

    private static Style CreateFallbackFlyoutPresenterStyle()
    {
        var style = new Style(typeof(FlyoutPresenter));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4)));
        style.Setters.Add(new Setter(Control.BackgroundProperty, SystemColors.WindowBrush));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, SystemColors.WindowFrameBrush));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateFallbackFlyoutPresenterTemplate()));
        return style;
    }

    private static ControlTemplate CreateFallbackFlyoutPresenterTemplate()
    {
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Control.PaddingProperty));
        borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
        borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
        borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));

        var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
        borderFactory.AppendChild(contentFactory);

        return new ControlTemplate(typeof(FlyoutPresenter)) { VisualTree = borderFactory };
    }

    private void DetachFlyout()
    {
        if (_overflowFlyout is null)
        {
            return;
        }

        _overflowFlyout.Opened -= OnOverflowFlyoutOpened;
        _overflowFlyout.Closed -= OnOverflowFlyoutClosed;
        if (_overflowFlyout.IsOpen)
        {
            _overflowFlyout.Hide();
        }

        _overflowFlyout = null;
    }

    private void AttachOverflowPanelHandlers()
    {
        if (_overflowPanel is null)
        {
            return;
        }

        _overflowPanel.AddHandler(ButtonBase.ClickEvent, _overflowItemClickHandler, true);
        _overflowPanel.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, _overflowItemMouseUpHandler, true);
    }

    private void DetachOverflowPanelHandlers()
    {
        if (_overflowPanel is null)
        {
            return;
        }

        _overflowPanel.RemoveHandler(ButtonBase.ClickEvent, _overflowItemClickHandler);
        _overflowPanel.RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent, _overflowItemMouseUpHandler);
    }

    private void OnOverflowItemClick(object sender, RoutedEventArgs e)
    {
        if (OverflowFlyoutAutoCloseMode != ToolBarOverflowFlyoutAutoCloseMode.Default)
        {
            return;
        }

        TryAutoCloseOverflowFlyout(e.OriginalSource as DependencyObject, buttonBaseItemOnly: true);
    }

    private void OnOverflowItemMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (OverflowFlyoutAutoCloseMode != ToolBarOverflowFlyoutAutoCloseMode.Always)
        {
            return;
        }

        TryAutoCloseOverflowFlyout(e.OriginalSource as DependencyObject, buttonBaseItemOnly: false);
    }

    private void TryAutoCloseOverflowFlyout(DependencyObject? originalSource, bool buttonBaseItemOnly)
    {
        if (!IsOverflowOpen || _overflowPanel is null || originalSource is null)
        {
            return;
        }

        for (DependencyObject? current = originalSource;
             current is not null;
             current = current is Visual
                 ? VisualTreeHelper.GetParent(current)
                 : LogicalTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, _overflowPanel))
            {
                return;
            }

            if (current is UIElement element && Items.Contains(element))
            {
                if (!buttonBaseItemOnly || ToolBarOverflowFlyoutAutoCloseTypes.Matches(element))
                {
                    IsOverflowOpen = false;
                }

                return;
            }
        }
    }

    private void AttachOverflowButton()
    {
        if (_overflowButton is null)
        {
            return;
        }

        _overflowButton.Click -= OnOverflowButtonClick;
        _overflowButton.Click += OnOverflowButtonClick;
    }

    private void DetachOverflowButton()
    {
        _overflowButton?.Click -= OnOverflowButtonClick;
    }

    private void OnOverflowButtonClick(object sender, RoutedEventArgs e)
    {
        if (!ShowOverflowMenu || !HasOverflowItems)
        {
            return;
        }

        if (IsOverflowOpen || _overflowFlyout?.IsOpen == true)
        {
            IsOverflowOpen = false;
            return;
        }

        // Defer open so StaysOpen=false Popup is not dismissed by this same click.
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                if (IsLoaded && ShowOverflowMenu && HasOverflowItems)
                {
                    IsOverflowOpen = true;
                }
            }),
            DispatcherPriority.Input);
    }

    private void OnOverflowFlyoutOpened(object? sender, object e)
    {
        if (_isUpdatingOverflowOpen)
        {
            return;
        }

        _isUpdatingOverflowOpen = true;
        try
        {
            SetCurrentValue(IsOverflowOpenProperty, true);
        }
        finally
        {
            _isUpdatingOverflowOpen = false;
        }

        ApplyOverflowPopupStaysOpen();
    }

    private void OnOverflowFlyoutClosed(object? sender, object e)
    {
        if (_isUpdatingOverflowOpen)
        {
            return;
        }

        _isUpdatingOverflowOpen = true;
        try
        {
            SetCurrentValue(IsOverflowOpenProperty, false);
        }
        finally
        {
            _isUpdatingOverflowOpen = false;
        }
    }

    private void PrepareOverflowContent()
    {
        if (_overflowPanel is null)
        {
            return;
        }

        _overflowPanel.ToolBar = this;
        _overflowPanel.WrapWidth = OverflowPanelWrapWidth;
        _overflowPanel.InvalidateMeasure();
        _overflowPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _overflowPanel.Arrange(new Rect(_overflowPanel.DesiredSize));
    }

    private void ApplyOverflowPopupStaysOpen()
    {
        if (_overflowFlyout?.InternalPopup is not { } popup)
        {
            return;
        }

        // Match ComboBox dropdown: StaysOpen=false so clicking outside dismisses freely.
        // Never keeps the flyout until the overflow button / IsOverflowOpen closes it.
        popup.StaysOpen = OverflowFlyoutAutoCloseMode == ToolBarOverflowFlyoutAutoCloseMode.Never;
    }

    private void UpdateOverflowFlyoutState()
    {
        if (_isUpdatingOverflowOpen)
        {
            return;
        }

        EnsureOverflowFlyout();

        if (_overflowFlyout is null || _overflowButton is null)
        {
            return;
        }

        _isUpdatingOverflowOpen = true;
        try
        {
            if (IsOverflowOpen && ShowOverflowMenu && HasOverflowItems)
            {
                PrepareOverflowContent();
                if (!_overflowFlyout.IsOpen)
                {
                    _overflowFlyout.ShowAt(_overflowButton);
                }

                ApplyOverflowPopupStaysOpen();
            }
            else if (_overflowFlyout.IsOpen)
            {
                _overflowFlyout.Hide();
            }
        }
        finally
        {
            _isUpdatingOverflowOpen = false;
        }
    }
}
