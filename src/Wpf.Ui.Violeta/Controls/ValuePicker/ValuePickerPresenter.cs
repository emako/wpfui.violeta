using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// The flyout popup content for <see cref="ValuePicker"/>.
/// </summary>
[TemplatePart(Name = PartPickerContainer, Type = typeof(Grid))]
[TemplatePart(Name = PartAcceptButton, Type = typeof(Button))]
[TemplatePart(Name = PartDismissButton, Type = typeof(Button))]
public class ValuePickerPresenter : Control
{
    public const string PartPickerContainer = "PART_PickerContainer";
    public const string PartAcceptButton = "PART_AcceptButton";
    public const string PartDismissButton = "PART_DismissButton";

    private Grid? _pickerContainer;
    private readonly List<DateTimePickerPanel> _panels = [];

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(
            nameof(Columns),
            typeof(IList<ValuePickerColumn>),
            typeof(ValuePickerPresenter),
            new PropertyMetadata(null, OnColumnsChanged));

    public static readonly DependencyProperty SelectedIndicesProperty =
        DependencyProperty.Register(
            nameof(SelectedIndices),
            typeof(int[]),
            typeof(ValuePickerPresenter),
            new PropertyMetadata(null, OnSelectedIndicesChanged));

    public static readonly DependencyProperty SelectedValuesProperty =
        DependencyProperty.Register(
            nameof(SelectedValues),
            typeof(string[]),
            typeof(ValuePickerPresenter),
            new PropertyMetadata(null, OnSelectedValuesChanged));

    public static readonly RoutedEvent ConfirmedEvent =
        EventManager.RegisterRoutedEvent(nameof(Confirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ValuePickerPresenter));

    public static readonly RoutedEvent DismissedEvent =
        EventManager.RegisterRoutedEvent(nameof(Dismissed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ValuePickerPresenter));

    public IList<ValuePickerColumn>? Columns
    {
        get => (IList<ValuePickerColumn>?)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public int[]? SelectedIndices
    {
        get => (int[]?)GetValue(SelectedIndicesProperty);
        set => SetValue(SelectedIndicesProperty, value);
    }

    public string[]? SelectedValues
    {
        get => (string[]?)GetValue(SelectedValuesProperty);
        set => SetValue(SelectedValuesProperty, value);
    }

    public event RoutedEventHandler Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    public event RoutedEventHandler Dismissed
    {
        add => AddHandler(DismissedEvent, value);
        remove => RemoveHandler(DismissedEvent, value);
    }

    static ValuePickerPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ValuePickerPresenter),
            new FrameworkPropertyMetadata(typeof(ValuePickerPresenter)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _pickerContainer = GetTemplateChild(PartPickerContainer) as Grid;
        RebuildColumns();

        if (GetTemplateChild(PartAcceptButton) is Button accept)
            accept.Click += (_, _) => RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
        if (GetTemplateChild(PartDismissButton) is Button dismiss)
            dismiss.Click += (_, _) => RaiseEvent(new RoutedEventArgs(DismissedEvent, this));

        SyncPanelsToSelection();
    }

    public string[]? GetSelectedValues()
    {
        if (_panels.Count == 0)
            return null;

        return _panels
            .Select(panel => panel.GetSelectedValue() ?? string.Empty)
            .ToArray();
    }

    public int[] GetSelectedIndices()
    {
        return _panels.Select(panel => panel.SelectedIndex).ToArray();
    }

    private void RebuildColumns()
    {
        if (_pickerContainer == null)
            return;

        _pickerContainer.Children.Clear();
        _pickerContainer.ColumnDefinitions.Clear();
        _panels.Clear();

        var columns = Columns;
        if (columns == null || columns.Count == 0)
            return;

        int columnIndex = 0;
        for (int i = 0; i < columns.Count; i++)
        {
            if (i > 0)
            {
                _pickerContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1) });

                var separator = new Rectangle();
                separator.SetResourceReference(Shape.FillProperty, "DateTimePickerSeparatorBackground");
                Grid.SetColumn(separator, columnIndex++);
                _pickerContainer.Children.Add(separator);
            }

            _pickerContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var column = columns[i];
            var panel = new DateTimePickerPanel
            {
                ItemHeight = 36,
                PanelType = DateTimePickerPanelType.ValueList,
                ItemsSource = column.Items,
                ShouldLoop = column.ShouldLoop,
            };

            Grid.SetColumn(panel, columnIndex++);
            _pickerContainer.Children.Add(panel);
            panel.ApplyTemplate();
            _panels.Add(panel);
        }

        SyncPanelsToSelection();
    }

    private void SyncPanelsToSelection()
    {
        if (_panels.Count == 0)
            return;

        var columns = Columns;
        for (int i = 0; i < _panels.Count; i++)
        {
            int index = ResolveSelectedIndex(i, columns?[i]);
            int itemCount = columns?[i].Items.Count ?? 0;
            if (itemCount > 0)
                index = Math.Max(0, Math.Min(index, itemCount - 1));

            _panels[i].SelectedIndex = index;
        }
    }

    private static int IndexOf(IList<string> items, string value)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == value)
                return i;
        }

        return -1;
    }

    private int ResolveSelectedIndex(int columnIndex, ValuePickerColumn? column)
    {
        if (SelectedIndices != null && columnIndex < SelectedIndices.Length)
            return SelectedIndices[columnIndex];

        if (SelectedValues != null
            && columnIndex < SelectedValues.Length
            && column?.Items != null)
        {
            int found = IndexOf(column.Items, SelectedValues[columnIndex]);
            if (found >= 0)
                return found;
        }

        return 0;
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValuePickerPresenter presenter)
            presenter.RebuildColumns();
    }

    private static void OnSelectedIndicesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValuePickerPresenter presenter && presenter._panels.Count > 0)
            presenter.SyncPanelsToSelection();
    }

    private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValuePickerPresenter presenter && presenter._panels.Count > 0)
            presenter.SyncPanelsToSelection();
    }
}
