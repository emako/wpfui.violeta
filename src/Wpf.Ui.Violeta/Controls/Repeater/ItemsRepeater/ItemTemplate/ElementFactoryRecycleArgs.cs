using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public sealed class ElementFactoryRecycleArgs
{
    public ElementFactoryRecycleArgs()
    {
    }

    public UIElement Parent { get; set; }
    public UIElement Element { get; set; }
}