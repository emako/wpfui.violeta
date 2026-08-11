using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

public sealed class ElementFactoryGetArgs
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ElementFactoryGetArgs()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public UIElement Parent { get; set; }
    public object Data { get; set; }
    internal int Index { get; set; }
}
