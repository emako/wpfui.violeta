using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat
{
    public interface IScrollAnchorProvider
    {
        void RegisterAnchorCandidate(UIElement element);
        void UnregisterAnchorCandidate(UIElement element);

        UIElement CurrentAnchor { get; }
    }
}

