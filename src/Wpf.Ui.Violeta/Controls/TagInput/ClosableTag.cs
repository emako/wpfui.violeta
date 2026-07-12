using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A chip/tag element that displays its <see cref="ContentControl.Content"/> together with a small
/// close button. Used as the item container inside <see cref="TagInput"/>.
/// </summary>
[TemplatePart(Name = PART_CloseButton, Type = typeof(Button))]
public class ClosableTag : ContentControl
{
    public const string PART_CloseButton = "PART_CloseButton";

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ClosableTag),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(ClosableTag),
            new PropertyMetadata(null));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    static ClosableTag()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ClosableTag),
            new FrameworkPropertyMetadata(typeof(ClosableTag)));
    }
}
