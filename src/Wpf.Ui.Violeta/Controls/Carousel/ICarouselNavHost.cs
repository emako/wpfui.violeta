using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Host that can be driven by <see cref="CarouselNav"/> (used by <see cref="Carousel"/> and <see cref="CardCarousel"/>).
/// </summary>
public interface ICarouselNavHost
{
    /// <summary>Total number of slides / cards.</summary>
    int TotalSlides { get; }

    /// <summary>Zero-based index of the active slide / card.</summary>
    int ActiveIndex { get; }

    /// <summary>Item container generator for slide count change notifications.</summary>
    ItemContainerGenerator ItemContainerGenerator { get; }

    /// <summary>Raised when <see cref="ActiveIndex"/> changes.</summary>
    event RoutedPropertyChangedEventHandler<int> ActiveIndexChanged;

    /// <summary>Selects the slide / card at the given index.</summary>
    void SelectPageByIndex(int index);

    /// <summary>Resets the autoplay countdown after user navigation.</summary>
    void ResetAutoplay();
}
