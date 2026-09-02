using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button with a primary action area and a separate chevron that opens a flyout.
/// Primary click uses <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/>;
/// double-click on the primary area uses <see cref="DoubleCommand"/>.
/// Chevron uses a WinUI AnimatedChevronDownSmall-style clipped translate bounce.
/// </summary>
[TemplatePart(Name = TemplateElementToggle, Type = typeof(Border))]
[TemplatePart(Name = TemplateElementToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = ChevronHostPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = ChevronIconPart, Type = typeof(UIElement))]
public class SplitButton : Wpf.Ui.Controls.Button
{
    private const string TemplateElementToggle = "PART_Toggle";
    private const string TemplateElementToggleButton = "PART_ToggleButton";
    private const string ChevronHostPart = "PART_ChevronHost";
    private const string ChevronIconPart = "PART_ChevronIcon";
    private const string FlyoutContextMenuStyleKey = "DefaultDropDownFlyoutContextMenuStyle";

    /// <summary>
    /// Fraction of the clipped chevron viewport translated on press.
    /// Matched to <see cref="DropDownButton"/>.
    /// </summary>
    private const double PressDepthRatio = 0.18;

    /// <summary>Upward overshoot on release, relative to viewport height.</summary>
    private const double OvershootRatio = 0.10;

    private ContextMenu? _contextMenu;
    private Border? _splitButtonToggleBorder;
    private TranslateTransform? _chevronTranslate;
    private FrameworkElement? _chevronHost;
    private bool _dismissFlyoutOnClick;

