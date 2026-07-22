using System;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Class that provides data for the <see cref="TaskDialog.HyperlinkClicked"/> event.
/// </summary>
/// <threadsafety instance="false" static="true" />
/// <remarks>
/// Creates a new instance of the <see cref="HyperlinkClickedEventArgs"/> class with the specified URL.
/// </remarks>
/// <param name="href">The URL of the hyperlink.</param>
public class HyperlinkClickedEventArgs(string href) : EventArgs
{
    /// <summary>
    /// Gets the URL of the hyperlink that was clicked.
    /// </summary>
    /// <value>
    /// The value of the href attribute of the hyperlink.
    /// </value>
    public string Href => href;
}
