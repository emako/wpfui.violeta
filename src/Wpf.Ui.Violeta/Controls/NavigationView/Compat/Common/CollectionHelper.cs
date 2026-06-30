#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

using System.Collections.Generic;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal static class CollectionHelper
{
    public static bool contains<T>(IList<T> c, T v)
    {
        return c.Contains(v);
    }

    public static void unique_push_back<T>(IList<T> c, T v)
    {
        if (!c.Contains(v))
        {
            c.Add(v);
        }
    }
}
