using System.Windows;

namespace Wpf.Ui.Markdown.Plugins;

/// <summary>
/// Overwrites WPF Style used for markdown document rendering.
/// </summary>
public interface IStyleOverwriter
{
    /// <summary>
    /// Overwrites the given style in place.
    /// </summary>
    /// <param name="style">The document style to overwrite.</param>
    /// <returns>The overwritten style.</returns>
    Style Overwrite(Style style);
}
