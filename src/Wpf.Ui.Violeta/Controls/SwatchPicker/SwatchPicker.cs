using System;
using System.Collections;
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
[TemplatePart(Name = PartPreviewSwatch, Type = typeof(Swatch))]
[TemplatePart(Name = PartEmptyPreview, Type = typeof(UIElement))]
public class SwatchPicker : Control
{
    public const string PartFlyoutButton = "PART_FlyoutButton";
    public const string PartPopup = "PART_Popup";
    public const string PartPresenter = "PART_Presenter";
    public const string PartPreviewSwatch = "PART_PreviewSwatch";
    public const string PartEmptyPreview = "PART_EmptyPreview";

    private ToggleButton? _flyoutButton;
    private Popup? _popup;
    private SwatchPickerPresenter? _presenter;
    private Swatch? _previewSwatch;
    private UIElement? _emptyPreview;
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
        DependencyProperty.Register(nameof(SwatchSize), typeof(double), typeof(SwatchPicker), new PropertyMetadata(28d, OnPreviewMetricsChanged));

    public static readonly DependencyProperty SwatchShapeProperty =
        DependencyProperty.Register(nameof(SwatchShape), typeof(SwatchShape), typeof(SwatchPicker), new PropertyMetadata(SwatchShape.Rounded, OnPreviewMetricsChanged));

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
        if (_presenter is not null)
            _presenter.SelectionChanged -= OnPresenterSelectionChanged;

        base.OnApplyTemplate();

        _flyoutButton = GetTemplateChild(PartFlyoutButton) as ToggleButton;
        _popup = GetTemplateChild(PartPopup) as Popup;
        _presenter = GetTemplateChild(PartPresenter) as SwatchPickerPresenter;
        _previewSwatch = GetTemplateChild(PartPreviewSwatch) as Swatch;
        _emptyPreview = GetTemplateChild(PartEmptyPreview) as UIElement;

        if (_presenter is not null)
            _presenter.SelectionChanged += OnPresenterSelectionChanged;

        UpdatePreviewSwatch();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
            return;

        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && e.Key is Key.Down or Key.Up or Key.F4)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void OnPresenterSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingSelection || e.AddedItems.Count == 0)
            return;

        // Instant-commit the value; popup is closed by click / Enter, not by arrow navigation.
        SetCurrentValue(SelectedItemProperty, e.AddedItems[0]);
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

            if (picker._presenter is not null && !Equals(picker._presenter.SelectedItem, e.NewValue))
                picker._presenter.SetCurrentValue(Selector.SelectedItemProperty, e.NewValue);

            var selectedSwatch = picker.ResolveSelectedSwatch(e.NewValue);
            picker.SetCurrentValue(SelectedSwatchProperty, selectedSwatch);
            picker.SetCurrentValue(SelectedValueProperty, picker.GetSelectedValue(e.NewValue));
            picker.UpdatePreviewSwatch();
        }
        finally
        {
            picker._syncingSelection = false;
        }

        picker.RaiseEvent(new SelectionChangedEventArgs(
            SelectionChangedEvent,
            e.OldValue is null ? Array.Empty<object>() : [e.OldValue],
            e.NewValue is null ? Array.Empty<object>() : [e.NewValue]));
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SwatchPicker)d;
        if (!picker._syncingSelection)
        {
            var match = picker.FindItemByValue(e.NewValue);
            if (!Equals(picker.SelectedItem, match))
                picker.SetCurrentValue(SelectedItemProperty, match);
        }

        picker.RaiseEvent(new RoutedPropertyChangedEventArgs<object>(e.OldValue, e.NewValue, SelectedValueChangedEvent));
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (SwatchPicker)d;
        if (e.NewValue is true)
        {
            picker.RegisterWindowHandler();
        }
        else
        {
            picker.UnregisterWindowHandler();
            picker._flyoutButton?.Focus();
        }
    }

    private static void OnPreviewMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SwatchPicker)d).UpdatePreviewSwatch();
    }

    private object? GetSelectedValue(object? item)
    {
        if (item is null)
            return null;

        if (item is Swatch swatch)
            return swatch.Value;

        if (string.IsNullOrEmpty(SelectedValuePath))
            return item;

        return item.GetType().GetProperty(SelectedValuePath)?.GetValue(item);
    }

    private object? FindItemByValue(object? value)
    {
        if (ItemsSource is null)
            return null;

        foreach (var item in ItemsSource)
        {
            if (Equals(GetSelectedValue(item), value))
                return item;
        }

        return null;
    }

    private Swatch? ResolveSelectedSwatch(object? item)
    {
        if (item is Swatch swatch)
            return swatch;

        return _presenter?.ResolveSwatch(item);
    }

    private void UpdatePreviewSwatch()
    {
        var source = SelectedSwatch ?? SelectedItem as Swatch;
        var size = Math.Max(0, SwatchSize);
        // Compact preview inside the closed box.
        var previewSize = Math.Min(24d, size > 0 ? size : 24d);

        if (_previewSwatch is not null)
        {
            _previewSwatch.Width = previewSize;
            _previewSwatch.Height = previewSize;
            _previewSwatch.CornerRadius = CreateCornerRadius(previewSize);

            if (source is not null)
            {
                _previewSwatch.Visibility = Visibility.Visible;
                _previewSwatch.Color = source.Color;
                _previewSwatch.ImageSource = source.ImageSource;
                _previewSwatch.Value = source.Value;
                _previewSwatch.IsEnabled = source.IsEnabled;
            }
            else
            {
                _previewSwatch.Visibility = Visibility.Collapsed;
                _previewSwatch.Color = null;
                _previewSwatch.ImageSource = null;
                _previewSwatch.Value = null;
            }
        }

        if (_emptyPreview is not null)
            _emptyPreview.Visibility = source is null ? Visibility.Visible : Visibility.Collapsed;
    }

    private CornerRadius CreateCornerRadius(double size)
    {
        return SwatchShape switch
        {
            SwatchShape.Square => new CornerRadius(0),
            SwatchShape.Circular => new CornerRadius(size / 2d),
            _ => new CornerRadius(Math.Min(6d, size / 4d)),
        };
    }

    private void RegisterWindowHandler()
    {
        _parentWindow ??= Window.GetWindow(this);
        if (_parentWindow is null || _windowHandlerRegistered)
            return;

        _parentWindow.PreviewMouseLeftButtonDown += OnWindowPreviewMouseLeftButtonDown;
        _parentWindow.PreviewMouseWheel += OnWindowPreviewMouseWheel;
        _windowHandlerRegistered = true;
    }

    private void UnregisterWindowHandler()
    {
        if (_parentWindow is null || !_windowHandlerRegistered)
            return;

        _parentWindow.PreviewMouseLeftButtonDown -= OnWindowPreviewMouseLeftButtonDown;
        _parentWindow.PreviewMouseWheel -= OnWindowPreviewMouseWheel;
        _windowHandlerRegistered = false;
    }

    private void OnWindowPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
            return;

        if (IsDescendantOf(target, this) || (_popup?.Child is UIElement child && IsDescendantOf(target, child)))
            return;

        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnWindowPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.OriginalSource is DependencyObject target && _popup?.Child is UIElement child && !IsDescendantOf(target, child))
            e.Handled = true;
    }

    private static bool IsDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        for (DependencyObject? current = element; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current == ancestor)
                return true;
        }

        return false;
    }
}
