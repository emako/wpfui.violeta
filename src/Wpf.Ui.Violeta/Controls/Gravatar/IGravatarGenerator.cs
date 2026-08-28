namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Generates a visual representation (typically an identicon) for a Gravatar <c>Id</c>.
/// </summary>
public interface IGravatarGenerator
{
    /// <summary>
    /// Creates the content to display for the given identifier.
    /// </summary>
    /// <param name="id">The identifier used to seed the avatar (e.g. user name or email).</param>
    /// <returns>A visual element (usually a <see cref="System.Windows.Shapes.Path"/>).</returns>
    public object GetGravatar(string id);
}
