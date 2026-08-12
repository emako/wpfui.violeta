using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A card-styled slide used by <see cref="CardCarousel"/>.
/// Visual defaults mirror WPF UI <c>Card</c> (background / border / corner radius).
/// </summary>
public class CardCarouselItem : ContentControl
{
    static CardCarouselItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CardCarouselItem),
            new FrameworkPropertyMetadata(typeof(CardCarouselItem)));
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(CardCarouselItem),
            new PropertyMetadata(false));

    /// <summary>Whether this card is currently in the center (focused) slot.</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
