using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class BoolStateTextBlock : TextBlock
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(bool),
        typeof(BoolStateTextBlock),
        new PropertyMetadata(false, OnDisplayStateChanged));

    public static readonly DependencyProperty TrueTextProperty = DependencyProperty.Register(
        nameof(TrueText),
        typeof(string),
        typeof(BoolStateTextBlock),
        new PropertyMetadata("True", OnDisplayStateChanged));

    public static readonly DependencyProperty FalseTextProperty = DependencyProperty.Register(
        nameof(FalseText),
        typeof(string),
        typeof(BoolStateTextBlock),
        new PropertyMetadata("False", OnDisplayStateChanged));

    public bool Value
    {
        get => (bool)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string TrueText
    {
        get => (string)GetValue(TrueTextProperty);
        set => SetValue(TrueTextProperty, value);
    }

    public string FalseText
    {
        get => (string)GetValue(FalseTextProperty);
        set => SetValue(FalseTextProperty, value);
    }

    public BoolStateTextBlock()
    {
        UpdateDisplayText();
    }

    private static void OnDisplayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BoolStateTextBlock boolStateTextBlock)
        {
            boolStateTextBlock.UpdateDisplayText();
        }
    }

    private void UpdateDisplayText()
    {
        Text = Value ? TrueText : FalseText;
    }
}
