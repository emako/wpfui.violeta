using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

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