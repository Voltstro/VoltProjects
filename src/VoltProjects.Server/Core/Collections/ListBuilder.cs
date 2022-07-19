// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace VoltProjects.Server.Core.Collections;

internal class ListBuilder<T> where T : notnull
{
    private readonly List<T> _array = new();

    public void Add(T item)
    {
        lock (_array)
        {
            _array.Add(item);
        }
    }

    public void AddRange(IReadOnlyList<T> items)
    {
        lock (_array)
        {
            for (var i = 0; i < items.Count; i++)
            {
                _array.Add(items[i]);
            }
        }
    }

    public bool Contains(T item)
    {
        lock (_array)
        {
            return _array.Contains(item);
        }
    }

    public void Sort(Comparison<T> comparison)
    {
        lock (_array)
        {
            _array.Sort(comparison);
        }
    }

    public IReadOnlyList<T> AsList()
    {
        lock (_array)
        {
            _array.TrimExcess();
            return _array;
        }
    }
}
