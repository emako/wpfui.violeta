using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents an individual card/slide in a <see cref="Carousel"/>.
/// Corresponds to Fluent UI React <c>CarouselCard</c>.
/// </summary>
public class CarouselItem : ContentControl
{
    static CarouselItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CarouselItem),
            new FrameworkPropertyMetadata(typeof(CarouselItem)));
    }

    /// <summary>
    /// Whether this item is the currently active page.
    /// </summary>
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(CarouselItem),
            new PropertyMetadata(false));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
