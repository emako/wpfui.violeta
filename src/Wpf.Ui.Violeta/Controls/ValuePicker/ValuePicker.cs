using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A drum-roll style value picker with one scrollable column per string array.
/// </summary>
[TemplatePart(Name = PartFlyoutButton, Type = typeof(Button))]
[TemplatePart(Name = PartFlyoutButtonContentGrid, Type = typeof(Grid))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartPresenter, Type = typeof(ValuePickerPresenter))]
[TemplatePart(Name = PartIconTextBlock, Type = typeof(TextBlock))]
public class ValuePicker : Control
{
    public const string PartFlyoutButton = "PART_FlyoutButton";
    public const string PartFlyoutButtonContentGrid = "PART_ButtonContentGrid";
    public const string PartPopup = "PART_Popup";
    public const string PartPresenter = "PART_Presenter";
    public const string PartIconTextBlock = "PART_IconTextBlock";

    private Button? _flyoutButton;
    private Grid? _flyoutContentGrid;
    private Popup? _popup;
    private ValuePickerPresenter? _presenter;
    private TextBlock? _iconTextBlock;
    private readonly List<TextBlock> _valueTextBlocks = [];

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(
            nameof(Columns),
            typeof(IList<ValuePickerColumn>),
            typeof(ValuePicker),
            new PropertyMetadata(null, OnColumnsChanged));

    public static readonly DependencyProperty SelectedValuesProperty =
        DependencyProperty.Register(
            nameof(SelectedValues),
            typeof(string[]),
            typeof(ValuePicker),
            new PropertyMetadata(null, OnSelectedValuesChanged));

    public static readonly DependencyProperty SelectedIndicesProperty =
        DependencyProperty.Register(
            nameof(SelectedIndices),
            typeof(int[]),
            typeof(ValuePicker),
            new PropertyMetadata(null, OnSelectedIndicesChanged));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(ValuePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(ValuePicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(
            nameof(IconGlyph),
            typeof(string),
            typeof(ValuePicker),
            new PropertyMetadata("\uE8BC", OnIconGlyphChanged));

    public static readonly RoutedEvent SelectedValuesChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectedValuesChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(ValuePicker));

    public IList<ValuePickerColumn>? Columns
    {
        get => (IList<ValuePickerColumn>?)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public string[]? SelectedValues
    {
        get => (string[]?)GetValue(SelectedValuesProperty);
        set => SetValue(SelectedValuesProperty, value);
    }

    public int[]? SelectedIndices
    {
        get => (int[]?)GetValue(SelectedIndicesProperty);
        set => SetValue(SelectedIndicesProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Fluent System Icons glyph shown on the right side of the flyout button.
    /// </summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public event RoutedEventHandler SelectedValuesChanged
    {
        add => AddHandler(SelectedValuesChangedEvent, value);
        remove => RemoveHandler(SelectedValuesChangedEvent, value);
    }

    static ValuePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ValuePicker),
            new FrameworkPropertyMetadata(typeof(ValuePicker)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_flyoutButton != null)
            _flyoutButton.Click -= OnFlyoutButtonClick;
        if (_presenter != null)
        {
            _presenter.Confirmed -= OnPresenterConfirmed;
            _presenter.Dismissed -= OnPresenterDismissed;
        }

        _flyoutButton = GetTemplateChild(PartFlyoutButton) as Button;
        _flyoutContentGrid = GetTemplateChild(PartFlyoutButtonContentGrid) as Grid;
        _popup = GetTemplateChild(PartPopup) as Popup;
        _presenter = GetTemplateChild(PartPresenter) as ValuePickerPresenter;
        _iconTextBlock = GetTemplateChild(PartIconTextBlock) as TextBlock;

        if (_flyoutButton != null)
            _flyoutButton.Click += OnFlyoutButtonClick;

        if (_presenter != null)
        {
            _presenter.Confirmed += OnPresenterConfirmed;
            _presenter.Dismissed += OnPresenterDismissed;
        }

        RebuildFlyoutContent();
        UpdateValueTextBlocks();
    }

    private void OnFlyoutButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_popup == null)
            return;

        if (_presenter != null)
        {
            _presenter.Columns = Columns;
            _presenter.SelectedValues = SelectedValues;
            _presenter.SelectedIndices = SelectedIndices;
        }

        _popup.IsOpen = true;
    }

