using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;
using Border = System.Windows.Controls.Border;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A <see cref="ToggleButton"/> with a primary content area and a separate chevron that opens a
/// ComboBox-like item drop-down. Primary click toggles <see cref="ToggleButton.IsChecked"/> and
/// invokes <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/>; double-click on
/// the primary area invokes <see cref="DoubleCommand"/>.
/// </summary>
[TemplatePart(Name = TemplateElementToggle, Type = typeof(Border))]
[TemplatePart(Name = TemplateElementToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = TemplateElementPopup, Type = typeof(Popup))]
[TemplatePart(Name = TemplateElementItemsHost, Type = typeof(ListBox))]
[TemplatePart(Name = ChevronHostPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = ChevronIconPart, Type = typeof(UIElement))]
public class SplitToggleButton : ToggleButton
{
    private const string TemplateElementToggle = "PART_Toggle";
    private const string TemplateElementToggleButton = "PART_ToggleButton";
    private const string TemplateElementPopup = "PART_Popup";
    private const string TemplateElementItemsHost = "PART_ItemsHost";
    private const string ChevronHostPart = "PART_ChevronHost";
    private const string ChevronIconPart = "PART_ChevronIcon";

    /// <summary>
    /// Fraction of the clipped chevron viewport translated on press.
    /// Matched to <see cref="DropDownButton"/> / <see cref="SplitButton"/>.
    /// </summary>
    private const double PressDepthRatio = 0.18;

    /// <summary>Upward overshoot on release, relative to viewport height.</summary>
    private const double OvershootRatio = 0.10;

    private Border? _splitToggleBorder;
    private Popup? _popup;
    private ListBox? _itemsHost;
    private Window? _parentWindow;
    private bool _windowHandlerRegistered;
    private bool _syncingSelection;
    private object? _contentBeforeSync;
    private TranslateTransform? _chevronTranslate;
    private FrameworkElement? _chevronHost;
    private bool _playChevronReleaseOnUp;

    /// <summary>Gets or sets the control responsible for toggling the drop-down.</summary>
    protected ToggleButton? SplitChevronToggleButton { get; set; }

