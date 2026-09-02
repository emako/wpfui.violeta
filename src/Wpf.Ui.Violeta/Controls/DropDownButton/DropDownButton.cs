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
/// A button that opens a flyout of choices. Flyout ContextMenu uses a Violeta style
/// without the upstream slide-from-above animation that covers the button.
/// Chevron uses a WinUI AnimatedChevronDownSmall-style clipped translate bounce.
/// </summary>
[TemplatePart(Name = ChevronIconPart, Type = typeof(UIElement))]
public class DropDownButton : Wpf.Ui.Controls.Button
{
    private const string FlyoutContextMenuStyleKey = "DefaultDropDownFlyoutContextMenuStyle";
    private const string ChevronIconPart = "PART_ChevronIcon";

    /// <summary>
    /// Fraction of the clipped chevron viewport translated on press.
    /// WinUI uses 7.5/48 (~0.156); tuned for FontSize-10 with ClipToBounds sink look.
    /// </summary>
    private const double PressDepthRatio = 0.18;

    /// <summary>Upward overshoot on release, relative to viewport height.</summary>
    private const double OvershootRatio = 0.10;

    private ContextMenu? _contextMenu;
    private TranslateTransform? _chevronTranslate;
    private FrameworkElement? _chevronHost;
    private bool _dismissFlyoutOnClick;

    static DropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(typeof(DropDownButton)));
    }

    public DropDownButton()
    {
        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        LostMouseCapture += OnLostMouseCapture;
    }

    /// <summary>Identifies the <see cref="Flyout"/> dependency property.</summary>
    public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register(
        nameof(Flyout),
        typeof(object),
        typeof(DropDownButton),
        new PropertyMetadata(null, OnFlyoutChanged));

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen),
        typeof(bool),
        typeof(DropDownButton),
        new PropertyMetadata(false));

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

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _chevronTranslate = null;
        _chevronHost = GetTemplateChild("PART_ChevronHost") as FrameworkElement;

        if (GetTemplateChild(ChevronIconPart) is UIElement chevron)
        {
            // Template Freezables are immutable — always install a fresh transform.
            _chevronTranslate = new TranslateTransform();
            chevron.RenderTransform = _chevronTranslate;
            chevron.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }

    private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DropDownButton)d).OnFlyoutChanged(e.OldValue, e.NewValue);
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

    protected virtual void OnContextMenuClosed(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    protected virtual void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // WinUI toggles the flyout: a click while open dismisses it.
        // WPF ContextMenu usually closes on MouseDown before MouseUp, so remember
        // not to reopen on the matching MouseUp.
        _dismissFlyoutOnClick = _contextMenu?.IsOpen == true || IsDropDownOpen;
        if (_dismissFlyoutOnClick)
        {
            _contextMenu?.SetCurrentValue(ContextMenu.IsOpenProperty, false);
            // Suppress Button IsPressed / Click chrome — dismiss is not a press.
            e.Handled = true;
            return;
        }

        BeginChevronPressAnimation();
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_dismissFlyoutOnClick)
        {
            _dismissFlyoutOnClick = false;
            e.Handled = true;
            return;
        }

        // ButtonBase only keeps IsPressed when the gesture started on this control
        // (and the pointer is still over it). Without this, a press elsewhere that
        // releases over us would still open the flyout.
        if (!IsPressed)
        {
            return;
        }

        BeginChevronReleaseAnimation();

        if (_contextMenu is null)
        {
            return;
        }

        _contextMenu.SetCurrentValue(MinWidthProperty, ActualWidth);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementTargetProperty, this);
        _contextMenu.SetCurrentValue(ContextMenu.PlacementProperty, PlacementMode.Bottom);
        _contextMenu.SetCurrentValue(ContextMenu.IsOpenProperty, true);
    }

    private void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        _dismissFlyoutOnClick = false;

        // Ensure we never leave the chevron stuck in the pressed offset.
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
        // WinUI PointerOverToPressed ≈ 150ms, cubic-bezier(0.167, 0.167, 0.65, 1)
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
        // WinUI PressedToNormal: depth → −overshoot (~83ms) → 0 (~317ms)
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
