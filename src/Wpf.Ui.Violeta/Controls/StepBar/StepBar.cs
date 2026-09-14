using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A step progress bar that visualizes a multi-step process.
/// </summary>
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(StepBarItem))]
[DefaultEvent(nameof(StepChanged))]
[TemplatePart(Name = ElementProgressBarBack, Type = typeof(ProgressBar))]
public class StepBar : ItemsControl
{
    private const string ElementProgressBarBack = "PART_ProgressBarBack";

    private ProgressBar? _progressBarBack;
    private int _oriStepIndex = -1;

    static StepBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepBar),
            new FrameworkPropertyMetadata(typeof(StepBar)));
    }

    public StepBar()
    {
        CommandBindings.Add(new CommandBinding(StepBarCommands.Next, (_, _) => Next()));
        CommandBindings.Add(new CommandBinding(StepBarCommands.Prev, (_, _) => Prev()));

        ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        AddHandler(StepBarItem.SelectedEvent, new RoutedEventHandler(OnStepBarItemSelected));
    }

    /// <summary>
    /// Bubbling event raised when <see cref="StepIndex"/> changes.
    /// </summary>
    public static readonly RoutedEvent StepChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(StepChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof(StepBar));

    /// <summary>
    /// Raised when <see cref="StepIndex"/> changes.
    /// </summary>
    [Category("Behavior")]
    public event RoutedPropertyChangedEventHandler<int> StepChanged
    {
        add => AddHandler(StepChangedEvent, value);
        remove => RemoveHandler(StepChangedEvent, value);
    }

    /// <summary>
    /// 0-based index of the current step. Supports two-way binding.
    /// </summary>
    public static readonly DependencyProperty StepIndexProperty =
        DependencyProperty.Register(
            nameof(StepIndex),
            typeof(int),
            typeof(StepBar),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnStepIndexChanged,
                CoerceStepIndex));

    /// <summary>
    /// 0-based index of the current step. Supports two-way binding.
    /// </summary>
    public int StepIndex
    {
        get => (int)GetValue(StepIndexProperty);
        set => SetValue(StepIndexProperty, value);
    }

    /// <summary>
    /// Placement of step labels relative to the indicator.
    /// Top/Bottom = horizontal; Left/Right = vertical.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(
            nameof(Dock),
            typeof(Dock),
            typeof(StepBar),
            new PropertyMetadata(Dock.Top));

    /// <summary>
    /// Placement of step labels relative to the indicator.
    /// Top/Bottom = horizontal; Left/Right = vertical.
    /// </summary>
    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    /// <summary>
    /// When true, clicking a step item navigates to that step.
    /// </summary>
    public static readonly DependencyProperty IsMouseSelectableProperty =
        DependencyProperty.Register(
            nameof(IsMouseSelectable),
            typeof(bool),
            typeof(StepBar),
            new PropertyMetadata(false));

    /// <summary>
    /// When true, clicking a step item navigates to that step.
    /// </summary>
    public bool IsMouseSelectable
    {
        get => (bool)GetValue(IsMouseSelectableProperty);
        set => SetValue(IsMouseSelectableProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _progressBarBack = GetTemplateChild(ElementProgressBarBack) as ProgressBar;
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is StepBarItem;

    protected override DependencyObject GetContainerForItemOverride() => new StepBarItem();

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        int colCount = Items.Count;
        if (_progressBarBack is null || colCount <= 0)
        {
            return;
        }

        if (Dock is Dock.Top or Dock.Bottom)
        {
            _progressBarBack.Width = (colCount - 1) * (ActualWidth / colCount);
        }
        else
        {
            _progressBarBack.Height = (colCount - 1) * (ActualHeight / colCount);
        }
    }

    /// <summary>Advance to the next step.</summary>
    public void Next() => StepIndex++;

    /// <summary>Go back to the previous step.</summary>
    public void Prev() => StepIndex--;

    private void OnStepBarItemSelected(object sender, RoutedEventArgs e)
    {
        if (!IsMouseSelectable)
        {
            return;
        }

        if (e.OriginalSource is StepBarItem item)
        {
            SetCurrentValue(StepIndexProperty, item.Index - 1);
        }
    }

    private void ItemContainerGenerator_StatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
        {
            return;
        }

        int count = Items.Count;
        InvalidateVisual();

        if (_progressBarBack is not null)
        {
            _progressBarBack.Maximum = Math.Max(count - 1, 0);
            _progressBarBack.Value = StepIndex;
        }

        if (count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StepBarItem stepBarItem)
            {
                stepBarItem.Index = i + 1;
            }
        }

        if (_oriStepIndex > 0)
        {
            StepIndex = _oriStepIndex;
            _oriStepIndex = -1;
        }
        else
        {
            ApplyStepIndex(StepIndex, raiseEvent: false);
        }
    }

    private static object CoerceStepIndex(DependencyObject d, object baseValue)
    {
        var ctl = (StepBar)d;
        int stepIndex = (int)baseValue;

        if (ctl.Items.Count == 0 && stepIndex > 0)
        {
            ctl._oriStepIndex = stepIndex;
            return 0;
        }

        if (stepIndex < 0)
        {
            return 0;
        }

        if (stepIndex >= ctl.Items.Count)
        {
            return ctl.Items.Count == 0 ? 0 : ctl.Items.Count - 1;
        }

        return baseValue;
    }

    private static void OnStepIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (StepBar)d;
        ctl.ApplyStepIndex((int)e.NewValue, raiseEvent: true, (int)e.OldValue);
    }

    private void ApplyStepIndex(int stepIndex, bool raiseEvent, int oldValue = 0)
    {
        for (int i = 0; i < stepIndex; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StepBarItem finished)
            {
                finished.Status = StepStatus.Complete;
            }
        }

        for (int i = stepIndex + 1; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StepBarItem waiting)
            {
                waiting.Status = StepStatus.Waiting;
            }
        }

        if (ItemContainerGenerator.ContainerFromIndex(stepIndex) is StepBarItem underWay)
        {
            underWay.Status = StepStatus.UnderWay;
        }

        if (_progressBarBack is not null)
        {
            var animation = new DoubleAnimation(stepIndex, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
            };
            _progressBarBack.BeginAnimation(RangeBase.ValueProperty, animation);
        }

        if (raiseEvent)
        {
            RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldValue, stepIndex, StepChangedEvent));
        }
    }
}
