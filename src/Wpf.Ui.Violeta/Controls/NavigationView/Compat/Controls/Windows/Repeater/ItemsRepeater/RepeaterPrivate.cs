#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal delegate void ConfigurationChangedEventHandler(IRepeaterScrollingSurface sender);

internal delegate void PostArrangeEventHandler(IRepeaterScrollingSurface sender);

internal delegate void ViewportChangedEventHandler(IRepeaterScrollingSurface sender, bool isFinal);

internal interface IRepeaterScrollingSurface
{
    bool IsHorizontallyScrollable { get; }
    bool IsVerticallyScrollable { get; }
    UIElement AnchorElement { get; }

    event ConfigurationChangedEventHandler ConfigurationChanged;

    event PostArrangeEventHandler PostArrange;

    event ViewportChangedEventHandler ViewportChanged;

    void RegisterAnchorCandidate(UIElement element);

    void UnregisterAnchorCandidate(UIElement element);

    Rect GetRelativeViewport(UIElement child);
}
