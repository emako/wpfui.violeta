using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class RepeaterUIElementCollection(UIElement visualParent, FrameworkElement logicalParent)
    : UIElementCollection(visualParent, logicalParent)
{
    public override int Count => _visualChildren.Count;

    public override bool IsSynchronized => _visualChildren.IsSynchronized;

    public override object SyncRoot => _visualChildren.SyncRoot;

    public override void CopyTo(Array array, int index)
    {
        _visualChildren.CopyTo(array, index);
    }

    public override void CopyTo(UIElement[] array, int index)
    {
        _visualChildren.CopyTo(array, index);
    }

    public override int Capacity
    {
        get => _visualChildren.Capacity;
        set => _visualChildren.Capacity = value;
    }

    public override UIElement this[int index]
    {
        get => (_visualChildren[index] as UIElement)!;
        set
        {
            ValidateElement(value);

            VisualCollection vc = _visualChildren;

            //if setting new element into slot or assigning null,
            //remove previously hooked element from the logical tree
            if (vc[index] != value)
            {
                if (vc[index] is UIElement e)
                    ClearLogicalParent(e);

                vc[index] = value;

                SetLogicalParent(value);
            }
        }
    }

    public override int Add(UIElement element)
    {
        return AddInternal(element);
    }

    // Warning: this method is very dangerous because it does not prevent adding children
    // into collection populated by generator. This may cause crashes if used incorrectly.
    // Don't call this unless you are deriving a panel that is populating the collection
    // in cooperation with the generator
    internal int AddInternal(UIElement element)
    {
        ValidateElement(element);

        SetLogicalParent(element);
        int retVal = _visualChildren.Add(element);

        return retVal;
    }

    public override int IndexOf(UIElement element)
    {
        return _visualChildren.IndexOf(element);
    }

    public override void Remove(UIElement element)
    {
        RemoveInternal(element);
    }

    internal void RemoveInternal(UIElement element)
    {
        _visualChildren.Remove(element);
        ClearLogicalParent(element);
    }

    public override bool Contains(UIElement element)
    {
        return _visualChildren.Contains(element);
    }

    public override void Clear()
    {
        ClearInternal();
    }

    // Warning: this method is very dangerous because it does not prevent adding children
    // into collection populated by generator. This may cause crashes if used incorrectly.
    // Don't call this unless you are deriving a panel that is populating the collection
    // in cooperation with the generator
    internal void ClearInternal()
    {
        VisualCollection vc = _visualChildren;
        int cnt = vc.Count;

        if (cnt > 0)
        {
            // copy children in VisualCollection so that we can clear the visual link first,
            // followed by the logical link
            Visual[] visuals = new Visual[cnt];
            for (int i = 0; i < cnt; i++)
            {
                visuals[i] = vc[i];
            }

            vc.Clear();

            //disconnect from logical tree
            for (int i = 0; i < cnt; i++)
            {
                if (visuals[i] is UIElement e)
                {
                    ClearLogicalParent(e);
                }
            }
        }
    }

    public override void Insert(int index, UIElement element)
    {
        InsertInternal(index, element);
    }

    // Warning: this method is very dangerous because it does not prevent adding children
    // into collection populated by generator. This may cause crashes if used incorrectly.
    // Don't call this unless you are deriving a panel that is populating the collection
    // in cooperation with the generator
    internal void InsertInternal(int index, UIElement element)
    {
        ValidateElement(element);

        SetLogicalParent(element);
        _visualChildren.Insert(index, element);
    }

    public override void RemoveAt(int index)
    {
        VisualCollection vc = _visualChildren;

        //disconnect from logical tree
        UIElement e = (vc[index] as UIElement)!;

        vc.RemoveAt(index);

        if (e != null)
            ClearLogicalParent(e);
    }

    public override void RemoveRange(int index, int count)
    {
        RemoveRangeInternal(index, count);
    }

    // Warning: this method is very dangerous because it does not prevent adding children
    // into collection populated by generator. This may cause crashes if used incorrectly.
    // Don't call this unless you are deriving a panel that is populating the collection
    // in cooperation with the generator
    internal void RemoveRangeInternal(int index, int count)
    {
        VisualCollection vc = _visualChildren;
        int cnt = vc.Count;
        if (count > (cnt - index))
        {
            count = cnt - index;
        }

        if (count > 0)
        {
            // copy children in VisualCollection so that we can clear the visual link first,
            // followed by the logical link
            Visual[] visuals = new Visual[count];
            int i = index;
            for (int loop = 0; loop < count; i++, loop++)
            {
                visuals[loop] = vc[i];
            }

            vc.RemoveRange(index, count);

            //disconnect from logical tree
            for (i = 0; i < count; i++)
            {
                if (visuals[i] is UIElement e)
                {
                    ClearLogicalParent(e);
                }
            }
        }
    }

    // Helper function to validate element; will throw exceptions if problems are detected.
    private void ValidateElement(UIElement element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(string.Format("Children of '{0}' cannot be null. Object derived from UIElement expected.", this.GetType()));
        }
    }

    public override IEnumerator GetEnumerator()
    {
        return _visualChildren.GetEnumerator();
    }

    private readonly VisualCollection _visualChildren = new(visualParent);
}
