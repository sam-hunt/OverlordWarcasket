using System;
using System.Collections.Generic;

namespace OverlordWarcasket;

// A reversible removal from lists another mod owns and reads: Hide takes every matching item
// out of a list, Restore puts each one back.
//
// An item returns by rank, ahead of the first entry that outranks it, never to the index it
// left. The lists are shared, and a sibling mod masking its own items overlaps this mask without
// nesting inside it: Harmony runs finalizers in the order of their prefixes, so the first mask
// to hide is the first to restore, into a list the second has since shortened. An index
// recorded at Hide is then stale, past the end of the list or one place off. Rank is the order
// the owner filled the list in, so on a list still in that order every item lands where it was,
// whatever other masks have out at the time and in whatever order they restore.
public sealed class ListMask<T>
{
    private readonly Func<T, int> rank;
    private readonly List<(List<T> list, T item)> hidden = new();

    public ListMask(Func<T, int> rank) => this.rank = rank;

    public void Hide(List<T> list, Predicate<T> match)
    {
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var item = list[i];
            if (!match(item))
                continue;
            hidden.Add((list, item));
            list.RemoveAt(i);
        }
    }

    // Hide collects from the end of a list, so walking back through the record returns equally
    // ranked items in the order they were in.
    public void Restore()
    {
        for (var i = hidden.Count - 1; i >= 0; i--)
        {
            var (list, item) = hidden[i];
            var itemRank = rank(item);
            var index = 0;
            while (index < list.Count && rank(list[index]) <= itemRank)
                index++;
            list.Insert(index, item);
        }
        hidden.Clear();
    }
}
