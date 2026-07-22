using System;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Provides data for the <see cref="TaskDialog.ExpandButtonClicked"/> event.
/// </summary>
/// <threadsafety instance="false" static="true" />
/// <remarks>
/// Initializes a new instance of the <see cref="ExpandButtonClickedEventArgs"/> class with the specified expanded state.
/// </remarks>
/// <param name="expanded"><see langword="true" /> if the the expanded content on the dialog is shown; otherwise, <see langword="false" />.</param>
public class ExpandButtonClickedEventArgs(bool expanded) : EventArgs
{
    /// <summary>
    /// Gets a value that indicates if the expanded content on the dialog is shown.
    /// </summary>
    /// <value><see langword="true" /> if the expanded content on the dialog is shown; otherwise, <see langword="false" />.</value>
    public bool Expanded => expanded;
}
