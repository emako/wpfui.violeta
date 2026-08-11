using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal interface IFlowLayoutAlgorithmDelegates
{
    public Size Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context);

    public Size Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context);

    public bool Algorithm_ShouldBreakLine(int index, double remainingSpace);

    public FlowLayoutAnchorInfo Algorithm_GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context);

    public FlowLayoutAnchorInfo Algorithm_GetAnchorForTargetElement(int targetIndex, Size availableSize, VirtualizingLayoutContext context);

    public Rect Algorithm_GetExtent(Size availableSize,
        VirtualizingLayoutContext context,
        UIElement firstRealized,
        int firstRealizedItemIndex,
        Rect firstRealizedLayoutBounds,
        UIElement lastRealized,
        int lastRealizedItemIndex,
        Rect lastRealizedLayoutBounds);

    public void Algorithm_OnElementMeasured(
        UIElement element,
        int index,
        Size availableSize,
        Size measureSize,
        Size desiredSize,
        Size provisionalArrangeSize,
        VirtualizingLayoutContext context);

    public void Algorithm_OnLineArranged(
        int startIndex,
        int countInLine,
        double lineSize,
        VirtualizingLayoutContext context);
}
