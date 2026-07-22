using System.ComponentModel;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Provides data for the <see cref="TaskDialog.ButtonClicked"/> event.
/// </summary>
/// <threadsafety instance="false" static="true" />
/// <remarks>
/// Initializes a new instance of the <see cref="TaskDialogItemClickedEventArgs"/> class with the specified item.
/// </remarks>
/// <param name="item">The <see cref="TaskDialogItem"/> that was clicked.</param>
public class TaskDialogItemClickedEventArgs(TaskDialogItem item) : CancelEventArgs
{
    /// <summary>
    /// Gets the item that was clicked.
    /// </summary>
    /// <value>
    /// The <see cref="TaskDialogItem"/> that was clicked.
    /// </value>
    public TaskDialogItem Item => item;
}
