namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Specifies how a <see cref="ToolBar"/> child participates in overflow layout.
/// </summary>
public enum OverflowMode
{
    /// <summary>Move to the overflow area only when there is not enough space.</summary>
    AsNeeded,

    /// <summary>Always place in the overflow area.</summary>
    Always,

    /// <summary>Never place in the overflow area.</summary>
    Never,
}
