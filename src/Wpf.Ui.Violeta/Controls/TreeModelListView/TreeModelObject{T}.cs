﻿using System.Collections.ObjectModel;

namespace Wpf.Ui.Controls;

public class TreeModelObject<T> where T : new()
{
    public virtual ObservableCollection<T> Children { get; set; } = [];
}
