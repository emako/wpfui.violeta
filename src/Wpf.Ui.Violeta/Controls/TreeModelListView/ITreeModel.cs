using System.Collections;

namespace Wpf.Ui.Controls;

public interface ITreeModel
{
    /// <summary>
    /// Get list of children of the specified parent
    /// </summary>
    public IEnumerable? GetChildren(object parent);

    /// <summary>
    /// returns wheather specified parent has any children or not.
    /// </summary>
    public bool HasChildren(object parent);
}
