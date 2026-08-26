using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A selectable segment used inside a <see cref="Segmented"/> control.
/// </summary>
[TemplatePart(Name = PartSelectionBackground, Type = typeof(Border))]
[TemplatePart(Name = PartRoot, Type = typeof(Border))]
public class SegmentedItem : ListBoxItem
{
    private const string PartSelectionBackground = "SelectionBackground";
    private const string PartRoot = "Root";

    /// <summary>Inset shared by hover chrome and the selected-state animation origin.</summary>
    internal const double ChromeInset = 2.5;

    private static readonly TimeSpan SelectionChromeAnimationDuration = TimeSpan.FromMilliseconds(167);
    private static readonly IEasingFunction SelectionChromeEasing = new CubicEase { EasingMode = EasingMode.EaseOut };

    private Border? _selectionBackground;
    private UIElement? _root;
    private DependencyPropertyDescriptor? _isPressedDescriptor;
    private ThicknessAnimation? _selectionChromeAnimation;
    private bool _isSelectionChromeAnimating;
    private bool _isPressActive;

    static SegmentedItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SegmentedItem),
            new FrameworkPropertyMetadata(typeof(SegmentedItem)));
    }

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(object),
        typeof(SegmentedItem),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Gets or sets the optional icon shown before <see cref="ContentControl.Content"/>.
    /// Icon-only segments omit content.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectionCornerRadius"/> dependency property.</summary>
    public static readonly DependencyProperty SelectionCornerRadiusProperty = DependencyProperty.Register(
        nameof(SelectionCornerRadius),
        typeof(CornerRadius),
        typeof(SegmentedItem),
        new FrameworkPropertyMetadata(
            new CornerRadius(4),
            FrameworkPropertyMetadataOptions.AffectsRender,
            OnSelectionCornerRadiusChanged));

    /// <summary>
    /// Corner radius for the selected-state fill at rest (margin 0). Edge segments use larger
    /// radii on shell-facing corners; middle segments stay uniformly rounded.
    /// </summary>
    public CornerRadius SelectionCornerRadius
    {
        get => (CornerRadius)GetValue(SelectionCornerRadiusProperty);
        set => SetValue(SelectionCornerRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UnhookPressHelper();

        _selectionBackground = GetTemplateChild(PartSelectionBackground) as Border;
        _root = GetTemplateChild(PartRoot) as UIElement;

        if (_root is not null)
        {
            _isPressedDescriptor = DependencyPropertyDescriptor.FromProperty(
                PressHelper.IsPressedProperty,
                typeof(UIElement));
            _isPressedDescriptor.AddValueChanged(_root, OnRootPressChanged);
        }

        ApplySelectionChrome(IsSelected, animate: false);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == Selector.IsSelectedProperty)
        {
            ApplySelectionChrome(IsSelected, animate: IsLoaded);

            if (!IsSelected)
            {
                UpdatePressState(isPressed: false);
            }
        }
        else if (e.Property == UIElement.IsEnabledProperty && !IsEnabled)
        {
            UpdatePressState(isPressed: false);
        }
    }

    private void OnRootPressChanged(object? sender, EventArgs e)
    {
        if (_root is null)
        {
            return;
        }

        UpdatePressState(PressHelper.GetIsPressed(_root));
    }

    private void UnhookPressHelper()
    {
        if (_root is not null && _isPressedDescriptor is not null)
        {
            _isPressedDescriptor.RemoveValueChanged(_root, OnRootPressChanged);
        }

        _isPressedDescriptor = null;
        _root = null;
    }

    private void UpdatePressState(bool isPressed)
    {
        if (!IsSelected || !IsEnabled)
        {
            if (_isPressActive)
            {
                _isPressActive = false;
                GetOwnerSegmented()?.SetIndicatorPressed(false);
            }

            return;
        }

        if (_isPressActive == isPressed)
        {
            return;
        }

        _isPressActive = isPressed;
        GetOwnerSegmented()?.SetIndicatorPressed(isPressed);
    }

    private Segmented? GetOwnerSegmented() =>
        ItemsControl.ItemsControlFromItemContainer(this) as Segmented;

    private static void OnSelectionCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedItem item)
        {
            item.UpdateSelectionCornerRadius();
        }
    }

    private void ApplySelectionChrome(bool selected, bool animate)
    {
        if (_selectionBackground is null)
        {
            return;
        }

        var targetMargin = selected ? new Thickness(0) : new Thickness(ChromeInset);

        if (!animate || !IsVisible)
        {
            StopSelectionChromeAnimation(applyCurrent: false);
            _selectionBackground.Margin = targetMargin;
            UpdateSelectionCornerRadius();
            return;
        }

        StopSelectionChromeAnimation(applyCurrent: true);

        if (selected)
        {
            _selectionBackground.CornerRadius = new CornerRadius(4);
        }

        _isSelectionChromeAnimating = true;

        _selectionChromeAnimation = new ThicknessAnimation
        {
            From = _selectionBackground.Margin,
            To = targetMargin,
            Duration = new Duration(SelectionChromeAnimationDuration),
            EasingFunction = SelectionChromeEasing,
            FillBehavior = FillBehavior.Stop,
        };

        _selectionChromeAnimation.Completed += OnSelectionChromeAnimationCompleted;
        _selectionBackground.BeginAnimation(MarginProperty, _selectionChromeAnimation);
    }

    private void OnSelectionChromeAnimationCompleted(object? sender, EventArgs e)
    {
        if (_selectionChromeAnimation is not null)
        {
            _selectionChromeAnimation.Completed -= OnSelectionChromeAnimationCompleted;
            _selectionChromeAnimation = null;
        }

        _isSelectionChromeAnimating = false;

        if (_selectionBackground is null)
        {
            return;
        }

        var targetMargin = IsSelected ? new Thickness(0) : new Thickness(ChromeInset);
        _selectionBackground.BeginAnimation(MarginProperty, null);
        _selectionBackground.Margin = targetMargin;
        UpdateSelectionCornerRadius();
    }

    private void StopSelectionChromeAnimation(bool applyCurrent)
    {
        if (_selectionBackground is null)
        {
            return;
        }

        if (_isSelectionChromeAnimating)
        {
            _isSelectionChromeAnimating = false;
            if (_selectionChromeAnimation is not null)
            {
                _selectionChromeAnimation.Completed -= OnSelectionChromeAnimationCompleted;
                _selectionChromeAnimation = null;
            }

            var currentMargin = _selectionBackground.Margin;
            _selectionBackground.BeginAnimation(MarginProperty, null);
            if (applyCurrent)
            {
                _selectionBackground.Margin = currentMargin;
            }
        }
    }

    private void UpdateSelectionCornerRadius()
    {
        if (_selectionBackground is null || _isSelectionChromeAnimating)
        {
            return;
        }

        _selectionBackground.CornerRadius = IsSelected
            ? SelectionCornerRadius
            : new CornerRadius(4);
    }
}
