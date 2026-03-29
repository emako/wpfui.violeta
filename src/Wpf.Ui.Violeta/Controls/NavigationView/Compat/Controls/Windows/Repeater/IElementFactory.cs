using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat
{
    public interface IElementFactory
    {
        UIElement GetElement(ElementFactoryGetArgs args);
        void RecycleElement(ElementFactoryRecycleArgs args);
    }
}
