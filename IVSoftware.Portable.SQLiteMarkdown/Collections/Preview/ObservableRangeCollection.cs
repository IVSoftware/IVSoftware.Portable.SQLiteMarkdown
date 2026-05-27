using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    internal class ObservableRangeCollection<T>
        : ObservableModeledCollection<T>
        , IRangeable
        , INotifyPreviewCollection
    {
        public ObservableRangeCollection() { }

        public event NotifyCollectionChangingEventHandler? CollectionChanging
        {
            add => NotifyCollectionChangingImpl.CollectionChanging += value;
            remove => NotifyCollectionChangingImpl.CollectionChanging -= value;
        }

        public new CollectionChangingEventingPolicy CollectionChangingEventingPolicy
        {
            get => base.CollectionChangingEventingPolicy;
            set => base.CollectionChangingEventingPolicy = value;
        }

        public void AddRange(IEnumerable items)
            => base
            .AsInterface<IRangeable>()
            !.AddRange(items);

        public int AddRangeDistinct(IEnumerable items)
            => base
            .AsInterface<IRangeable>()
            !.AddRangeDistinct(items);

        public void InsertRange(int startingIndex, IEnumerable items)
            => base
            .AsInterface<IRangeable>()
            !.InsertRange(startingIndex, items);

        public int RemoveMultiple(IEnumerable items)
            => base
            .AsInterface<IRangeable>()
            !.RemoveMultiple(items);

        /// <summary>
        /// Remove items from the starting index to the ending index, inclusive.
        /// </summary>
        public void RemoveRange(int startingIndex, int endingIndex) =>
            base
            .AsInterface<IRangeable>()
            !.RemoveRange(startingIndex, endingIndex);
    }


#if false && REFERENCE_ONLY
        public void AddRange(IEnumerable items) => RangeableImpl.AddRange(items);

        public int AddRangeDistinct(IEnumerable items)
        {
            XElement inertModel = new(Model);

            if (typeof(T).GetModeledPathInfo().GetPath is not GetPathDlgt dlgt)
            {
                this.ThrowHard<InvalidOperationException>(
                    $"{nameof(GetPathDlgt)} is reqired to form path for distinct.");
                return 0;
            }
            else
            {
                int changed = 0;
                using (RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
                {
                    int newStartingIndex = Count;
                    foreach (var item in items)
                    {
                        if (item is T itemT && dlgt(itemT) is string fullPath)
                        {
                            if (string.IsNullOrWhiteSpace(fullPath))
                            {
                                "ObservablePreviewCollection".ThrowHard<ArgumentException>($"The '{nameof(fullPath)}' argument cannot be empty.");
                                CancelModelAuthorityEpoch();
                                return 0;
                            }

                            switch (inertModel.Place(fullPath))
                            {
                                case PlacerResult.Created:
                                    InsertItem(newStartingIndex++, itemT);
                                    changed++;
                                    break;
                                default:
                                    /* G T K - N O O P */
                                    // Skipping this item as non-distinct.
                                    break;
                            }
                        }
                        else
                        {
                            item.ThrowHard<InvalidCastException>($"All range items must be {typeof(T).Name}");
                            CancelModelAuthorityEpoch();
                            return 0;
                        }
                    }
                    return changed;
                }
            }
        }
        public void InsertRange(int startingIndex, IEnumerable items)
        {
            using (RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
            {
                foreach (var item in items)
                {
                    if (item is T itemT)
                    {
                        // [Careful]
                        // Use Insert.
                        // We can't use Add because the collection
                        // doesn't actually change until the end.
                        InsertItem(startingIndex++, itemT);
                    }
                    else
                    {
                        item.ThrowHard<InvalidCastException>($"All range items must be {typeof(T).Name}");
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate the items, removing one of:
        /// - The first instance of each T in items.
        /// - The indexes listed in items (all must be valid).
        /// - (TODO) Applying custom range descriptors passed as items.
        /// </summary>
        public int RemoveMultiple(IEnumerable items)
        {
            int count = 0;
            int removed = 0;

            List<T> itemsT = new();
            List<int> indexes = new();

            foreach (var unk in items)
            {
                count++;
                switch (unk)
                {
                    case T itemT:
                        itemsT.Add(itemT);
                        break;

                    case int index:
                        if (index < 0 || index >= Count)
                        {
                            this.ThrowHard<IndexOutOfRangeException>(
                                $"Index {index} must be in range [0..{Count - 1}].");
                            return 0;   // Only reachable if consumer handles the Throw.
                        }
                        indexes.Add(index);
                        break;

                    default:
                        this.ThrowHard<InvalidCastException>(
                            $"Unsupported item type: {unk?.GetType().Name ?? "null"}.");
                        return 0;
                }
            }

            if (itemsT.Count == count)
            {
                using (RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
                {
                    foreach (var item in itemsT)
                    {
                        int index = IndexOf(item);
                        if (index >= 0)
                        {
                            RemoveAt(index);
                            removed++;
                        }
                    }
                }
            }
            else if (indexes.Count == count)
            {
                var seen = new HashSet<int>();
                foreach (var i in indexes)
                {
                    if (!seen.Add(i))
                    {
                        this.ThrowHard<InvalidOperationException>(
                            "Duplicate indexes are not allowed.");
                        return 0;
                    }
                }
                using (RequestAuthority(ModelDataExchangeAuthority.CollectionDeferred))
                {
                    foreach (var removeAt in indexes.OrderByDescending(_ => _))
                    {
                        RemoveAt(removeAt);
                        removed++;
                    }
                }
            }
            else
            {
                this.ThrowHard<AmbiguousMatchException>(
                     $"Ambiguous input: expected all items to be either '{typeof(T).Name}' or 'int'. " +
                     $"Observed mix: {itemsT.Count} '{typeof(T).Name}', {indexes.Count} 'int', " +
                     $"{count - itemsT.Count - indexes.Count} unsupported.");
                return 0;
            }
            return removed;
        }
#endif
}
