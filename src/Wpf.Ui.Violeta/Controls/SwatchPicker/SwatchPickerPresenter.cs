using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Hosts the selectable swatches inside a <see cref="SwatchPicker"/> popup.
/// </summary>
public class SwatchPickerPresenter : ListBox
{
    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(SwatchPickerPresenter), new PropertyMetadata(8, OnLayoutMetricsChanged));

    public static readonly DependencyProperty FocusModeProperty =
        DependencyProperty.Register(nameof(FocusMode), typeof(SwatchPickerFocusMode), typeof(SwatchPickerPresenter), new PropertyMetadata(SwatchPickerFocusMode.Arrow));

    public static readonly DependencyProperty LayoutProperty =
        DependencyProperty.Register(nameof(Layout), typeof(SwatchPickerLayout), typeof(SwatchPickerPresenter), new PropertyMetadata(SwatchPickerLayout.Row, OnLayoutMetricsChanged));

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SwatchPickerPresenter), new PropertyMetadata(4d, OnLayoutMetricsChanged));

    public static readonly DependencyProperty SwatchSizeProperty =
        DependencyProperty.Register(nameof(SwatchSize), typeof(double), typeof(SwatchPickerPresenter), new PropertyMetadata(28d, OnLayoutMetricsChanged));

    public static readonly DependencyProperty SwatchShapeProperty =
        DependencyProperty.Register(nameof(SwatchShape), typeof(SwatchShape), typeof(SwatchPickerPresenter), new PropertyMetadata(SwatchShape.Rounded, OnLayoutMetricsChanged));

    static SwatchPickerPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SwatchPickerPresenter), new FrameworkPropertyMetadata(typeof(SwatchPickerPresenter)));
    }

    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    public SwatchPickerFocusMode FocusMode
    {
        get => (SwatchPickerFocusMode)GetValue(FocusModeProperty);
        set => SetValue(FocusModeProperty, value);
    }

    public SwatchPickerLayout Layout
    {
        get => (SwatchPickerLayout)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public double SwatchSize
    {
        get => (double)GetValue(SwatchSizeProperty);
        set => SetValue(SwatchSizeProperty, value);
    }

    public SwatchShape SwatchShape
    {
        get => (SwatchShape)GetValue(SwatchShapeProperty);
        set => SetValue(SwatchShapeProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateItemsPanel();
        RefreshItemChrome();
    }

    protected override DependencyObject GetContainerForItemOverride() => new ListBoxItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not ListBoxItem container)
            return;

        container.PreviewMouseLeftButtonDown -= OnContainerPreviewMouseLeftButtonDown;
        container.PreviewMouseLeftButtonDown += OnContainerPreviewMouseLeftButtonDown;
        container.Margin = CreateItemMargin();

        if (item is Swatch swatch)
        {
            ApplySwatchChrome(swatch);
            container.IsEnabled = swatch.IsEnabled;
            swatch.Click -= OnSwatchClick;
            swatch.Click += OnSwatchClick;
        }
        else if (container.Content is Swatch templatedSwatch)
        {
            ApplySwatchChrome(templatedSwatch);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is ListBoxItem container)
            container.PreviewMouseLeftButtonDown -= OnContainerPreviewMouseLeftButtonDown;

        if (item is Swatch swatch)
            swatch.Click -= OnSwatchClick;

        base.ClearContainerForItemOverride(element, item);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        foreach (var removed in e.RemovedItems)
        {
            if (ResolveSwatch(removed) is { } oldSwatch)
                oldSwatch.SetCurrentValue(Swatch.IsSelectedProperty, false);
        }

        foreach (var added in e.AddedItems)
        {
            if (ResolveSwatch(added) is { } newSwatch)
                newSwatch.SetCurrentValue(Swatch.IsSelectedProperty, true);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space)
        {
            CloseParentDropDown();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
        if (e.Handled || FocusMode != SwatchPickerFocusMode.Arrow)
            return;

        var offset = e.Key switch
        {
            Key.Left => -1,
            Key.Right => 1,
            Key.Up => -Math.Max(1, EffectiveColumnCount),
            Key.Down => Math.Max(1, EffectiveColumnCount),
            _ => 0
        };

        if (offset == 0 || Items.Count == 0)
            return;

        var start = SelectedIndex >= 0 ? SelectedIndex : 0;
        var direction = offset > 0 ? 1 : -1;
        var index = start;
        for (var attempt = 0; attempt < Items.Count; attempt++)
        {
            index = (index + offset) % Items.Count;
            if (index < 0)
                index += Items.Count;

            if (ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem { IsEnabled: true } item)
            {
                SetCurrentValue(SelectedIndexProperty, index);
                item.Focus();
                e.Handled = true;
                return;
            }

            offset = direction;
        }
    }

    internal Swatch? ResolveSwatch(object? item)
    {
        if (item is Swatch swatch)
            return swatch;

        if (item is null)
            return null;

        return ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem container
            ? FindVisualChild<Swatch>(container)
            : null;
    }

    private int EffectiveColumnCount =>
        Layout == SwatchPickerLayout.Grid ? Math.Max(1, ColumnCount) : Math.Max(1, Items.Count);

    private Thickness CreateItemMargin()
    {
        var gap = Math.Max(0, Spacing) / 2d;
        return new Thickness(gap);
    }

    private CornerRadius CreateCornerRadius()
    {
        return SwatchShape switch
        {
            SwatchShape.Square => new CornerRadius(0),
            SwatchShape.Circular => new CornerRadius(Math.Max(0, SwatchSize) / 2d),
            _ => new CornerRadius(Math.Min(6d, Math.Max(0, SwatchSize) / 4d)),
        };
    }

    private void ApplySwatchChrome(Swatch swatch)
    {
        swatch.SetCurrentValue(WidthProperty, SwatchSize);
        swatch.SetCurrentValue(HeightProperty, SwatchSize);
        swatch.SetCurrentValue(Swatch.CornerRadiusProperty, CreateCornerRadius());
    }

    private void OnContainerPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Swatch is a Button and would otherwise swallow the click before ListBoxItem selects.
        if (sender is not ListBoxItem { IsEnabled: true } container)
            return;

        container.IsSelected = true;
        CloseParentDropDown();
    }

    private void OnSwatchClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Swatch swatch)
            return;

        SetCurrentValue(SelectedItemProperty, swatch);
        CloseParentDropDown();
    }

    private void CloseParentDropDown()
    {
        if (TemplatedParent is SwatchPicker picker)
            picker.SetCurrentValue(SwatchPicker.IsDropDownOpenProperty, false);
    }

    private void RefreshItemChrome()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem container)
                continue;

            container.Margin = CreateItemMargin();

            if (Items[i] is Swatch itemSwatch)
            {
                ApplySwatchChrome(itemSwatch);
                container.IsEnabled = itemSwatch.IsEnabled;
            }
            else if (FindVisualChild<Swatch>(container) is { } nested)
            {
                ApplySwatchChrome(nested);
            }
        }
    }

    private void UpdateItemsPanel()
    {
        if (Layout == SwatchPickerLayout.Grid)
        {
            var factory = new FrameworkElementFactory(typeof(UniformGrid));
            factory.SetValue(UniformGrid.ColumnsProperty, Math.Max(1, ColumnCount));
            ItemsPanel = new ItemsPanelTemplate(factory);
        }
        else
        {
            var factory = new FrameworkElementFactory(typeof(StackPanel));
            factory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            ItemsPanel = new ItemsPanelTemplate(factory);
        }
    }

    private static void OnLayoutMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SwatchPickerPresenter presenter)
        {
            presenter.UpdateItemsPanel();
            presenter.RefreshItemChrome();
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;

            var nested = FindVisualChild<T>(child);
            if (nested is not null)
                return nested;
        }

        return null;
    }
}
