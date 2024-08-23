using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public interface IElementFactory
{
    UIElement GetElement(ElementFactoryGetArgs args);
    void RecycleElement(ElementFactoryRecycleArgs args);
}