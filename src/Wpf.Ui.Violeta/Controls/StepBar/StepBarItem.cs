using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A single step item hosted by <see cref="StepBar"/>.
/// </summary>
public class StepBarItem : ContentControl
{
    private bool _isMouseLeftButtonDown;

    static StepBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepBarItem),
            new FrameworkPropertyMetadata(typeof(StepBarItem)));
    }

    /// <summary>
    /// 1-based step number displayed in the indicator.
    /// </summary>
    public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(
            nameof(Index),
            typeof(int),
            typeof(StepBarItem),
            new PropertyMetadata(-1));

    /// <summary>
    /// 1-based step number displayed in the indicator.
    /// </summary>
    public int Index
    {
        get => (int)GetValue(IndexProperty);
        internal set => SetValue(IndexProperty, value);
    }

    /// <summary>
    /// Current status of this step.
    /// </summary>
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(StepStatus),
            typeof(StepBarItem),
            new PropertyMetadata(StepStatus.Waiting));

    /// <summary>
    /// Current status of this step.
    /// </summary>
    public StepStatus Status
    {
        get => (StepStatus)GetValue(StatusProperty);
        internal set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Bubbling event raised when the item is clicked.
    /// </summary>
    public static readonly RoutedEvent SelectedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Selected),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StepBarItem));

    /// <summary>
    /// Raised when the item is clicked.
    /// </summary>
    public event RoutedEventHandler Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        _isMouseLeftButtonDown = false;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        _isMouseLeftButtonDown = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_isMouseLeftButtonDown)
        {
            return;
        }

        _isMouseLeftButtonDown = false;
        RaiseEvent(new RoutedEventArgs(SelectedEvent, this));
    }
}
