#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public class ElementFactory : DependencyObject, IElementFactoryShim
{
    public ElementFactory()
    {
    }

    #region IElementFactory

    public UIElement GetElement(ElementFactoryGetArgs args)
    {
        return GetElementCore(args);
    }

    public void RecycleElement(ElementFactoryRecycleArgs args)
    {
        RecycleElementCore(args);
    }

    #endregion IElementFactory

    protected virtual UIElement GetElementCore(ElementFactoryGetArgs args)
    {
        throw new NotImplementedException();
    }

    protected virtual void RecycleElementCore(ElementFactoryRecycleArgs args)
    {
        throw new NotImplementedException();
    }
}
