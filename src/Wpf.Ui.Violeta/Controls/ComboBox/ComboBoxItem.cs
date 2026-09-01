using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// ComboBox item with optional <see cref="InputGestureText"/> and <see cref="Command"/> support.
/// </summary>
public class ComboBoxItem : System.Windows.Controls.ComboBoxItem
{
    static ComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ComboBoxItem),
            new FrameworkPropertyMetadata(typeof(ComboBoxItem)));
    }

    /// <summary>
    /// Shortcut text shown on the trailing edge of the item (same role as <c>MenuItem.InputGestureText</c>).
    /// Registered as attached so default ComboBox / ToggleComboBox item templates can bind it.
    /// </summary>
    public static readonly DependencyProperty InputGestureTextProperty =
        DependencyProperty.RegisterAttached(
            "InputGestureText",
            typeof(string),
            typeof(ComboBoxItem),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public static string GetInputGestureText(DependencyObject element)
    {
        return (string)element.GetValue(InputGestureTextProperty);
    }

    public static void SetInputGestureText(DependencyObject element, string value)
    {
        element.SetValue(InputGestureTextProperty, value);
    }

    [Bindable(true)]
    [Category("Content")]
    public string InputGestureText
    {
        get => (string)GetValue(InputGestureTextProperty);
        set => SetValue(InputGestureTextProperty, value);
    }

    /// <summary>Identifies the <see cref="Command"/> dependency property.</summary>
    public static readonly DependencyProperty CommandProperty = ButtonBase.CommandProperty.AddOwner(
        typeof(ComboBoxItem));

    /// <summary>
    /// Gets or sets the command invoked when this item becomes selected.
    /// </summary>
    [Bindable(true)]
    [Category("Action")]
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Identifies the <see cref="CommandParameter"/> dependency property.</summary>
    public static readonly DependencyProperty CommandParameterProperty =
        ButtonBase.CommandParameterProperty.AddOwner(typeof(ComboBoxItem));

    /// <summary>
    /// Gets or sets the parameter passed to <see cref="Command"/> when it is invoked.
    /// </summary>
    [Bindable(true)]
    [Category("Action")]
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);
        TryExecuteCommand();
    }

    private void TryExecuteCommand()
    {
        var parameter = CommandParameter;
        var command = Command;
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }
}
