using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class Indicator : Control
{
    public IndicatorType IndicatorType
    {
        get => (IndicatorType)GetValue(IndicatorTypeProperty);
        set => SetValue(IndicatorTypeProperty, value);
    }

    public static readonly DependencyProperty IndicatorTypeProperty
        = DependencyProperty.Register(nameof(IndicatorType), typeof(IndicatorType), typeof(Indicator), new PropertyMetadata(IndicatorType.Twist));

    static Indicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Indicator),
            new FrameworkPropertyMetadata(typeof(Indicator)));
    }

    public override void OnApplyTemplate()
    {
        UpdateVisualState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsVisibleProperty)
        {
            UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        if (Template == null)
        {
            return;
        }

        FrameworkElement mainGrid = (FrameworkElement)GetTemplateChild("MainGrid");
        if (mainGrid == null)
        {
            return;
        }

        string targetState = IsVisible ? "Active" : "Inactive";
        VisualStateManager.GoToElementState(mainGrid, targetState, true);
    }
}
