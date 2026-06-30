#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System;

namespace Wpf.Ui.Violeta.Controls.Compat;

[Flags]
public enum AnimationContext
{
    None = 0,
    CollectionChangeAdd = 1,
    CollectionChangeRemove = 2,
    CollectionChangeReset = 4,
    LayoutTransition = 8,
}

[Flags]
public enum ElementRealizationOptions
{
    None = 0,
    ForceCreate = 1,
    SuppressAutoRecycle = 2,
}
