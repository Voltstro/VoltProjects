// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace VoltProjects.Server.Core.Collections;

/// <summary>
///     Thread-safe builder of a <see cref="List{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class ListBuilder<T> where T : notnull
{
    private readonly List<T> _array = new();

    /// <summary>
    ///     Add an item
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        lock (_array)
        {
            _array.Add(item);
        }
    }

    /// <summary>
    ///     Add a range (list) of items
    /// </summary>
    /// <param name="items"></param>
    public void AddRange(IReadOnlyList<T> items)
    {
        lock (_array)
        {
            for (int i = 0; i < items.Count; i++)
            {
                _array.Add(items[i]);
            }
        }
    }

    /// <summary>
    ///     Does this list contain an item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
        lock (_array)
        {
            return _array.Contains(item);
        }
    }

    /// <summary>
    ///     Sort this list
    /// </summary>
    /// <param name="comparison"></param>
    public void Sort(Comparison<T> comparison)
    {
        lock (_array)
        {
            _array.Sort(comparison);
        }
    }

    /// <summary>
    ///     Convert this list to a <see cref="IReadOnlyList{T}"/>
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<T> AsList()
    {
        lock (_array)
        {
            _array.TrimExcess();
            return _array;
        }
    }
}
