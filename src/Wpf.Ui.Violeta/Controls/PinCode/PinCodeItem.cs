using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A single cell in a <see cref="PinCode"/> control that displays one character.
/// </summary>
public class PinCodeItem : Control
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(PinCodeItem),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDisplayChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty PasswordCharProperty =
        DependencyProperty.Register(
            nameof(PasswordChar), typeof(char), typeof(PinCodeItem),
            new PropertyMetadata('\0', OnDisplayChanged));

    public char PasswordChar
    {
        get => (char)GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(PinCodeItem),
            new PropertyMetadata(new CornerRadius(0)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private static readonly DependencyPropertyKey DisplayTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(DisplayText), typeof(string), typeof(PinCodeItem),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

    /// <summary>
    /// The text actually shown: the <see cref="PasswordChar"/> (if set) when the cell is filled,
    /// otherwise the raw <see cref="Text"/>.
    /// </summary>
    public string DisplayText => (string)GetValue(DisplayTextProperty);

    static PinCodeItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PinCodeItem),
            new FrameworkPropertyMetadata(typeof(PinCodeItem)));

        FocusableProperty.OverrideMetadata(
            typeof(PinCodeItem),
            new FrameworkPropertyMetadata(true));
    }

    private static void OnDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PinCodeItem)d).UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
        var pc = PasswordChar;
        var text = Text ?? string.Empty;
        SetValue(DisplayTextPropertyKey,
            pc != '\0' && !string.IsNullOrEmpty(text) ? pc.ToString() : text);
    }

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();
        e.Handled = true;
    }
}
