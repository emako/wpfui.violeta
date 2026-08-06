using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A pagination indicator button for <see cref="CarouselNav"/>.
/// Mirrors Fluent UI React <c>CarouselNavButton</c> (<c>role="tab"</c>).
/// </summary>
public class CarouselNavButton : Button
{
    static CarouselNavButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CarouselNavButton),
            new FrameworkPropertyMetadata(typeof(CarouselNavButton)));
    }

    public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(
            nameof(Index),
            typeof(int),
            typeof(CarouselNavButton),
            new PropertyMetadata(-1));

    public static readonly DependencyProperty AppearanceProperty =
        DependencyProperty.Register(
            nameof(Appearance),
            typeof(CarouselNavAppearance),
            typeof(CarouselNavButton),
            new PropertyMetadata(CarouselNavAppearance.Default));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(CarouselNavButton),
            new PropertyMetadata(false));

    /// <summary>Zero-based page index this button represents.</summary>
    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    /// <summary>Visual appearance inherited from <see cref="CarouselNav"/>.</summary>
    public CarouselNavAppearance Appearance
    {
        get => (CarouselNavAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    /// <summary>Whether this button represents the active carousel page.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
}
