using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

internal abstract class ViewportManager
{
    public abstract UIElement SuggestedAnchor { get; }

    public abstract double HorizontalCacheLength { get; set; }

    public abstract double VerticalCacheLength { get; set; }

    public abstract Rect GetLayoutVisibleWindow();
    public abstract Rect GetLayoutRealizationWindow();

    public abstract void SetLayoutExtent(Rect extent);
    public abstract Point GetOrigin();

    public abstract void OnLayoutChanged(bool isVirtualizing);
    public abstract void OnElementPrepared(UIElement element);
    public abstract void OnElementCleared(UIElement element);
    public abstract void OnOwnerMeasuring();
    public abstract void OnOwnerArranged();
    public abstract void OnMakeAnchor(UIElement anchor, bool isAnchorOutsideRealizedRange);

    public abstract void ResetScrollers();

    public abstract UIElement MadeAnchor { get; }
}