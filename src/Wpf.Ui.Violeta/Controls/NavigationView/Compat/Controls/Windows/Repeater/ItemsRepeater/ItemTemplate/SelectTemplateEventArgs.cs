#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public sealed class SelectTemplateEventArgs : EventArgs
{
    internal SelectTemplateEventArgs()
    {
    }

    public string TemplateKey { get; set; }

    public object DataContext { get; internal set; }

    public UIElement Owner { get; internal set; }
}
