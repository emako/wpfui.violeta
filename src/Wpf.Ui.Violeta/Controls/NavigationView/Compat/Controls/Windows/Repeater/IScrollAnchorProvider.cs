using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public interface IScrollAnchorProvider
{
    public void RegisterAnchorCandidate(UIElement element);

    public void UnregisterAnchorCandidate(UIElement element);

    public UIElement CurrentAnchor { get; }
}
