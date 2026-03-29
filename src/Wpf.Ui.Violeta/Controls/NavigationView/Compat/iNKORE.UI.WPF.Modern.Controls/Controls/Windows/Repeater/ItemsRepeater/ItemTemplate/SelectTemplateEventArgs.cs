using System;
using System.Windows;

namespace iNKORE.UI.WPF.Modern.Controls
{
    public sealed class SelectTemplateEventArgs : EventArgs
    {
        internal SelectTemplateEventArgs()
        {
        }

        public string TemplateKey { get; set; }

        public object DataContext { get; internal set; }

        public UIElement Owner { get; internal set; }
    }
}
