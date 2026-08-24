using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A toggle button that switches icons for unchecked / checked / indeterminate states.
/// Default chrome shows hover and selected borders; use
/// <c>SampleIconToggleButtonStyle</c> for a flat icon-only look.
/// </summary>
/// <remarks>
/// When <see cref="ToggleButton.IsThreeState"/> is <c>true</c>,
/// <see cref="IndeterminateIcon"/> is shown for <c>IsChecked == null</c>.
/// </remarks>
/// <example>
/// <code lang="xml">
/// &lt;vio:IconToggleButton
///     CheckedIcon="{ui:SymbolIcon EyeOff24}"
///     UncheckedIcon="{ui:SymbolIcon Eye24}"
///     ToolTip="Toggle visibility" /&gt;
/// </code>
/// </example>
public class IconToggleButton : ToggleButton
{
    /// <summary>Identifies the <see cref="IsCheckedChanged"/> routed event.</summary>
    public static readonly RoutedEvent IsCheckedChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(IsCheckedChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(IconToggleButton));

    /// <summary>Occurs when <see cref="ToggleButton.IsChecked"/> changes.</summary>
    public event RoutedEventHandler IsCheckedChanged
    {
        add => AddHandler(IsCheckedChangedEvent, value);
        remove => RemoveHandler(IsCheckedChangedEvent, value);
    }

    /// <summary>Identifies the <see cref="CheckedIcon"/> dependency property.</summary>
    public static readonly DependencyProperty CheckedIconProperty =
        DependencyProperty.Register(
            nameof(CheckedIcon),
            typeof(IconElement),
            typeof(IconToggleButton),
            new PropertyMetadata(null));

    /// <summary>Icon shown when <see cref="ToggleButton.IsChecked"/> is <c>true</c>.</summary>
    public IconElement? CheckedIcon
    {
        get => (IconElement?)GetValue(CheckedIconProperty);
        set => SetValue(CheckedIconProperty, value);
    }

    /// <summary>Identifies the <see cref="UncheckedIcon"/> dependency property.</summary>
    public static readonly DependencyProperty UncheckedIconProperty =
        DependencyProperty.Register(
            nameof(UncheckedIcon),
            typeof(IconElement),
            typeof(IconToggleButton),
            new PropertyMetadata(null));

    /// <summary>Icon shown when <see cref="ToggleButton.IsChecked"/> is <c>false</c>.</summary>
    public IconElement? UncheckedIcon
    {
        get => (IconElement?)GetValue(UncheckedIconProperty);
        set => SetValue(UncheckedIconProperty, value);
    }

    /// <summary>Identifies the <see cref="IndeterminateIcon"/> dependency property.</summary>
    public static readonly DependencyProperty IndeterminateIconProperty =
        DependencyProperty.Register(
            nameof(IndeterminateIcon),
            typeof(IconElement),
            typeof(IconToggleButton),
            new PropertyMetadata(null));

    /// <summary>
    /// Icon shown when <see cref="ToggleButton.IsChecked"/> is <c>null</c>
    /// (requires <see cref="ToggleButton.IsThreeState"/>).
    /// Falls back to <see cref="UncheckedIcon"/> when unset.
    /// </summary>
    public IconElement? IndeterminateIcon
    {
        get => (IconElement?)GetValue(IndeterminateIconProperty);
        set => SetValue(IndeterminateIconProperty, value);
    }

    static IconToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(IconToggleButton),
            new FrameworkPropertyMetadata(typeof(IconToggleButton)));

        BackgroundProperty.OverrideMetadata(
            typeof(IconToggleButton),
            new FrameworkPropertyMetadata(Brushes.Transparent));
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsCheckedProperty)
        {
            RaiseEvent(new RoutedEventArgs(IsCheckedChangedEvent, this));
        }
    }
}
