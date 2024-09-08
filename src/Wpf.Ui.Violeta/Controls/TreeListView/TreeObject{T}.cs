using System.Collections.ObjectModel;

namespace Wpf.Ui.Violeta.Controls;

public class TreeObject<T> where T : new()
{
    public virtual ObservableCollection<T> Children { get; set; } = [];
}
