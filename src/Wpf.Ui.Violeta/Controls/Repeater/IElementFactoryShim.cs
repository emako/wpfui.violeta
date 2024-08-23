using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public interface IElementFactoryShim
{
    UIElement GetElement(ElementFactoryGetArgs args);
    void RecycleElement(ElementFactoryRecycleArgs context);
}