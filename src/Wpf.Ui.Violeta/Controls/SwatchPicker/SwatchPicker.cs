using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Provides a popup for selecting a color or image swatch.
/// </summary>
[TemplatePart(Name = PartFlyoutButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartPresenter, Type = typeof(SwatchPickerPresenter))]
public class SwatchPicker : Control
{
    public const string PartFlyoutButton = "PART_FlyoutButton";
    public const string PartPopup = "PART_Popup";
    public const string PartPresenter = "PART_Presenter";

    private ToggleButton? _flyoutButton;
    private Popup? _popup;
    private SwatchPickerPresenter? _presenter;
    private Window? _parentWindow;
    private bool _windowHandlerRegistered;
    private bool _syncingSelection;

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SwatchPicker), new PropertyMetadata(null));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(SwatchPicker), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SwatchPicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(nameof(SelectedValue), typeof(object), typeof(SwatchPicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

    public static readonly DependencyProperty SelectedSwatchProperty =
        DependencyProperty.Register(nameof(SelectedSwatch), typeof(Swatch), typeof(SwatchPicker), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(SwatchPicker), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(SwatchPicker), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(SwatchPicker), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

    public static readonly DependencyProperty LayoutProperty =
        DependencyProperty.Register(nameof(Layout), typeof(SwatchPickerLayout), typeof(SwatchPicker), new PropertyMetadata(SwatchPickerLayout.Row));

    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(SwatchPicker), new PropertyMetadata(8));

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SwatchPicker), new PropertyMetadata(4d));

    public static readonly DependencyProperty FocusModeProperty =
        DependencyProperty.Register(nameof(FocusMode), typeof(SwatchPickerFocusMode), typeof(SwatchPicker), new PropertyMetadata(SwatchPickerFocusMode.Arrow));

    public static readonly DependencyProperty SwatchSizeProperty =
        DependencyProperty.Register(nameof(SwatchSize), typeof(double), typeof(SwatchPicker), new PropertyMetadata(28d));

    public static readonly DependencyProperty SwatchShapeProperty =
        DependencyProperty.Register(nameof(SwatchShape), typeof(SwatchShape), typeof(SwatchPicker), new PropertyMetadata(SwatchShape.Rounded));

    public static readonly RoutedEvent SelectedValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectedValueChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(SwatchPicker));

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(SwatchPicker));

    static SwatchPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SwatchPicker), new FrameworkPropertyMetadata(typeof(SwatchPicker)));
    }

    public IEnumerable? ItemsSource { get => (IEnumerable?)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public DataTemplate? ItemTemplate { get => (DataTemplate?)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public object? SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
    public object? SelectedValue { get => GetValue(SelectedValueProperty); set => SetValue(SelectedValueProperty, value); }
    public Swatch? SelectedSwatch { get => (Swatch?)GetValue(SelectedSwatchProperty); set => SetValue(SelectedSwatchProperty, value); }
    public string SelectedValuePath { get => (string)GetValue(SelectedValuePathProperty); set => SetValue(SelectedValuePathProperty, value); }
    public string DisplayMemberPath { get => (string)GetValue(DisplayMemberPathProperty); set => SetValue(DisplayMemberPathProperty, value); }
    public bool IsDropDownOpen { get => (bool)GetValue(IsDropDownOpenProperty); set => SetValue(IsDropDownOpenProperty, value); }
    public SwatchPickerLayout Layout { get => (SwatchPickerLayout)GetValue(LayoutProperty); set => SetValue(LayoutProperty, value); }
    public int ColumnCount { get => (int)GetValue(ColumnCountProperty); set => SetValue(ColumnCountProperty, value); }
    public double Spacing { get => (double)GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }
    public SwatchPickerFocusMode FocusMode { get => (SwatchPickerFocusMode)GetValue(FocusModeProperty); set => SetValue(FocusModeProperty, value); }
    public double SwatchSize { get => (double)GetValue(SwatchSizeProperty); set => SetValue(SwatchSizeProperty, value); }
    public SwatchShape SwatchShape { get => (SwatchShape)GetValue(SwatchShapeProperty); set => SetValue(SwatchShapeProperty, value); }

    public event RoutedPropertyChangedEventHandler<object> SelectedValueChanged { add => AddHandler(SelectedValueChangedEvent, value); remove => RemoveHandler(SelectedValueChangedEvent, value); }
    public event SelectionChangedEventHandler SelectionChanged { add => AddHandler(SelectionChangedEvent, value); remove => RemoveHandler(SelectionChangedEvent, value); }

    public override void OnApplyTemplate()
    {
        if (_presenter is not null) _presenter.SelectionChanged -= OnPresenterSelectionChanged;
        base.OnApplyTemplate();
        _flyoutButton = GetTemplateChild(PartFlyoutButton) as ToggleButton;
        _popup = GetTemplateChild(PartPopup) as Popup;
        _presenter = GetTemplateChild(PartPresenter) as SwatchPickerPresenter;
        if (_presenter is not null) _presenter.SelectionChanged += OnPresenterSelectionChanged;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;
        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void OnPresenterSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0 || _syncingSelection) return;
        SetCurrentValue(SelectedItemProperty, e.AddedItems[0]);
        SetCurrentValue(IsDropDownOpenProperty, false);
        _flyoutButton?.Focus();
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SwatchPicker)d;
        picker._syncingSelection = true;
        try
        {
            if (e.OldValue is Swatch oldSwatch)
                oldSwatch.SetCurrentValue(Swatch.IsSelectedProperty, false);

            if (e.NewValue is Swatch newSwatch)
                newSwatch.SetCurrentValue(Swatch.IsSelectedProperty, true);

            if (picker._presenter is not null) picker._presenter.SelectedItem = e.NewValue;
            picker.SetCurrentValue(SelectedValueProperty, picker.GetSelectedValue(e.NewValue));
            picker.SetCurrentValue(SelectedSwatchProperty, picker.FindSwatch(e.NewValue));
        }
        finally { picker._syncingSelection = false; }
        picker.RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.OldValue is null ? Array.Empty<object>() : new[] { e.OldValue }, e.NewValue is null ? Array.Empty<object>() : new[] { e.NewValue }));
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SwatchPicker)d;
        picker.RaiseEvent(new RoutedPropertyChangedEventArgs<object>(e.OldValue, e.NewValue, SelectedValueChangedEvent));
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SwatchPicker)d;
        if (e.NewValue is true) picker.RegisterWindowHandler(); else picker.UnregisterWindowHandler();
    }

    private object? GetSelectedValue(object? item)
    {
        if (item is null) return null;
        if (item is Swatch swatch) return swatch.Value;
        if (string.IsNullOrEmpty(SelectedValuePath)) return item;
        var property = item.GetType().GetProperty(SelectedValuePath);
        return property?.GetValue(item);
    }

    private Swatch? FindSwatch(object? item)
    {
        if (_presenter is null || item is null) return null;
        return _presenter.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem container
            ? FindVisualChild<Swatch>(container)
            : null;
    }

    private void RegisterWindowHandler()
    {
        _parentWindow ??= Window.GetWindow(this);
        if (_parentWindow is null || _windowHandlerRegistered) return;
        _parentWindow.PreviewMouseLeftButtonDown += OnWindowPreviewMouseLeftButtonDown;
        _parentWindow.PreviewMouseWheel += OnWindowPreviewMouseWheel;
        _windowHandlerRegistered = true;
    }

    private void UnregisterWindowHandler()
    {
        if (_parentWindow is null || !_windowHandlerRegistered) return;
        _parentWindow.PreviewMouseLeftButtonDown -= OnWindowPreviewMouseLeftButtonDown;
        _parentWindow.PreviewMouseWheel -= OnWindowPreviewMouseWheel;
        _windowHandlerRegistered = false;
    }

    private void OnWindowPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target || IsDescendantOf(target, this) || (_popup?.Child is UIElement child && IsDescendantOf(target, child))) return;
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnWindowPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.OriginalSource is DependencyObject target && _popup?.Child is UIElement child && !IsDescendantOf(target, child)) e.Handled = true;
    }

    private static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        for (DependencyObject? current = element; current is not null; current = VisualTreeHelper.GetParent(current)) if (current == ancestor) return true;
        return false;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result) return result;
            var nested = FindVisualChild<T>(child);
            if (nested is not null) return nested;
        }
        return null;
    }
}
