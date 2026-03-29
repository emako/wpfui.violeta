using System.Windows;

namespace iNKORE.UI.WPF.Modern.Controls
{
    public sealed class ElementFactoryRecycleArgs
    {
        public ElementFactoryRecycleArgs()
        {
        }

        public UIElement Parent { get; set; }
        public UIElement Element { get; set; }
    }
}