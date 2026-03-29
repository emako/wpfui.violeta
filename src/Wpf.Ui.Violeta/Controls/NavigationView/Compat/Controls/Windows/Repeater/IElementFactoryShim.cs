using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat
{
    public interface IElementFactoryShim
    {
        UIElement GetElement(ElementFactoryGetArgs args);
        void RecycleElement(ElementFactoryRecycleArgs context);
    }
}
