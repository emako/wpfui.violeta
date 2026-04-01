using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public interface IElementFactory
{
    public UIElement GetElement(ElementFactoryGetArgs args);

    public void RecycleElement(ElementFactoryRecycleArgs args);
}
