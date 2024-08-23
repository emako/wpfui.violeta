using System;

namespace Wpf.Ui.Violeta.Controls;

[Flags]
public enum AnimationContext
{
    None = 0,
    CollectionChangeAdd = 1,
    CollectionChangeRemove = 2,
    CollectionChangeReset = 4,
    LayoutTransition = 8
}

[Flags]
public enum ElementRealizationOptions
{
    None = 0,
    ForceCreate = 1,
    SuppressAutoRecycle = 2
}
