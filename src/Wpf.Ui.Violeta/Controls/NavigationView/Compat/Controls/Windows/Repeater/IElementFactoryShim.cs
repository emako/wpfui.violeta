using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public interface IElementFactoryShim
{
    public UIElement GetElement(ElementFactoryGetArgs args);

    public void RecycleElement(ElementFactoryRecycleArgs context);
}
