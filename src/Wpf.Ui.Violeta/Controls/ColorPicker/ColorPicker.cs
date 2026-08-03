using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Presents a color for user editing using a spectrum, palette and component sliders within a drop down.
/// Editing is available when the drop down flyout is opened; otherwise, only the preview content area is shown.
/// </summary>
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
[TemplatePart(Name = PART_FlyoutButton, Type = typeof(ToggleButton))]
public class ColorPicker : ColorView
{
    public const string PART_Popup = "PART_Popup";
    public const string PART_FlyoutButton = "PART_FlyoutButton";

    private Popup? _popup;
    private Window? _parentWindow;
    private bool _windowHandlerRegistered;

    static ColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    public ColorPicker()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(ColorPicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(
            nameof(ContentTemplate), typeof(DataTemplate), typeof(ColorPicker),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(
            nameof(IsDropDownOpen), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsDropDownOpenChanged));

    /// <summary>
    /// Gets or sets any content displayed in the ColorPicker's preview content area.
    /// </summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display the content of the ColorPicker's preview content area.
    /// </summary>
    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the color editing drop-down is open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _popup = GetTemplateChild(PART_Popup) as Popup;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _parentWindow = Window.GetWindow(this);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        UnregisterWindowHandler();
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

    /// <summary>
    /// While the popup is open, block mouse-wheel events that don't occur over the popup
    /// content itself, so the host panel behind the control can't be scrolled — matching
    /// the standard <see cref="System.Windows.Controls.ComboBox"/> drop-down behavior.
    /// </summary>
    private void OnWindowPreviewMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
            return;

        if (_popup?.Child is UIElement popupChild && IsVisualDescendantOf(target, popupChild))
            return;

        e.Handled = true;
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (ColorPicker)d;
        if ((bool)e.NewValue)
            picker.RegisterWindowHandler();
        else
            picker.UnregisterWindowHandler();
    }

    /// <summary>
    /// Window-level preview handler for light-dismiss: closes the popup when
    /// the user clicks outside both the header and the popup content.
    /// </summary>
    private void OnWindowPreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject target)
            return;

        // Click inside our own header area → let the ToggleButton handle toggle
        if (IsVisualDescendantOf(target, this))
            return;

        // Click inside popup content → ColorView / sliders handle it themselves
        if (_popup?.Child is UIElement popupChild && IsVisualDescendantOf(target, popupChild))
            return;

        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static bool IsVisualDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current == ancestor)
                return true;
            current = VisualTreeHelper.GetParent(current);
        }

        return false;
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
        else if (!IsDropDownOpen && (e.Key == Key.Return || e.Key == Key.Space))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Tab)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }
}
