using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public interface IScrollAnchorProvider
{
    void RegisterAnchorCandidate(UIElement element);
    void UnregisterAnchorCandidate(UIElement element);

    UIElement CurrentAnchor { get; }
}
