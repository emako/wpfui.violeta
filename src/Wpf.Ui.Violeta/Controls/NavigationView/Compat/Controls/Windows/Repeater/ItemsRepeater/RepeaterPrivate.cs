using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal delegate void ConfigurationChangedEventHandler(IRepeaterScrollingSurface sender);

internal delegate void PostArrangeEventHandler(IRepeaterScrollingSurface sender);

internal delegate void ViewportChangedEventHandler(IRepeaterScrollingSurface sender, bool isFinal);

internal interface IRepeaterScrollingSurface
{
    public bool IsHorizontallyScrollable { get; }
    public bool IsVerticallyScrollable { get; }
    public UIElement AnchorElement { get; }

    public event ConfigurationChangedEventHandler ConfigurationChanged;

    public event PostArrangeEventHandler PostArrange;

    public event ViewportChangedEventHandler ViewportChanged;

    public void RegisterAnchorCandidate(UIElement element);

    public void UnregisterAnchorCandidate(UIElement element);

    public Rect GetRelativeViewport(UIElement child);
}
