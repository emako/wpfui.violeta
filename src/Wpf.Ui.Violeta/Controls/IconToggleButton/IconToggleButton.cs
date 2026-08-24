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
}
