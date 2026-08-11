using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public sealed class SelectTemplateEventArgs : EventArgs
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal SelectTemplateEventArgs()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public string TemplateKey { get; set; }

    public object DataContext { get; internal set; }

    public UIElement Owner { get; internal set; }
}
