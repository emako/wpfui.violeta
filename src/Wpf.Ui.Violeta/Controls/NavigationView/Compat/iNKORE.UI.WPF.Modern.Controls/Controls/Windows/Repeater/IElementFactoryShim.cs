using System.Windows;

namespace iNKORE.UI.WPF.Modern.Controls
{
    public interface IElementFactoryShim
    {
        UIElement GetElement(ElementFactoryGetArgs args);
        void RecycleElement(ElementFactoryRecycleArgs context);
    }
}