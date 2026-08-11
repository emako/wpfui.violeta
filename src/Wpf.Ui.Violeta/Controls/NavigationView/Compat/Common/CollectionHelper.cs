using System.Collections.Generic;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class CollectionHelper
{
    public static bool Contains<T>(IList<T> c, T v)
    {
        return c.Contains(v);
    }

    public static void UniquePushBack<T>(IList<T> c, T v)
    {
        if (!c.Contains(v))
        {
            c.Add(v);
        }
    }
}
