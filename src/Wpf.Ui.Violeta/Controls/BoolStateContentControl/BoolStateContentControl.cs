using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class BoolStateContentControl : ContentControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(bool),
        typeof(BoolStateContentControl),
        new PropertyMetadata(false, OnStateChanged));

    public static readonly DependencyProperty TrueContentProperty = DependencyProperty.Register(
        nameof(TrueContent),
        typeof(object),
        typeof(BoolStateContentControl),
        new PropertyMetadata(null, OnStateChanged));

    public static readonly DependencyProperty FalseContentProperty = DependencyProperty.Register(
        nameof(FalseContent),
        typeof(object),
        typeof(BoolStateContentControl),
        new PropertyMetadata(null, OnStateChanged));

    public bool Value
    {
        get => (bool)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public object? TrueContent
    {
        get => GetValue(TrueContentProperty);
        set => SetValue(TrueContentProperty, value);
    }

    public object? FalseContent
    {
        get => GetValue(FalseContentProperty);
        set => SetValue(FalseContentProperty, value);
    }

    public BoolStateContentControl()
    {
        UpdateContent();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BoolStateContentControl ctrl)
        {
            ctrl.UpdateContent();
        }
    }

    private void UpdateContent()
    {
        SetCurrentValue(ContentProperty, Value ? TrueContent : FalseContent);
    }
}
