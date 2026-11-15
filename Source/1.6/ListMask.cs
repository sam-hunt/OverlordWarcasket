using System;
using System.Collections.Generic;

namespace OverlordWarcasket;

// A reversible removal from lists another mod owns and reads: Hide takes every matching item
// out of a list, Restore puts each one back at the index it left. Items are taken from the end
// of a list toward its start and returned in the opposite order, so every recorded index is the
// item's own in the untouched list, across any number of Hide calls on any number of lists.
public sealed class ListMask<T>
{
    private readonly List<(List<T> list, int index, T item)> hidden = new();

    public void Hide(List<T> list, Predicate<T> match)
    {
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var item = list[i];
            if (!match(item))
                continue;
            hidden.Add((list, i, item));
            list.RemoveAt(i);
        }
    }

    public void Restore()
    {
        for (var i = hidden.Count - 1; i >= 0; i--)
        {
            var (list, index, item) = hidden[i];
            list.Insert(index, item);
        }
        hidden.Clear();
    }
}
