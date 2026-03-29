using System.Windows;

namespace iNKORE.UI.WPF.Modern.Controls
{
    public sealed class ElementFactoryGetArgs
    {
        public ElementFactoryGetArgs()
        {
        }

        public UIElement Parent { get; set; }
        public object Data { get; set; }
        internal int Index { get; set; }
    }
}