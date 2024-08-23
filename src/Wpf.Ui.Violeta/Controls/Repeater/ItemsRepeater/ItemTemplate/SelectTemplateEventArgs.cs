using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public sealed class SelectTemplateEventArgs : EventArgs
{
    internal SelectTemplateEventArgs()
    {
    }

    public string TemplateKey { get; set; }

    public object DataContext { get; internal set; }

    public UIElement Owner { get; internal set; }
}