    private void OnPresenterConfirmed(object? sender, RoutedEventArgs e)
    {
        if (_presenter != null)
        {
            SelectedValues = _presenter.GetSelectedValues();
            SelectedIndices = _presenter.GetSelectedIndices();
        }

        ClosePopup();
    }

    private void OnPresenterDismissed(object? sender, RoutedEventArgs e)
    {
        ClosePopup();
    }

    private void ClosePopup()
    {
        if (_popup != null)
            _popup.IsOpen = false;
    }

    private void RebuildFlyoutContent()
    {
        if (_flyoutContentGrid == null)
            return;

        _flyoutContentGrid.Children.Clear();
        _flyoutContentGrid.ColumnDefinitions.Clear();
        _valueTextBlocks.Clear();

        var columns = Columns;
        int columnIndex = 0;

        if (columns != null)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    _flyoutContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    var separator = new Rectangle
                    {
                        Width = 1,
                        Height = 18,
                    };
                    separator.SetResourceReference(Shape.FillProperty, "DateTimePickerSeparatorBackground");
                    Grid.SetColumn(separator, columnIndex++);
                    _flyoutContentGrid.Children.Add(separator);
                }

                _flyoutContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var textBlock = new TextBlock
                {
                    Padding = new Thickness(12, 0, 12, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = FontSize,
                    FontWeight = FontWeight,
                };
                Grid.SetColumn(textBlock, columnIndex++);
                _flyoutContentGrid.Children.Add(textBlock);
                _valueTextBlocks.Add(textBlock);
            }
        }

        _flyoutContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        if (_iconTextBlock == null)
        {
            _iconTextBlock = new TextBlock
            {
                Width = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
            };
            _iconTextBlock.SetResourceReference(TextBlock.FontFamilyProperty, "FluentSystemIcons");
            _iconTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "DateTimePickerIconForeground");
        }

        _iconTextBlock.Text = IconGlyph;
        Grid.SetColumn(_iconTextBlock, columnIndex);
        _flyoutContentGrid.Children.Add(_iconTextBlock);
    }

    private void UpdateValueTextBlocks()
    {
        var columns = Columns;
        if (columns == null)
            return;

        for (int i = 0; i < _valueTextBlocks.Count; i++)
        {
            string text;
            if (SelectedValues != null && i < SelectedValues.Length)
            {
                text = SelectedValues[i];
            }
            else if (SelectedIndices != null
                     && i < SelectedIndices.Length
                     && i < columns.Count
                     && SelectedIndices[i] >= 0
                     && SelectedIndices[i] < columns[i].Items.Count)
            {
                text = columns[i].Items[SelectedIndices[i]];
            }
            else
            {
                text = columns[i].Placeholder ?? string.Empty;
            }

            _valueTextBlocks[i].Text = text;
        }
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValuePicker picker)
        {
            picker.RebuildFlyoutContent();
            picker.UpdateValueTextBlocks();
        }
    }

    private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValuePicker picker)
        {
            picker.UpdateValueTextBlocks();
            picker.RaiseEvent(new RoutedEventArgs(SelectedValuesChangedEvent, picker));
        }
    }

    private static void OnSelectedIndicesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValuePicker picker)
            picker.UpdateValueTextBlocks();
    }

    private static void OnIconGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValuePicker picker && picker._iconTextBlock != null)
            picker._iconTextBlock.Text = picker.IconGlyph;
    }
}
