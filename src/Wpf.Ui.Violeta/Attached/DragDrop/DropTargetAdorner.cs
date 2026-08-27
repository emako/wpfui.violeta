using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Attached.DragDrop;

/// <summary>
/// Base class for drop target Adorner.
/// </summary>
public abstract class DropTargetAdorner : Adorner
{
    private readonly AdornerLayer adornerLayer;

    /// <summary>
    /// Gets or Sets the pen which can be used for the render process.
    /// </summary>
    public Pen Pen { get; set; } = new Pen(Brushes.Gray, 2);

    public IDropInfo DropInfo { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public DropTargetAdorner(UIElement adornedElement, IDropInfo dropInfo)
        : base(adornedElement)
    {
        this.DropInfo = dropInfo;
        this.IsHitTestVisible = false;
        this.AllowDrop = false;
        this.SnapsToDevicePixels = true;
        this.adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
        // can be null but should normally not be null
        this.adornerLayer?.Add(this);
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Detach the adorner from its adorner layer.
    /// </summary>
    public void Detach()
    {
        if (this.adornerLayer is null)
        {
            return;
        }

        if (!this.adornerLayer.Dispatcher.CheckAccess())
        {
            this.adornerLayer.Dispatcher.Invoke(this.Detach);
            return;
        }

        this.adornerLayer.Remove(this);
    }

    internal static DropTargetAdorner Create(Type type, UIElement adornedElement, IDropInfo dropInfo)
    {
        if (!typeof(DropTargetAdorner).IsAssignableFrom(type))
        {
            throw new InvalidOperationException("The requested adorner class does not derive from DropTargetAdorner.");
        }

        var ctor = type.GetConstructor([typeof(UIElement), typeof(IDropInfo)]);
        if (ctor is null && dropInfo is DropInfo)
        {
            ctor = type.GetConstructor([typeof(UIElement), typeof(DropInfo)]);
        }

        return (ctor?.Invoke([adornedElement, dropInfo]) as DropTargetAdorner)!;
    }
}
