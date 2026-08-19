//
//  Stfu — Sam’s Tiny Framework Utilities
//
//  Copyright © 2013–2021 Sam Hocevar <sam@hocevar.net>
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Stfu.Linq;

public static class Extensions
{
    /// <summary>
    /// Create a one-element enumerable from a single object
    /// </summary>
    public static IEnumerable<T> Yield<T>(this T element)
    {
        yield return element;
    }

    /// <summary>
    /// Call a function on each element of a sequence
    /// </summary>
    public static void ForAll<T>(this IEnumerable<T> elements, Action<T> fn)
    {
        foreach (var e in elements)
            fn(e);
    }

    /// <summary>
    /// Partition a sequence into chunks of fixed size
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> elements, int size)
        => new ChunkHelper<T>(elements, size);

    private sealed class ChunkHelper<T> : IEnumerable<IEnumerable<T>>
    {
        public ChunkHelper(IEnumerable<T> elements, int size)
        {
            m_elements = elements;
            m_size = size;
        }

        public IEnumerator<IEnumerable<T>> GetEnumerator()
        {
            using (var enumerator = m_elements.GetEnumerator())
            {
                m_has_more = enumerator.MoveNext();
                while (m_has_more)
                    yield return GetNextBatch(enumerator).ToList();
            }
        }

        private IEnumerable<T> GetNextBatch(IEnumerator<T> enumerator)
        {
            for (int i = 0; i < m_size; ++i)
            {
                yield return enumerator.Current;
                m_has_more = enumerator.MoveNext();
                if (!m_has_more)
                    yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private readonly IEnumerable<T> m_elements;
        private readonly int m_size;
        private bool m_has_more;
    }

    /// <summary>
    /// Insert element into sequence at given index
    /// e.g. { 1, 2, 3 } 42 1 ⇒ { 1, 42, 2, 3 }
    /// </summary>
    public static IEnumerable<T> InsertAt<T>(this IEnumerable<T> elements, T e, int index)
        => elements.Take(index).Concat(e.Yield()).Concat(elements.Skip(index));

    /// <summary>
    /// Convert a sequence into a context-aware sequence
    /// e.g. { 12, 42, 17 } ⇒ { [0, 12, 42],
    ///                            [12, 42, 17],
    ///                                [42, 17, 0] }
    /// </summary>
    public static IEnumerable<(T Previous, T Current, T Next)> WithPreviousAndNext<T>(this IEnumerable<T> elements)
    {
        var queue = new Queue<T>(2);
        queue.Enqueue(default(T));
        foreach (var e in elements)
        {
            if (queue.Count > 1)
                yield return (queue.Dequeue(), queue.Peek(), e);
            queue.Enqueue(e);
        }
        if (queue.Count > 1)
            yield return (queue.Dequeue(), queue.Peek(), default(T));
    }

    /// <summary>
    /// Intersperse a value between elements of a sequence
    /// e.g. { 1, 2, 3 }, 0 ⇒ { 1, 0, 2, 0, 3 }
    /// </summary>
    public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> elements, T delim)
        => elements.SelectMany((e) => new T[] { delim, e }).Skip(1);

    /// <summary>
    /// Convert a sequence into a hashset
    /// </summary>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> elements,
                                          IEqualityComparer<T> comparer = null)
        => new HashSet<T>(elements, comparer);

    /// <summary>
    /// Return whether array contains a given element
    /// </summary>
    public static bool Contains<T>(this T[] array, T element)
        where T : IComparable<T>
        => Array.Find(array, e => element.CompareTo(e) == 0) != null;
}
