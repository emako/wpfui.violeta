using System;

namespace Wpf.Ui.Violeta.Attached.DragDrop;

/// <summary>
/// Implementation of the <see cref="IDropHintInfo"/> interface to hold DropHint information.
/// </summary>
public class DropHintInfo(IDragInfo dragInfo) : IDropHintInfo
{
    /// <inheritdoc />
    public IDragInfo DragInfo { get; } = dragInfo;

    /// <inheritdoc />
    public Type DropTargetHintAdorner { get; set; } = null!;

    /// <inheritdoc />
    public string DropHintText { get; set; } = null!;

    /// <inheritdoc />
    public DropHintState DropTargetHintState { get; set; }
}
