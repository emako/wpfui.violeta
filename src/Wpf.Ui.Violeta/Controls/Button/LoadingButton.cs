using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Loading indicator placement relative to button content.
/// </summary>
public enum LoadingPlacement
{
    Left,
    Right,
    Top,
    Bottom,
}

/// <summary>
/// A <see cref="Wpf.Ui.Controls.Button"/> that can display a spinning loading indicator
/// in one of four positions (Left / Right / Top / Bottom) around its content.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;vio:LoadingButton
///     IsLoading="{Binding IsBusy}"
///     LoadingPlacement="Left"
///     Content="Submit" /&gt;
/// </code>
/// </example>
public class LoadingButton : Wpf.Ui.Controls.Button
{
    /// <summary>Identifies the <see cref="IsLoading"/> dependency property.</summary>
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingButton),
            new PropertyMetadata(false));

    /// <summary>Gets or sets a value indicating whether the loading animation is visible.</summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>Identifies the <see cref="LoadingPlacement"/> dependency property.</summary>
    public static readonly DependencyProperty LoadingPlacementProperty =
        DependencyProperty.Register(
            nameof(LoadingPlacement),
            typeof(LoadingPlacement),
            typeof(LoadingButton),
            new PropertyMetadata(LoadingPlacement.Left));

    /// <summary>Gets or sets the position of the loading indicator relative to the button content.</summary>
    public LoadingPlacement LoadingPlacement
    {
        get => (LoadingPlacement)GetValue(LoadingPlacementProperty);
        set => SetValue(LoadingPlacementProperty, value);
    }

    static LoadingButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LoadingButton),
            new FrameworkPropertyMetadata(typeof(LoadingButton)));
    }
}
