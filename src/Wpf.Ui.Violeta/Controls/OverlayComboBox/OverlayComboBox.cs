using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// ComboBox-like selector whose drop-down lives in the same HWND (never <see cref="Popup"/>).
/// Prefers a <c>PART_OverlayLayer</c> <see cref="Canvas"/> ancestor (e.g. on <see cref="ColorView"/>),
/// then falls back to <see cref="AdornerLayer"/>.
/// </summary>
[TemplatePart(Name = PartToggleButton, Type = typeof(ToggleButton))]
public class OverlayComboBox : Selector
{
    public const string PartToggleButton = "PART_ToggleButton";
    public const string PartOverlayLayer = "PART_OverlayLayer";

    private ToggleButton? _toggleButton;
    private Canvas? _overlayLayer;
    private AdornerLayer? _adornerLayer;
    private OverlayComboBoxDropDownAdorner? _adorner;
    private Border? _dropDownBorder;
    private ListBox? _dropDownList;
    private bool _isUpdatingDropDown;
    private bool _syncingDropDownSelection;
    private bool _overlayWasHitTestVisible;

    static OverlayComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OverlayComboBox),
            new FrameworkPropertyMetadata(typeof(OverlayComboBox)));
    }

    public OverlayComboBox()
    {
        Loaded += (_, _) => UpdateSelectionBoxItem();
        Unloaded += (_, _) => CloseDropDown();
        IsVisibleChanged += (_, e) =>
        {
            if (e.NewValue is false)
                SetCurrentValue(IsDropDownOpenProperty, false);
        };
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(OverlayComboBox),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged));

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(OverlayComboBox),
            new FrameworkPropertyMetadata(SystemParameters.PrimaryScreenHeight / 3));

    public static readonly DependencyProperty CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner(
            typeof(OverlayComboBox),
            new FrameworkPropertyMetadata(
                new CornerRadius(4),
                FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly DependencyPropertyKey SelectionBoxItemPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SelectionBoxItem),
            typeof(object),
            typeof(OverlayComboBox),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectionBoxItemProperty =
        SelectionBoxItemPropertyKey.DependencyProperty;

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    [Bindable(true)]
    [Category("Layout")]
    [TypeConverter(typeof(LengthConverter))]
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public object? SelectionBoxItem => GetValue(SelectionBoxItemProperty);

    public override void OnApplyTemplate()
    {
        CloseDropDown();
        base.OnApplyTemplate();
        _toggleButton = GetTemplateChild(PartToggleButton) as ToggleButton;
        UpdateSelectionBoxItem();
        if (IsDropDownOpen)
            OpenDropDown();
    }

    protected override DependencyObject GetContainerForItemOverride() =>
        new System.Windows.Controls.ComboBoxItem();

    protected override bool IsItemItsOwnContainerOverride(object item) =>
        item is System.Windows.Controls.ComboBoxItem;

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        UpdateSelectionBoxItem();
        if (_dropDownList is not null && !_syncingDropDownSelection)
            SyncDropDownListSelection();
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateSelectionBoxItem();
        if (IsDropDownOpen)
            RebuildDropDownItems();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
            return;

        if ((e.Key == Key.F4 && !Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) ||
            ((e.Key == Key.Down || e.Key == Key.Up) && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key is Key.Down or Key.Up or Key.Return or Key.Space))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);

        // Drop-down lives outside our visual tree (Canvas / Adorner), so losing focus here
        // often means focus moved into the list — defer and re-check both hosts.
        if (!(bool)e.NewValue && IsDropDownOpen)
            Dispatcher.BeginInvoke(DispatcherPriority.Input, CloseIfFocusLeft);
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var combo = (OverlayComboBox)d;
        if (combo._isUpdatingDropDown)
            return;

        if ((bool)e.NewValue)
            combo.OpenDropDown();
        else
            combo.CloseDropDown();
    }

    private void OpenDropDown()
    {
        _overlayLayer = FindOverlayLayer();
        _adornerLayer = _overlayLayer is null ? AdornerLayer.GetAdornerLayer(this) : null;

        if (_overlayLayer is null && _adornerLayer is null)
        {
            ForceClose();
            return;
        }

        EnsureDropDownVisuals();
        RebuildDropDownItems();
        AttachDropDown();
        PositionDropDown();

        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (!IsDropDownOpen)
                return;

            if (_overlayLayer is not null)
                _overlayLayer.PreviewMouseLeftButtonDown += OnOverlayPreviewMouseLeftButtonDown;
            else if (Window.GetWindow(this) is { } window)
                window.PreviewMouseLeftButtonDown += OnOverlayPreviewMouseLeftButtonDown;

            if (Window.GetWindow(this) is { } owner)
                owner.Deactivated += OnOwnerDeactivated;

            if (_dropDownList is not null)
                _dropDownList.LostKeyboardFocus += OnDropDownLostKeyboardFocus;

            PositionDropDown();
            _dropDownList?.Focus();
        }));
    }

    private void CloseDropDown()
    {
        if (_overlayLayer is not null)
            _overlayLayer.PreviewMouseLeftButtonDown -= OnOverlayPreviewMouseLeftButtonDown;
        if (Window.GetWindow(this) is { } window)
        {
            window.PreviewMouseLeftButtonDown -= OnOverlayPreviewMouseLeftButtonDown;
            window.Deactivated -= OnOwnerDeactivated;
        }

        if (_dropDownList is not null)
        {
            _dropDownList.LostKeyboardFocus -= OnDropDownLostKeyboardFocus;
            _dropDownList.SelectionChanged -= OnDropDownListSelectionChanged;
        }

        DetachDropDown();

        _dropDownBorder = null;
        _dropDownList = null;
        _overlayLayer = null;
        _adornerLayer = null;

        if (IsDropDownOpen)
            ForceClose();

        if (_toggleButton?.IsChecked == true)
            _toggleButton.IsChecked = false;
    }

    private void ForceClose()
    {
        _isUpdatingDropDown = true;
        try
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
        finally
        {
            _isUpdatingDropDown = false;
        }
    }

    private void AttachDropDown()
    {
        if (_dropDownBorder is null)
            return;

        if (_overlayLayer is not null)
        {
            _overlayWasHitTestVisible = _overlayLayer.IsHitTestVisible;
            _overlayLayer.IsHitTestVisible = true;
            if (!_overlayLayer.Children.Contains(_dropDownBorder))
                _overlayLayer.Children.Add(_dropDownBorder);
            return;
        }

        UIElement? adorned = GetAdornerDecoratorChild() ?? this;
        _adorner = new OverlayComboBoxDropDownAdorner(adorned, _dropDownBorder, this);
        _adornerLayer?.Add(_adorner);
    }

    private void DetachDropDown()
    {
        if (_overlayLayer is not null && _dropDownBorder is not null)
        {
            _overlayLayer.Children.Remove(_dropDownBorder);
            _overlayLayer.IsHitTestVisible = _overlayWasHitTestVisible;
        }

        if (_adorner is not null)
        {
            _adornerLayer?.Remove(_adorner);
            _adorner.DetachChild();
            _adorner = null;
        }
    }

    private void PositionDropDown()
    {
        if (_dropDownBorder is null)
            return;

        _dropDownBorder.MinWidth = Math.Max(ActualWidth, 0);
        _dropDownBorder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        if (_overlayLayer is not null)
        {
            Point origin = TranslatePoint(new Point(0, ActualHeight), _overlayLayer);
            Canvas.SetLeft(_dropDownBorder, origin.X);
            Canvas.SetTop(_dropDownBorder, origin.Y + 1);
            _dropDownBorder.Width = Math.Max(ActualWidth, _dropDownBorder.DesiredSize.Width);
            return;
        }

        _adorner?.InvalidateMeasure();
        _adorner?.InvalidateArrange();
    }

    private void EnsureDropDownVisuals()
    {
        if (_dropDownList is not null && _dropDownBorder is not null)
            return;

        _dropDownList = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Focusable = true,
            SnapsToDevicePixels = true,
            OverridesDefaultStyle = true,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            MaxHeight = MaxDropDownHeight,
            Template = CreateDropDownListTemplate(),
        };

        if (TryFindResource("DefaultOverlayComboBoxItemStyle") is Style overlayItemStyle)
            _dropDownList.ItemContainerStyle = overlayItemStyle;
        else if (ItemContainerStyle is not null)
            _dropDownList.ItemContainerStyle = ItemContainerStyle;

        ScrollViewer.SetHorizontalScrollBarVisibility(_dropDownList, ScrollBarVisibility.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(_dropDownList, ScrollBarVisibility.Auto);
        _dropDownList.SelectionChanged += OnDropDownListSelectionChanged;

        CornerRadius cornerRadius = TryFindResource("PopupCornerRadius") as CornerRadius? ?? new CornerRadius(8);

        _dropDownBorder = new Border
        {
            Child = _dropDownList,
            Padding = new Thickness(0, 4, 0, 6),
            BorderThickness = new Thickness(1),
            CornerRadius = cornerRadius,
            SnapsToDevicePixels = true,
            Focusable = false,
            Effect = new DropShadowEffect
            {
                BlurRadius = 20,
                Direction = 270,
                Opacity = 0.135,
                ShadowDepth = 10,
                Color = Color.FromRgb(0x20, 0x20, 0x20),
            },
        };

        if (TryFindResource("ComboBoxDropDownBackground") is Brush background)
            _dropDownBorder.Background = background;
        else
            _dropDownBorder.Background = Brushes.White;

        if (TryFindResource("ComboBoxDropDownBorderBrush") is Brush borderBrush)
            _dropDownBorder.BorderBrush = borderBrush;
        else
            _dropDownBorder.BorderBrush = Brushes.Gray;
    }

    private static ControlTemplate CreateDropDownListTemplate()
    {
        var template = new ControlTemplate(typeof(ListBox));
        var scrollViewer = new FrameworkElementFactory(typeof(ScrollViewer));
        scrollViewer.SetValue(UIElement.FocusableProperty, false);
        scrollViewer.SetValue(Control.PaddingProperty, new Thickness(0));
        scrollViewer.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
        scrollViewer.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);

        var itemsPresenter = new FrameworkElementFactory(typeof(ItemsPresenter));
        scrollViewer.AppendChild(itemsPresenter);
        template.VisualTree = scrollViewer;
        return template;
    }

    private void RebuildDropDownItems()
    {
        if (_dropDownList is null)
            return;

        _syncingDropDownSelection = true;
        try
        {
            _dropDownList.Items.Clear();
            foreach (object item in Items)
            {
                if (item is ContentControl contentControl)
                {
                    _dropDownList.Items.Add(new ListBoxItem
                    {
                        Content = contentControl.Content,
                        ContentTemplate = contentControl.ContentTemplate,
                        Tag = contentControl.Tag,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    });
                }
                else
                {
                    _dropDownList.Items.Add(item);
                }
            }

            SyncDropDownListSelection();
        }
        finally
        {
            _syncingDropDownSelection = false;
        }
    }

    private void SyncDropDownListSelection()
    {
        if (_dropDownList is null)
            return;

        _syncingDropDownSelection = true;
        try
        {
            int index = SelectedIndex;
            _dropDownList.SelectedIndex = index >= 0 && index < _dropDownList.Items.Count ? index : -1;
        }
        finally
        {
            _syncingDropDownSelection = false;
        }
    }

    private void OnDropDownListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingDropDownSelection || _dropDownList is null)
            return;

        int index = _dropDownList.SelectedIndex;
        if (index < 0 || index >= Items.Count)
            return;

        SelectedIndex = index;
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnOverlayPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsMouseOver)
            return;

        if (_dropDownBorder?.IsMouseOver == true)
            return;

        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnOwnerDeactivated(object? sender, EventArgs e) =>
        SetCurrentValue(IsDropDownOpenProperty, false);

    private void OnDropDownLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        Dispatcher.BeginInvoke(DispatcherPriority.Input, CloseIfFocusLeft);

    private void CloseIfFocusLeft()
    {
        if (!IsDropDownOpen)
            return;

        if (IsKeyboardFocusWithin || IsDropDownFocusWithin())
            return;

        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private bool IsDropDownFocusWithin()
    {
        if (_dropDownBorder is null && _dropDownList is null)
            return false;

        if (_dropDownList?.IsKeyboardFocusWithin == true || _dropDownBorder?.IsKeyboardFocusWithin == true)
            return true;

        return Keyboard.FocusedElement is DependencyObject focused &&
               ((_dropDownBorder is not null && IsDescendantOf(focused, _dropDownBorder)) ||
                (_dropDownList is not null && IsDescendantOf(focused, _dropDownList)));
    }

    private static bool IsDescendantOf(DependencyObject? element, DependencyObject? ancestor)
    {
        while (element is not null)
        {
            if (element == ancestor)
                return true;
            element = VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element);
        }

        return false;
    }

    private Canvas? FindOverlayLayer()
    {
        if (TemplatedParent is FrameworkElement templatedParent &&
            templatedParent.FindName(PartOverlayLayer) is Canvas templateCanvas)
            return templateCanvas;

        DependencyObject? current = this;
        while (current is not null)
        {
            if (current is Canvas { Name: PartOverlayLayer } canvas)
                return canvas;

            if (current is FrameworkElement fe)
            {
                if (fe.FindName(PartOverlayLayer) is Canvas named)
                    return named;

                if (fe.TemplatedParent is FrameworkElement parent &&
                    parent.FindName(PartOverlayLayer) is Canvas parentCanvas)
                    return parentCanvas;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private UIElement? GetAdornerDecoratorChild()
    {
        DependencyObject? current = this;
        while (current is not null)
        {
            if (current is AdornerDecorator decorator)
                return decorator.Child;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void UpdateSelectionBoxItem()
    {
        object? boxItem = SelectedItem switch
        {
            null => null,
            ContentControl { Content: not null } contentControl => contentControl.Content,
            var item => item,
        };
        SetValue(SelectionBoxItemPropertyKey, boxItem);
    }
}

internal sealed class OverlayComboBoxDropDownAdorner : Adorner
{
    private UIElement? _child;
    private readonly OverlayComboBox _owner;

    public OverlayComboBoxDropDownAdorner(UIElement adornedElement, UIElement child, OverlayComboBox owner)
        : base(adornedElement)
    {
        _owner = owner;
        IsClipEnabled = false;
        IsHitTestVisible = true;
        AttachChild(child);
    }

    public void AttachChild(UIElement child)
    {
        if (_child is not null)
            DetachChild();
        _child = child;
        AddVisualChild(child);
    }

    public void DetachChild()
    {
        if (_child is null)
            return;
        RemoveVisualChild(_child);
        _child = null;
    }

    protected override int VisualChildrenCount => _child is null ? 0 : 1;

    protected override Visual GetVisualChild(int index) =>
        _child ?? throw new ArgumentOutOfRangeException(nameof(index));

    protected override Size MeasureOverride(Size constraint)
    {
        if (_child is null)
            return new Size();

        double width = Math.Max(_owner.ActualWidth, 0);
        _child.Measure(new Size(Math.Max(width, 1), double.PositiveInfinity));
        return constraint.IsEmpty ? new Size() : constraint;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_child is null || !_owner.IsVisible)
            return finalSize;

        Point origin;
        try
        {
            origin = _owner.TranslatePoint(new Point(0, _owner.ActualHeight), AdornedElement);
        }
        catch (InvalidOperationException)
        {
            return finalSize;
        }

        double width = Math.Max(_owner.ActualWidth, _child.DesiredSize.Width);
        double height = Math.Max(_child.DesiredSize.Height, 1);
        _child.Arrange(new Rect(origin.X, origin.Y + 1, width, height));
        return finalSize;
    }
}
