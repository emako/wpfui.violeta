using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class TransitioningContentControlDoubleValueHolder : DependencyObject
{
    public static readonly DependencyProperty XProperty
        = DependencyProperty.Register(nameof(X), typeof(double), typeof(TransitioningContentControlDoubleValueHolder), new PropertyMetadata(0d));

    public static readonly DependencyProperty YProperty
        = DependencyProperty.Register(nameof(Y), typeof(double), typeof(TransitioningContentControlDoubleValueHolder), new PropertyMetadata(0d));

    public static readonly DependencyProperty ZProperty
        = DependencyProperty.Register(nameof(Z), typeof(double), typeof(TransitioningContentControlDoubleValueHolder), new PropertyMetadata(0d));

    public double X
    {
        get => (double)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public double Y
    {
        get => (double)GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public double Z
    {
        get => (double)GetValue(ZProperty);
        set => SetValue(ZProperty, value);
    }
}
