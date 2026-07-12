namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Defines the behavior of <see cref="TagInput"/> when its inner text box loses focus.
/// </summary>
public enum LostFocusBehavior
{
    /// <summary>Do nothing with the pending text.</summary>
    None,

    /// <summary>Commit the pending text as a new tag.</summary>
    Add,

    /// <summary>Discard the pending text.</summary>
    Clear,
}
