#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public interface IScrollAnchorProvider
{
    public void RegisterAnchorCandidate(UIElement element);

    public void UnregisterAnchorCandidate(UIElement element);

    public UIElement CurrentAnchor { get; }
}
