using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

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

    #endregion

    protected virtual UIElement GetElementCore(ElementFactoryGetArgs args)
    {
        throw new NotImplementedException();
    }

    protected virtual void RecycleElementCore(ElementFactoryRecycleArgs args)
    {
        throw new NotImplementedException();
    }
}