    static SplitToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SplitToggleButton),
            new FrameworkPropertyMetadata(typeof(SplitToggleButton)));
    }

    public SplitToggleButton()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region Dependency properties

    /// <summary>Identifies the <see cref="CornerRadius"/> dependency property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(SplitToggleButton),
        new FrameworkPropertyMetadata(
            new CornerRadius(4),
            FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(SplitToggleButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen),
        typeof(bool),
        typeof(SplitToggleButton),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnIsDropDownOpenChanged));

    /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(SplitToggleButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="ItemTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(SplitToggleButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="DisplayMemberPath"/> dependency property.</summary>
    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
        nameof(DisplayMemberPath),
        typeof(string),
        typeof(SplitToggleButton),
        new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="SelectedItem"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(object),
        typeof(SplitToggleButton),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedItemChanged));

    /// <summary>Identifies the <see cref="SelectedIndex"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex),
        typeof(int),
        typeof(SplitToggleButton),
        new FrameworkPropertyMetadata(
            -1,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedIndexChanged));

    /// <summary>Identifies the <see cref="MaxDropDownHeight"/> dependency property.</summary>
    public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
        nameof(MaxDropDownHeight),
        typeof(double),
        typeof(SplitToggleButton),
        new PropertyMetadata(400.0));

    /// <summary>
    /// Identifies the <see cref="SyncContentWithSelection"/> dependency property.
    /// When <c>true</c>, <see cref="ContentControl.Content"/> follows <see cref="SelectedItem"/>.
    /// Default is <c>false</c>.
    /// </summary>
    public static readonly DependencyProperty SyncContentWithSelectionProperty = DependencyProperty.Register(
        nameof(SyncContentWithSelection),
        typeof(bool),
        typeof(SplitToggleButton),
        new PropertyMetadata(false, OnSyncContentWithSelectionChanged));

    /// <summary>
    /// Identifies the <see cref="IsSelectionCancelable"/> dependency property.
    /// When <c>true</c>, clicking the currently selected item clears the selection.
    /// </summary>
    public static readonly DependencyProperty IsSelectionCancelableProperty = DependencyProperty.Register(
        nameof(IsSelectionCancelable),
        typeof(bool),
        typeof(SplitToggleButton),
        new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="DoubleCommand"/> dependency property.</summary>
    public static readonly DependencyProperty DoubleCommandProperty = DependencyProperty.Register(
        nameof(DoubleCommand),
        typeof(ICommand),
        typeof(SplitToggleButton),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="DoubleCommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty DoubleCommandParameterProperty = DependencyProperty.Register(
        nameof(DoubleCommandParameter),
        typeof(object),
        typeof(SplitToggleButton),
        new PropertyMetadata(null));

    /// <summary>Gets or sets the corner radius of the control chrome.</summary>
    [Bindable(true)]
    [Category("Appearance")]
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Gets or sets the icon displayed before the content.</summary>
    [Bindable(true)]
    [Category("Appearance")]
    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
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

    /// <summary>Gets or sets the collection used to generate drop-down items.</summary>
    [Bindable(true)]
    [Category("Content")]
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Gets or sets the data template for drop-down items.</summary>
    [Bindable(true)]
    [Category("Content")]
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Gets or sets a path to a value on the source object for display.</summary>
    [Bindable(true)]
    [Category("Content")]
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>Gets or sets the currently selected item.</summary>
    [Bindable(true)]
    [Category("Appearance")]
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Gets or sets the index of the selected item, or <c>-1</c> when none.</summary>
    [Bindable(true)]
    [Category("Appearance")]
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Gets or sets the maximum height of the drop-down.</summary>
    [Bindable(true)]
    [Category("Layout")]
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets whether <see cref="ContentControl.Content"/> automatically follows
    /// <see cref="SelectedItem"/>. Default is <c>false</c>.
    /// </summary>
    [Bindable(true)]
    [Category("Behavior")]
    public bool SyncContentWithSelection
    {
        get => (bool)GetValue(SyncContentWithSelectionProperty);
        set => SetValue(SyncContentWithSelectionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether clicking the currently selected drop-down item clears the selection.
    /// </summary>
    [Bindable(true)]
    [Category("Behavior")]
    public bool IsSelectionCancelable
    {
        get => (bool)GetValue(IsSelectionCancelableProperty);
        set => SetValue(IsSelectionCancelableProperty, value);
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

    #endregion Dependency properties

    /// <summary>Identifies the <see cref="SelectionChanged"/> routed event.</summary>
    public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectionChanged),
        RoutingStrategy.Bubble,
        typeof(SelectionChangedEventHandler),
        typeof(SplitToggleButton));

    /// <summary>Occurs when the selected item changes.</summary>
    public event SelectionChangedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        ReleaseTemplateResources();

        base.OnApplyTemplate();

        SplitChevronToggleButton = GetTemplateChild(TemplateElementToggleButton) as ToggleButton
            ?? throw new NullReferenceException(
                $"Element {TemplateElementToggleButton} of type {typeof(ToggleButton)} not found in {typeof(SplitToggleButton)}");

        _splitToggleBorder = GetTemplateChild(TemplateElementToggle) as Border;
        _popup = GetTemplateChild(TemplateElementPopup) as Popup;
        _itemsHost = GetTemplateChild(TemplateElementItemsHost) as ListBox;
        _chevronHost =
            GetTemplateChild(ChevronHostPart) as FrameworkElement
            ?? SplitChevronToggleButton.Content as FrameworkElement;
        _chevronTranslate = null;

        var chevron =
            GetTemplateChild(ChevronIconPart) as UIElement
            ?? (_chevronHost as Decorator)?.Child;

        if (chevron is not null)
        {
            // Template Freezables are immutable — always install a fresh transform.
            _chevronTranslate = new TranslateTransform();
            chevron.RenderTransform = _chevronTranslate;
            chevron.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        AttachTemplateHandlers();
        SyncItemsHostSelection();
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

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
        {
            return;
        }

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
    }

    /// <summary>Releases template event handlers.</summary>
    protected virtual void ReleaseTemplateResources()
    {
        if (SplitChevronToggleButton is not null)
        {
            SplitChevronToggleButton.PreviewMouseLeftButtonDown -= OnChevronPreviewMouseLeftButtonDown;
            SplitChevronToggleButton.PreviewMouseLeftButtonUp -= OnChevronPreviewMouseLeftButtonUp;
            SplitChevronToggleButton.LostMouseCapture -= OnChevronLostMouseCapture;
        }

        if (_itemsHost is not null)
        {
            _itemsHost.SelectionChanged -= OnItemsHostSelectionChanged;
            _itemsHost.PreviewMouseLeftButtonDown -= OnItemsHostPreviewMouseLeftButtonDown;
        }

        _playChevronReleaseOnUp = false;
        _chevronTranslate = null;
        _chevronHost = null;
    }

    private void AttachTemplateHandlers()
    {
        if (SplitChevronToggleButton is not null)
        {
            SplitChevronToggleButton.PreviewMouseLeftButtonDown -= OnChevronPreviewMouseLeftButtonDown;
            SplitChevronToggleButton.PreviewMouseLeftButtonUp -= OnChevronPreviewMouseLeftButtonUp;
            SplitChevronToggleButton.LostMouseCapture -= OnChevronLostMouseCapture;

            SplitChevronToggleButton.PreviewMouseLeftButtonDown += OnChevronPreviewMouseLeftButtonDown;
            SplitChevronToggleButton.PreviewMouseLeftButtonUp += OnChevronPreviewMouseLeftButtonUp;
            SplitChevronToggleButton.LostMouseCapture += OnChevronLostMouseCapture;
        }

        if (_itemsHost is not null)
        {
            _itemsHost.SelectionChanged -= OnItemsHostSelectionChanged;
            _itemsHost.SelectionChanged += OnItemsHostSelectionChanged;
            _itemsHost.PreviewMouseLeftButtonDown -= OnItemsHostPreviewMouseLeftButtonDown;
            _itemsHost.PreviewMouseLeftButtonDown += OnItemsHostPreviewMouseLeftButtonDown;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _parentWindow = Window.GetWindow(this);
        if (SplitChevronToggleButton is not null)
        {
            AttachTemplateHandlers();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ReleaseTemplateResources();
        UnregisterWindowHandler();
    }

    private void OnChevronPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Prevent the host ToggleButton from toggling IsChecked when the chevron is clicked.
        e.Handled = true;

        // WinUI: dismiss while open has no press chrome / chevron motion.
        if (IsDropDownOpen)
        {
            _playChevronReleaseOnUp = false;
            SetCurrentValue(IsDropDownOpenProperty, false);
            return;
        }

        BeginChevronPressAnimation();
        _playChevronReleaseOnUp = true;
        SetCurrentValue(IsDropDownOpenProperty, true);
    }

    private void OnChevronPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        if (!_playChevronReleaseOnUp)
        {
            return;
        }

        _playChevronReleaseOnUp = false;
        BeginChevronReleaseAnimation();
    }

    private void OnChevronLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (_playChevronReleaseOnUp)
        {
            _playChevronReleaseOnUp = false;
            BeginChevronReleaseAnimation();
        }
        else if (_chevronTranslate is not null && Math.Abs(_chevronTranslate.Y) > 0.01)
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

    private void OnItemsHostPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsSelectionCancelable || _itemsHost is null)
        {
            return;
        }

        var item = FindListBoxItem(e.OriginalSource as DependencyObject);
        if (item is null)
        {
            return;
        }

        var data = item.DataContext ?? item.Content;
        if (Equals(data, SelectedItem))
        {
            SetCurrentValue(SelectedItemProperty, null);
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
    }

    private void OnItemsHostSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TwoWay SelectedItem binding updates the DP; close the drop-down after a pick.
        if (e.AddedItems.Count > 0)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SplitToggleButton)d;
        if (e.NewValue is true)
        {
            control.RegisterWindowHandler();
        }
        else
        {
            control.UnregisterWindowHandler();
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SplitToggleButton)d;
        if (!control._syncingSelection)
        {
            control._syncingSelection = true;
            try
            {
                control.SyncItemsHostSelection();
                control.SetCurrentValue(SelectedIndexProperty, control.IndexOfItem(e.NewValue));
            }
            finally
            {
                control._syncingSelection = false;
            }
        }

        control.ApplyContentSync();
        control.RaiseEvent(
            new SelectionChangedEventArgs(
                SelectionChangedEvent,
                e.OldValue is null ? Array.Empty<object>() : [e.OldValue],
                e.NewValue is null ? Array.Empty<object>() : [e.NewValue]));
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SplitToggleButton)d;
        if (control._syncingSelection)
        {
            return;
        }

        control._syncingSelection = true;
        try
        {
            var index = e.NewValue is int i ? i : -1;
            var item = control.ItemAt(index);
            control.SetCurrentValue(SelectedItemProperty, item);
            control.SyncItemsHostSelection();
        }
        finally
        {
            control._syncingSelection = false;
        }
    }

    private static void OnSyncContentWithSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SplitToggleButton)d;
        if (e.NewValue is true)
        {
            control._contentBeforeSync = control.Content;
            control.ApplyContentSync();
        }
        else if (control._contentBeforeSync is not null || control.ReadLocalValue(ContentProperty) != DependencyProperty.UnsetValue)
        {
            // Restore the content that was present before syncing started, when possible.
            if (control._contentBeforeSync is not null)
            {
                control.SetCurrentValue(ContentProperty, control._contentBeforeSync);
            }
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

    private void SyncItemsHostSelection()
    {
        if (_itemsHost is null)
        {
            return;
        }

        if (!Equals(_itemsHost.SelectedItem, SelectedItem))
        {
            _itemsHost.SelectedItem = SelectedItem;
        }
    }

    private object? ItemAt(int index)
    {
        if (index < 0 || ItemsSource is null)
        {
            return null;
        }

        var i = 0;
        foreach (var item in ItemsSource)
        {
            if (i == index)
            {
                return item;
            }

            i++;
        }

        return null;
    }

    private int IndexOfItem(object? target)
    {
        if (target is null || ItemsSource is null)
        {
            return -1;
        }

        var i = 0;
        foreach (var item in ItemsSource)
        {
            if (Equals(item, target))
            {
                return i;
            }

            i++;
        }

        return -1;
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

        if (ItemTemplate is not null)
        {
            // Keep a readable fallback; templates are used in the drop-down list.
            return item.ToString() ?? string.Empty;
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

            var type = current.GetType();
            var property = type.GetProperty(segment, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property is null)
            {
                return null;
            }

            current = property.GetValue(current);
        }

        return current;
    }

    private static ListBoxItem? FindListBoxItem(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ListBoxItem item)
            {
                return item;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }

    private bool IsOverToggle(Point positionRelativeToThis)
    {
        if (_splitToggleBorder is null)
        {
            return false;
        }

        var toggleOrigin = _splitToggleBorder.TranslatePoint(new Point(0, 0), this);
        var bounds = new Rect(toggleOrigin, _splitToggleBorder.RenderSize);
        return bounds.Contains(positionRelativeToThis);
    }

    private void RegisterWindowHandler()
    {
        _parentWindow ??= Window.GetWindow(this);

        if (_parentWindow is not null && !_windowHandlerRegistered)
        {
            _parentWindow.PreviewMouseLeftButtonDown += OnWindowPreviewMouseLeftButtonDown;
            _parentWindow.PreviewMouseWheel += OnWindowPreviewMouseWheel;
            _windowHandlerRegistered = true;
        }
    }

    private void UnregisterWindowHandler()
    {
        if (_parentWindow is not null && _windowHandlerRegistered)
        {
            _parentWindow.PreviewMouseLeftButtonDown -= OnWindowPreviewMouseLeftButtonDown;
            _parentWindow.PreviewMouseWheel -= OnWindowPreviewMouseWheel;
            _windowHandlerRegistered = false;
        }
    }

    private void OnWindowPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
        {
            return;
        }

        if (_popup?.Child is UIElement popupChild && IsVisualDescendantOf(target, popupChild))
        {
            return;
        }

        e.Handled = true;
    }

    private void OnWindowPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
        {
            return;
        }

        if (IsVisualDescendantOf(target, this))
        {
            return;
        }

        if (_popup?.Child is UIElement popupChild && IsVisualDescendantOf(target, popupChild))
        {
            return;
        }

        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static bool IsVisualDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current == ancestor)
            {
                return true;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return false;
    }
}
