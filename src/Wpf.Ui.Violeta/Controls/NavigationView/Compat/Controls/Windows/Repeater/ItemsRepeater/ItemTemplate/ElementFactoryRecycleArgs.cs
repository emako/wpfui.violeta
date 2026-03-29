using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat
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
