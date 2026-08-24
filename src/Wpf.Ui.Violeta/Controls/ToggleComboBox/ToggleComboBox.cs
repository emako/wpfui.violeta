using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Border = System.Windows.Controls.Border;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A split control: primary area toggles <see cref="IsChecked"/> / raises <see cref="Click"/>;
/// chevron opens a ComboBox drop-down for single-item selection. Drop-down open, dismiss, and
/// animation behavior come from <see cref="System.Windows.Controls.ComboBox"/>.
/// </summary>
[TemplatePart(Name = TemplateElementToggle, Type = typeof(Border))]
[TemplatePart(Name = TemplateElementToggleButton, Type = typeof(ToggleButton))]
public class ToggleComboBox : System.Windows.Controls.ComboBox
{
    private const string TemplateElementToggle = "PART_Toggle";
    private const string TemplateElementToggleButton = "PART_ToggleButton";

    private Border? _chevronBorder;
    private object? _contentBeforeSync;

    /// <summary>Gets or sets the control responsible for toggling the drop-down.</summary>
    protected ToggleButton? ChevronToggleButton { get; set; }

    static ToggleComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToggleComboBox),
            new FrameworkPropertyMetadata(typeof(ToggleComboBox)));

        BackgroundProperty.OverrideMetadata(
            typeof(ToggleComboBox),
            new FrameworkPropertyMetadata(OnChromeBackgroundChanged));
    }

    private static void OnChromeBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var toggleComboBox = (ToggleComboBox)d;
        toggleComboBox.CoerceValue(MouseOverSecondaryBackgroundProperty);
        toggleComboBox.CoerceValue(PressedSecondaryBackgroundProperty);
    }

    private static void OnCheckedBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var toggleComboBox = (ToggleComboBox)d;
        toggleComboBox.CoerceValue(CheckedSecondaryBackgroundProperty);
        toggleComboBox.CoerceValue(CheckedSecondaryPressedBackgroundProperty);
    }

    private static object? CoerceSecondaryBackground(DependencyObject d, object? baseValue) =>
        baseValue ?? ((ToggleComboBox)d).Background;

    private static object? CoerceCheckedSecondaryBackground(DependencyObject d, object? baseValue) =>
        baseValue ?? ((ToggleComboBox)d).CheckedBackground;

    public ToggleComboBox()
    {
        IsEditable = false;
    }

    #region Dependency properties

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(
            new CornerRadius(4),
            FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(ToggleComboBox),
        new PropertyMetadata(null));

    /// <summary>Primary label shown on the left split (not the ComboBox selection box).</summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(object),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
        nameof(ContentTemplate),
        typeof(DataTemplate),
        typeof(ToggleComboBox),
        new PropertyMetadata(null));

    public static readonly DependencyProperty IsCheckedProperty = ToggleButton.IsCheckedProperty.AddOwner(
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
            OnIsCheckedChanged));

    public static readonly DependencyPropertyKey IsPrimaryPressedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsPrimaryPressed),
            typeof(bool),
            typeof(ToggleComboBox),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsPrimaryPressedProperty =
        IsPrimaryPressedPropertyKey.DependencyProperty;

    public static readonly DependencyProperty CommandProperty = ButtonBase.CommandProperty.AddOwner(
        typeof(ToggleComboBox));

    public static readonly DependencyProperty CommandParameterProperty =
        ButtonBase.CommandParameterProperty.AddOwner(typeof(ToggleComboBox));

    public static readonly DependencyProperty DoubleCommandProperty = DependencyProperty.Register(
        nameof(DoubleCommand),
        typeof(ICommand),
        typeof(ToggleComboBox),
        new PropertyMetadata(null));

    public static readonly DependencyProperty DoubleCommandParameterProperty = DependencyProperty.Register(
        nameof(DoubleCommandParameter),
        typeof(object),
        typeof(ToggleComboBox),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SyncContentWithSelectionProperty = DependencyProperty.Register(
        nameof(SyncContentWithSelection),
        typeof(bool),
        typeof(ToggleComboBox),
        new PropertyMetadata(false, OnSyncContentWithSelectionChanged));

    public static readonly DependencyProperty IsSelectionCancelableProperty = DependencyProperty.Register(
        nameof(IsSelectionCancelable),
        typeof(bool),
        typeof(ToggleComboBox),
        new PropertyMetadata(true));

    public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register(
        nameof(MouseOverBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.Register(
        nameof(MouseOverBorderBrush),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
        nameof(PressedBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PressedBorderBrushProperty = DependencyProperty.Register(
        nameof(PressedBorderBrush),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Background for the non-hovered segment while the other segment is hovered (unchecked).
    /// Falls back to <see cref="System.Windows.Controls.Control.Background"/> when unset.
    /// </summary>
    public static readonly DependencyProperty MouseOverSecondaryBackgroundProperty = DependencyProperty.Register(
        nameof(MouseOverSecondaryBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            CoerceSecondaryBackground));

    /// <summary>
    /// Background for the non-pressed segment while the other segment is pressed (unchecked).
    /// Falls back to <see cref="System.Windows.Controls.Control.Background"/> when unset.
    /// </summary>
    public static readonly DependencyProperty PressedSecondaryBackgroundProperty = DependencyProperty.Register(
        nameof(PressedSecondaryBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            CoerceSecondaryBackground));

    public static readonly DependencyProperty CheckedBackgroundProperty = DependencyProperty.Register(
        nameof(CheckedBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, OnCheckedBackgroundChanged));

    public static readonly DependencyProperty CheckedPointerOverBackgroundProperty = DependencyProperty.Register(
        nameof(CheckedPointerOverBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CheckedPressedBackgroundProperty = DependencyProperty.Register(
        nameof(CheckedPressedBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Background for the non-hovered segment while the other segment is hovered (checked).
    /// Falls back to <see cref="CheckedBackground"/> when unset.
    /// </summary>
    public static readonly DependencyProperty CheckedSecondaryBackgroundProperty = DependencyProperty.Register(
        nameof(CheckedSecondaryBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            CoerceCheckedSecondaryBackground));

    /// <summary>
    /// Background for the non-pressed segment while the other segment is pressed (checked).
    /// Falls back to <see cref="CheckedBackground"/> when unset.
    /// </summary>
    public static readonly DependencyProperty CheckedSecondaryPressedBackgroundProperty = DependencyProperty.Register(
        nameof(CheckedSecondaryPressedBackground),
        typeof(Brush),
        typeof(ToggleComboBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            null,
            CoerceCheckedSecondaryBackground));

    [Bindable(true)]
    [Category("Appearance")]
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    [Bindable(true)]
    [Category("Content")]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    [Bindable(true)]
    [Category("Content")]
    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public bool? IsChecked
    {
        get => (bool?)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>True while the primary (left) area is pressed ??for template chrome.</summary>
    [Browsable(false)]
    public bool IsPrimaryPressed => (bool)GetValue(IsPrimaryPressedProperty);

    [Bindable(true)]
    [Category("Action")]
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    [Bindable(true)]
    [Category("Action")]
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    [Bindable(true)]
    [Category("Action")]
    public ICommand? DoubleCommand
    {
        get => (ICommand?)GetValue(DoubleCommandProperty);
        set => SetValue(DoubleCommandProperty, value);
    }

    [Bindable(true)]
    [Category("Action")]
    public object? DoubleCommandParameter
    {
        get => GetValue(DoubleCommandParameterProperty);
        set => SetValue(DoubleCommandParameterProperty, value);
    }

    [Bindable(true)]
    [Category("Behavior")]
    public bool SyncContentWithSelection
    {
        get => (bool)GetValue(SyncContentWithSelectionProperty);
        set => SetValue(SyncContentWithSelectionProperty, value);
    }

    [Bindable(true)]
    [Category("Behavior")]
    public bool IsSelectionCancelable
    {
        get => (bool)GetValue(IsSelectionCancelableProperty);
        set => SetValue(IsSelectionCancelableProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? MouseOverBackground
    {
        get => (Brush?)GetValue(MouseOverBackgroundProperty);
        set => SetValue(MouseOverBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? MouseOverBorderBrush
    {
        get => (Brush?)GetValue(MouseOverBorderBrushProperty);
        set => SetValue(MouseOverBorderBrushProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? PressedBackground
    {
        get => (Brush?)GetValue(PressedBackgroundProperty);
        set => SetValue(PressedBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? PressedBorderBrush
    {
        get => (Brush?)GetValue(PressedBorderBrushProperty);
        set => SetValue(PressedBorderBrushProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? MouseOverSecondaryBackground
    {
        get => (Brush?)GetValue(MouseOverSecondaryBackgroundProperty);
        set => SetValue(MouseOverSecondaryBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? PressedSecondaryBackground
    {
        get => (Brush?)GetValue(PressedSecondaryBackgroundProperty);
        set => SetValue(PressedSecondaryBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? CheckedBackground
    {
        get => (Brush?)GetValue(CheckedBackgroundProperty);
        set => SetValue(CheckedBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? CheckedPointerOverBackground
    {
        get => (Brush?)GetValue(CheckedPointerOverBackgroundProperty);
        set => SetValue(CheckedPointerOverBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? CheckedPressedBackground
    {
        get => (Brush?)GetValue(CheckedPressedBackgroundProperty);
        set => SetValue(CheckedPressedBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? CheckedSecondaryBackground
    {
        get => (Brush?)GetValue(CheckedSecondaryBackgroundProperty);
        set => SetValue(CheckedSecondaryBackgroundProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public Brush? CheckedSecondaryPressedBackground
    {
        get => (Brush?)GetValue(CheckedSecondaryPressedBackgroundProperty);
        set => SetValue(CheckedSecondaryPressedBackgroundProperty, value);
    }

    #endregion Dependency properties

    #region Routed events

    public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(ToggleComboBox));

    public static readonly RoutedEvent CheckedEvent = ToggleButton.CheckedEvent.AddOwner(typeof(ToggleComboBox));

    public static readonly RoutedEvent UncheckedEvent = ToggleButton.UncheckedEvent.AddOwner(typeof(ToggleComboBox));

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    public event RoutedEventHandler Checked
    {
        add => AddHandler(CheckedEvent, value);
        remove => RemoveHandler(CheckedEvent, value);
    }

    public event RoutedEventHandler Unchecked
    {
        add => AddHandler(UncheckedEvent, value);
        remove => RemoveHandler(UncheckedEvent, value);
    }

    #endregion Routed events

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ChevronToggleButton = GetTemplateChild(TemplateElementToggleButton) as ToggleButton;
        _chevronBorder = GetTemplateChild(TemplateElementToggle) as Border;
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        ApplyContentSync();
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        // Cancelable re-click of the selected drop-down item.
        if (IsSelectionCancelable && IsDropDownOpen && e.OriginalSource is DependencyObject source)
        {
            var item = FindComboBoxItem(source);
            if (item is not null)
            {
                var data = ItemContainerGenerator.ItemFromContainer(item);
                if (data == DependencyProperty.UnsetValue)
                {
                    data = item.Content;
                }

                if (Equals(data, SelectedItem))
                {
                    SetCurrentValue(SelectedItemProperty, null);
                    SetCurrentValue(IsDropDownOpenProperty, false);
                    e.Handled = true;
                    return;
                }
            }
        }

        if (IsOverToggle(e.GetPosition(this)))
        {
            // Let ComboBox + chevron ToggleButton drive IsDropDownOpen.
            base.OnPreviewMouseLeftButtonDown(e);
            return;
        }

        if (IsOverPrimary(e.GetPosition(this)))
        {
            e.Handled = true;
            SetValue(IsPrimaryPressedPropertyKey, true);
            Focus();
            return;
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        var wasPrimaryPressed = IsPrimaryPressed;
        if (wasPrimaryPressed)
        {
            SetValue(IsPrimaryPressedPropertyKey, false);
        }

        if (IsOverToggle(e.GetPosition(this)))
        {
            base.OnPreviewMouseLeftButtonUp(e);
            return;
        }

        if (wasPrimaryPressed && IsOverPrimary(e.GetPosition(this)))
        {
            e.Handled = true;
            OnPrimaryClick();
            return;
        }

        base.OnPreviewMouseLeftButtonUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (IsPrimaryPressed)
        {
            SetValue(IsPrimaryPressedPropertyKey, false);
        }
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (e.ChangedButton != MouseButton.Left || e.Handled)
        {
            return;
        }

        if (IsOverToggle(e.GetPosition(this)) || !IsOverPrimary(e.GetPosition(this)))
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

    private void OnPrimaryClick()
    {
        SetCurrentValue(IsCheckedProperty, IsChecked != true);
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));

        var parameter = CommandParameter;
        var command = Command;
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToggleComboBox)d;
        if (e.NewValue is true)
        {
            control.RaiseEvent(new RoutedEventArgs(CheckedEvent, control));
        }
        else
        {
            control.RaiseEvent(new RoutedEventArgs(UncheckedEvent, control));
        }
    }

    private static void OnSyncContentWithSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ToggleComboBox)d;
        if (e.NewValue is true)
        {
            control._contentBeforeSync = control.Content;
            control.ApplyContentSync();
        }
        else if (control._contentBeforeSync is not null)
        {
            control.SetCurrentValue(ContentProperty, control._contentBeforeSync);
        }
    }

    private void ApplyContentSync()
    {
        if (!SyncContentWithSelection)
        {
            return;
        }

        SetCurrentValue(ContentProperty, GetDisplayText(SelectedItem));
    }

    private object? GetDisplayText(object? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var value = GetPropertyValue(item, DisplayMemberPath);
            return value?.ToString() ?? string.Empty;
        }

        return item is string s ? s : item.ToString() ?? string.Empty;
    }

    private static object? GetPropertyValue(object item, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return item;
        }

        object? current = item;
        foreach (var segment in path.Split('.'))
        {
            if (current is null)
            {
                return null;
            }

            var property = current.GetType().GetProperty(
                segment,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property is null)
            {
                return null;
            }

            current = property.GetValue(current);
        }

        return current;
    }

    private bool IsOverToggle(Point positionRelativeToThis)
    {
        if (_chevronBorder is null)
        {
            return false;
        }

        var origin = _chevronBorder.TranslatePoint(new Point(0, 0), this);
        return new Rect(origin, _chevronBorder.RenderSize).Contains(positionRelativeToThis);
    }

    private bool IsOverPrimary(Point positionRelativeToThis)
    {
        if (_chevronBorder is null)
        {
            return new Rect(RenderSize).Contains(positionRelativeToThis);
        }

        var toggleOrigin = _chevronBorder.TranslatePoint(new Point(0, 0), this);
        return positionRelativeToThis.X < toggleOrigin.X
            && positionRelativeToThis.X >= 0
            && positionRelativeToThis.Y >= 0
            && positionRelativeToThis.Y <= ActualHeight;
    }

    private static ComboBoxItem? FindComboBoxItem(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ComboBoxItem item)
            {
                return item;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }
}