    /// <summary>Gets or sets the control responsible for toggling the drop-down.</summary>
    protected ToggleButton? SplitButtonToggleButton { get; set; }

    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SplitButton),
            new FrameworkPropertyMetadata(typeof(SplitButton)));

        BackgroundProperty.OverrideMetadata(
            typeof(SplitButton),
            new FrameworkPropertyMetadata(OnChromeBackgroundChanged));
    }

    private static void OnChromeBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var splitButton = (SplitButton)d;
        splitButton.CoerceValue(MouseOverSecondaryBackgroundProperty);
        splitButton.CoerceValue(PressedSecondaryBackgroundProperty);
    }

    private static object? CoerceSecondaryBackground(DependencyObject d, object? baseValue) =>
        baseValue ?? ((SplitButton)d).Background;

    public SplitButton()
    {
        Unloaded += static (sender, _) => ((SplitButton)sender).ReleaseTemplateResources();
        Loaded += static (sender, _) =>
        {
            var self = (SplitButton)sender;
            if (self.SplitButtonToggleButton is not null)
            {
                self.AttachToggleButtonClick();
            }
        };
    }

    /// <summary>Identifies the <see cref="Flyout"/> dependency property.</summary>
    public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register(
        nameof(Flyout),
        typeof(object),
        typeof(SplitButton),
        new PropertyMetadata(null, OnFlyoutChanged));

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen),
        typeof(bool),
        typeof(SplitButton),
        new PropertyMetadata(false, OnIsDropDownOpenChanged));

    /// <summary>Identifies the <see cref="DoubleCommand"/> dependency property.</summary>
    public static readonly DependencyProperty DoubleCommandProperty = DependencyProperty.Register(
        nameof(DoubleCommand),
        typeof(ICommand),
        typeof(SplitButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="DoubleCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty DoubleCommandParameterProperty = DependencyProperty.Register(
        nameof(DoubleCommandParameter),
        typeof(object),
        typeof(SplitButton),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="MouseOverSecondaryBackground"/> dependency property.
    /// WinUI SplitButton unfocused segment fill during primary hover (e.g. SplitButtonInAppBarUnfocusedPointerOver).
    /// </summary>
    public static readonly DependencyProperty MouseOverSecondaryBackgroundProperty = DependencyProperty.Register(
        nameof(MouseOverSecondaryBackground),
        typeof(Brush),
        typeof(SplitButton),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            CoerceSecondaryBackground));

    /// <summary>
    /// Identifies the <see cref="PressedSecondaryBackground"/> dependency property.
    /// WinUI SplitButton unfocused segment fill during primary press.
    /// </summary>
    public static readonly DependencyProperty PressedSecondaryBackgroundProperty = DependencyProperty.Register(
        nameof(PressedSecondaryBackground),
        typeof(Brush),
        typeof(SplitButton),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            CoerceSecondaryBackground));

    /// <summary>Gets or sets the flyout associated with this button.</summary>
    [Bindable(true)]
    public object? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the drop-down is currently open.</summary>
    [Bindable(true)]
    [Browsable(false)]
    [Category("Appearance")]
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the command invoked when the primary content area is double-clicked.
    /// Single-click continues to use <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/>.
    /// </summary>
    [Bindable(true)]
    [Category("Action")]
    public ICommand? DoubleCommand
    {
        get => (ICommand?)GetValue(DoubleCommandProperty);
        set => SetValue(DoubleCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="DoubleCommand"/>.</summary>
    [Bindable(true)]
    [Category("Action")]
    public object? DoubleCommandParameter
    {
        get => GetValue(DoubleCommandParameterProperty);
        set => SetValue(DoubleCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush applied to the non-hovered segment while the other segment is hovered.
    /// When unset, falls back to <see cref="System.Windows.Controls.Control.Background"/> so appearance variants keep their base color.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush? MouseOverSecondaryBackground
    {
        get => (Brush?)GetValue(MouseOverSecondaryBackgroundProperty);
        set => SetValue(MouseOverSecondaryBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush applied to the non-pressed segment while the other segment is pressed.
    /// When unset, falls back to <see cref="System.Windows.Controls.Control.Background"/>.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public Brush? PressedSecondaryBackground
    {
        get => (Brush?)GetValue(PressedSecondaryBackgroundProperty);
        set => SetValue(PressedSecondaryBackgroundProperty, value);
    }

    private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SplitButton)d).OnFlyoutChanged(e.OldValue, e.NewValue);
    }

    /// <summary>Invoked when <see cref="Flyout"/> changes.</summary>
    protected virtual void OnFlyoutChanged(object? oldValue, object? newValue)
    {
        if (oldValue is ContextMenu oldMenu)
        {
            oldMenu.Opened -= OnContextMenuOpened;
            oldMenu.Closed -= OnContextMenuClosed;
        }

        _contextMenu = null;

        if (newValue is ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;
            ApplyFlyoutContextMenuStyle(contextMenu);
            contextMenu.Opened += OnContextMenuOpened;
            contextMenu.Closed += OnContextMenuClosed;
        }
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SplitButton)d).OnIsDropDownOpenChanged(e.NewValue is true);
    }

    protected virtual void OnContextMenuClosed(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    protected virtual void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    /// <summary>Invoked when <see cref="IsDropDownOpen"/> changes.</summary>
    protected virtual void OnIsDropDownOpenChanged(bool currentValue) { }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        ReleaseTemplateResources();

        base.OnApplyTemplate();

        SplitButtonToggleButton = GetTemplateChild(TemplateElementToggleButton) as ToggleButton
            ?? throw new NullReferenceException(
                $"Element {TemplateElementToggleButton} of type {typeof(ToggleButton)} not found in {typeof(SplitButton)}");

        _splitButtonToggleBorder = GetTemplateChild(TemplateElementToggle) as Border;
        _chevronHost =
            GetTemplateChild(ChevronHostPart) as FrameworkElement
            ?? SplitButtonToggleButton.Content as FrameworkElement;
        _chevronTranslate = null;

        UIElement? chevron = GetTemplateChild(ChevronIconPart) as UIElement;

        if (chevron is null && _chevronHost is Border { Child: UIElement borderChild })
        {
            chevron = borderChild;
        }
        else if (chevron is null && _chevronHost is Decorator { Child: UIElement decoratorChild })
        {
            chevron = decoratorChild;
        }

        if (chevron is not null)
        {
            // Template Freezables are immutable — always install a fresh transform.
            _chevronTranslate = new TranslateTransform();
            chevron.RenderTransform = _chevronTranslate;
            chevron.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        AttachToggleButtonClick();
    }

    /// <inheritdoc />
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (e.ChangedButton != MouseButton.Left || e.Handled)
        {
            return;
        }

        if (IsOverToggle(e.GetPosition(this)))
        {
            return;
        }

        var parameter = DoubleCommandParameter;
        var command = DoubleCommand;
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
            e.Handled = true;
        }
    }

    /// <summary>Releases template event handlers.</summary>
    protected virtual void ReleaseTemplateResources()
    {
        if (SplitButtonToggleButton is not null)
        {
            SplitButtonToggleButton.PreviewMouseLeftButtonDown -= OnTogglePreviewMouseLeftButtonDown;
            SplitButtonToggleButton.PreviewMouseLeftButtonUp -= OnTogglePreviewMouseLeftButtonUp;
            SplitButtonToggleButton.LostMouseCapture -= OnToggleLostMouseCapture;
        }

        _dismissFlyoutOnClick = false;
        _chevronTranslate = null;
        _chevronHost = null;
    }

    private void AttachToggleButtonClick()
    {
        if (SplitButtonToggleButton is null)
        {
            return;
        }

        SplitButtonToggleButton.PreviewMouseLeftButtonDown -= OnTogglePreviewMouseLeftButtonDown;
        SplitButtonToggleButton.PreviewMouseLeftButtonUp -= OnTogglePreviewMouseLeftButtonUp;
        SplitButtonToggleButton.LostMouseCapture -= OnToggleLostMouseCapture;

        SplitButtonToggleButton.PreviewMouseLeftButtonDown += OnTogglePreviewMouseLeftButtonDown;
        SplitButtonToggleButton.PreviewMouseLeftButtonUp += OnTogglePreviewMouseLeftButtonUp;
        SplitButtonToggleButton.LostMouseCapture += OnToggleLostMouseCapture;
    }

    private void OnTogglePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // WinUI toggles the flyout: a click while open dismisses it without press chrome.
        _dismissFlyoutOnClick = _contextMenu?.IsOpen == true || IsDropDownOpen;
        if (_dismissFlyoutOnClick)
        {
            _contextMenu?.SetCurrentValue(ContextMenu.IsOpenProperty, false);
            e.Handled = true;
            return;
        }

        BeginChevronPressAnimation();
    }

    private void OnTogglePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_dismissFlyoutOnClick)
        {
            _dismissFlyoutOnClick = false;
            e.Handled = true;
            return;
        }

        // Same as DropDownButton: only open when the press started on the chevron
        // (ToggleButton.IsPressed). A press elsewhere that releases over us must not open.
        if (sender is not ToggleButton { IsPressed: true }
            || _contextMenu is null
            || _splitButtonToggleBorder is null)
        {
            return;
        }

        BeginChevronReleaseAnimation();

        var position = e.GetPosition(_splitButtonToggleBorder);
        if (VisualTreeHelper.HitTest(_splitButtonToggleBorder, position)?.VisualHit is null)
        {
            return;
        }

        _contextMenu.SetCurrentValue(MinWidthProperty, ActualWidth);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementTargetProperty, this);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementProperty, PlacementMode.Bottom);
        _contextMenu.SetCurrentValue(ContextMenu.IsOpenProperty, true);
    }

    private void OnToggleLostMouseCapture(object sender, MouseEventArgs e)
    {
        _dismissFlyoutOnClick = false;

        if (_chevronTranslate is not null && Math.Abs(_chevronTranslate.Y) > 0.01)
        {
            BeginChevronReleaseAnimation();
        }
    }

    private double GetPressDepth()
    {
        var viewport = _chevronHost?.ActualHeight > 0
            ? _chevronHost.ActualHeight
            : 12.0;
        return viewport * PressDepthRatio;
    }

    private double GetOvershoot()
    {
        var viewport = _chevronHost?.ActualHeight > 0
            ? _chevronHost.ActualHeight
            : 12.0;
        return -(viewport * OvershootRatio);
    }

    private void BeginChevronPressAnimation()
    {
        if (_chevronTranslate is null)
        {
            return;
        }

        var depth = GetPressDepth();
        var animation = new DoubleAnimationUsingKeyFrames
        {
            FillBehavior = FillBehavior.HoldEnd,
        };
        animation.KeyFrames.Add(
            new SplineDoubleKeyFrame(depth, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150)))
            {
                KeySpline = new KeySpline(0.167, 0.167, 0.65, 1.0),
            });

        _chevronTranslate.BeginAnimation(TranslateTransform.YProperty, animation);
    }

    private void BeginChevronReleaseAnimation()
    {
        if (_chevronTranslate is null)
        {
            return;
        }

        var overshoot = GetOvershoot();
        var animation = new DoubleAnimationUsingKeyFrames();
        animation.KeyFrames.Add(
            new SplineDoubleKeyFrame(overshoot, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(83)))
            {
                KeySpline = new KeySpline(0.55, 0.0, 0.75, 1.0),
            });
        animation.KeyFrames.Add(
            new SplineDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(317)))
            {
                KeySpline = new KeySpline(0.35, 0.0, 0.0, 1.0),
            });

        _chevronTranslate.BeginAnimation(TranslateTransform.YProperty, animation);
    }

    private bool IsOverToggle(Point positionRelativeToThis)
    {
        if (_splitButtonToggleBorder is null)
        {
            return false;
        }

        var toggleOrigin = _splitButtonToggleBorder.TranslatePoint(new Point(0, 0), this);
        var bounds = new Rect(toggleOrigin, _splitButtonToggleBorder.RenderSize);
        return bounds.Contains(positionRelativeToThis);
    }

    private void ApplyFlyoutContextMenuStyle(ContextMenu contextMenu)
    {
        if (contextMenu.ReadLocalValue(StyleProperty) != DependencyProperty.UnsetValue)
        {
            return;
        }

        var style =
            TryFindResource(FlyoutContextMenuStyleKey) as Style
            ?? Application.Current?.TryFindResource(FlyoutContextMenuStyleKey) as Style;

        if (style is not null)
        {
            contextMenu.Style = style;
        }
    }
}
