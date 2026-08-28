using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Adorner host for the auto-created growl panel. Empty regions are not hit-testable
/// because the root grid has a null background.
/// </summary>
internal sealed class GrowlAdorner : Adorner
{
    private readonly UIElement _child;

    public GrowlAdorner(UIElement adornedElement, UIElement child)
        : base(adornedElement)
    {
        _child = child;
        AddVisualChild(child);
    }

    public UIElement Child => _child;

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) => _child;

    protected override Size MeasureOverride(Size constraint)
    {
        _child.Measure(constraint);
        return constraint;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _child.Arrange(new Rect(finalSize));
        return finalSize;
    }
}
