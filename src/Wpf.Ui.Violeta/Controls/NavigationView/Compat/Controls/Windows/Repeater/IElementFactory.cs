using System.Windows;

namespace iNKORE.UI.WPF.Modern.Controls
{
    public interface IElementFactory
    {
        UIElement GetElement(ElementFactoryGetArgs args);
        void RecycleElement(ElementFactoryRecycleArgs args);
    }
}