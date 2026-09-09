using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Adorner that hosts a single <see cref="Visual"/> child (e.g. a <see cref="DrawingVisual"/>).
/// </summary>
public class VisualAdornerContainer(UIElement adornedElement) : Adorner(adornedElement)
{
    private Visual? _child = adornedElement;

    public Visual? Child
    {
        get => _child;
        set
        {
            if (value is null)
            {
                if (_child is not null)
                {
                    RemoveVisualChild(_child);
                }

                _child = value;
                return;
            }

            AddVisualChild(value);
            _child = value;
        }
    }

    protected override int VisualChildrenCount => _child is not null ? 1 : 0;

    protected override Visual GetVisualChild(int index)
    {
        if (index == 0 && _child is not null)
        {
            return _child;
        }

        return base.GetVisualChild(index);
    }
}
