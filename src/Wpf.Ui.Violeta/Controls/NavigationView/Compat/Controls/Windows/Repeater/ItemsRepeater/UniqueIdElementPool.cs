using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class UniqueIdElementPool(ItemsRepeater owner) : IEnumerable<KeyValuePair<string, UIElement>>
{
    public void Add(UIElement element)
    {
        Debug.Assert(owner.ItemsSourceView.HasKeyIndexMapping);

        var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
        var key = virtInfo.UniqueId;

        if (m_elementMap.ContainsKey(key))
        {
            string message = "The unique id provided (" + virtInfo.UniqueId + ") is not unique.";
            throw new Exception(message);
        }

        m_elementMap.Add(key, element);
    }

    public UIElement Remove(int index)
    {
        Debug.Assert(owner.ItemsSourceView.HasKeyIndexMapping);

        // Check if there is already a element in the mapping and if so, use it.
        string key = owner.ItemsSourceView.KeyFromIndex(index);
        if (m_elementMap.TryGetValue(key, out UIElement? element))
        {
            m_elementMap.Remove(key);
        }

        return element!;
    }

    public void Clear()
    {
        Debug.Assert(owner.ItemsSourceView.HasKeyIndexMapping);
        m_elementMap.Clear();
    }

    public IEnumerator<KeyValuePair<string, UIElement>> GetEnumerator()
    {
        return m_elementMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

#if DEBUG
    public bool IsEmpty => m_elementMap.Count == 0;

#endif
    private readonly Dictionary<string, UIElement> m_elementMap = [];
}
