using System.Windows;

namespace iNKORE.UI.WPF.Modern.Controls
{
    public interface IScrollAnchorProvider
    {
        void RegisterAnchorCandidate(UIElement element);
        void UnregisterAnchorCandidate(UIElement element);

        UIElement CurrentAnchor { get; }
    }
}
