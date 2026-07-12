using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Violeta.Threading;

namespace Wpf.Ui.Violeta.Controls;

[TemplateVisualState(Name = BusyMaskVisualStates.StateHidden, GroupName = BusyMaskVisualStates.GroupVisibility)]
[TemplateVisualState(Name = BusyMaskVisualStates.StateVisible, GroupName = BusyMaskVisualStates.GroupVisibility)]
public class BusyMask : ContentControl
{
    [Category("BusyIndicator")]
    [Description("Gets or sets whether the indicator is busy.")]
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(BusyMask), new PropertyMetadata(false, OnIsBusyChanged));

    [Category("BusyIndicator")]
    [Description("Gets or sets indicator content such as waiting message.")]
    public string BusyContent
    {
        get => (string)GetValue(BusyContentProperty);
        set => SetValue(BusyContentProperty, value);
    }

    public static readonly DependencyProperty BusyContentProperty
        = DependencyProperty.Register(nameof(BusyContent), typeof(string), typeof(BusyMask), new PropertyMetadata("Please wait..."));

    [Category("BusyIndicator")]
    [Description("Gets or sets indicator content margin.")]
    public Thickness BusyContentMargin
    {
        get => (Thickness)GetValue(BusyContentMarginProperty);
        set => SetValue(BusyContentMarginProperty, value);
    }

    public static readonly DependencyProperty BusyContentMarginProperty
        = DependencyProperty.Register(nameof(BusyContentMargin), typeof(Thickness), typeof(BusyMask), new PropertyMetadata(new Thickness(10)));

    [Category("BusyIndicator")]
    [Description("Gets or sets the indicator type.")]
    public IndicatorType IndicatorType
    {
        get => (IndicatorType)GetValue(IndicatorTypeProperty);
        set => SetValue(IndicatorTypeProperty, value);
    }

    public static readonly DependencyProperty IndicatorTypeProperty
        = DependencyProperty.Register(nameof(IndicatorType), typeof(IndicatorType), typeof(BusyMask), new PropertyMetadata(IndicatorType.Twist));

    [Category("BusyIndicator")]
    [Description("Gets or sets the control which gets focused after the wait is over.")]
    public Control FocusAfterBusy
    {
        get => (Control)GetValue(FocusAfterBusyProperty);
        set => SetValue(FocusAfterBusyProperty, value);
    }

    public static readonly DependencyProperty FocusAfterBusyProperty
        = DependencyProperty.Register(nameof(FocusAfterBusy), typeof(Control), typeof(BusyMask), new PropertyMetadata(null));

    static BusyMask()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyMask),
            new FrameworkPropertyMetadata(typeof(BusyMask)));
    }

    private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BusyMask)d).OnIsBusyChanged(e);
    }

    protected virtual void OnIsBusyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue)
        {
            FocusAfterBusy?.Dispatcher.Delay(100, (_) =>
            {
                FocusAfterBusy.Focus();
            });
        }

        ChangeVisualState((bool)e.NewValue);
    }

    public override void OnApplyTemplate()
    {
        ChangeVisualState(IsBusy);
    }

    protected virtual void ChangeVisualState(bool isBusyContentVisible = false)
    {
        VisualStateManager.GoToState(this, isBusyContentVisible
            ? BusyMaskVisualStates.StateVisible
            : BusyMaskVisualStates.StateHidden,
        true);
    }
}
