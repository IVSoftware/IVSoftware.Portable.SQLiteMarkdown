using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Xml.Linq.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Routed IList over a canonical collection with optional filtering.
    /// </summary>
    /// <remarks>
    /// - Provides a single read surface (Read) that switches between canonical
    ///   and filtered subsets based on FilteringState.
    /// - All read operations (Count, indexers, enumeration, Contains, CopyTo)
    ///   are routed through Read to ensure consistent view semantics.
    /// - All write operations are applied exclusively to the canonical superset.
    ///
    /// - Index-based mutations follow a strict rule:
    ///   - Indices originate from the routed view.
    ///   - Canonical position is resolved via identity (CanonIndexOf).
    ///   - This prevents divergence between filtered projections and canon.
    ///
    /// - The filtered subset is never treated as an authority and is not
    ///   mutated directly under any circumstance.
    ///
    /// - Add operations always append to the canonical superset.
    /// - Insert operations translate view index to canonical index.
    /// - RemoveAt resolves view index to canonical identity before removal.
    ///
    /// - Debugging process (leak elimination):
    ///   1. Identified all direct reads from canonical/subset collections.
    ///   2. Centralized all reads through a single routed Read surface.
    ///   3. Eliminated all writes to the filtered subset.
    ///   4. Fixed index-based mutations to translate view index to canon.
    ///   5. Unified Add through IList.Add to prevent dual write paths.
    ///   6. Ensured enumeration routes through IEnumerable interfaces only.
    ///   7. Introduced CanonIndexOf to enforce identity-based mapping.
    /// </remarks>
    partial class ModeledMarkdownContext<T> : IList
    {
        #region R O U T I N G
        public int Count =>
            FilteringState == FilteringState.Active
            ? PredicateMatchCount
            : CanonicalCount;

        IList Read =>
            FilteringState == FilteringState.Active
            ? PredicateMatchSubsetProtected
            : CanonicalSupersetProtected;
        IEnumerator IEnumerable.GetEnumerator() => Read.GetEnumerator();

        /// <summary>
        /// Resolves the canonical index for an item from the routed surface.
        /// </summary>
        /// <remarks>
        /// - Uses canonical superset as the single source of truth.
        /// - Throws if the item cannot be resolved to prevent silent drift.
        /// - Mental Model:
        ///   "Map view identity back to canonical position."
        /// </remarks>
        private int CanonIndexOf(object item)
        {
            if (item is not T valueT)
            {
                ThrowHard<InvalidCastException>(
                    $"Item must be assignable to {typeof(T).Name}."
                );
                return -1;
            }

            int index = CanonicalSupersetProtected.IndexOf(valueT);

            if (index < 0)
            {
                ThrowHard<InvalidOperationException>(
                    "Unable to resolve canonical index for routed item."
                );
            }

            return index;
        }
        #endregion R O U T I N G

        /// <summary>
        /// Gets or sets the element at the specified index via the routed view.
        /// </summary>
        /// <remarks>
        /// - Setter resolves the routed index back to canonical before mutation.
        /// - Enforces type safety and canonical authority.
        /// - Mental Model:
        ///   "Indices from the view; writes to the canon."
        /// </remarks>
        [Indexer]
        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is not T valueT)
                {
                    ThrowHard<InvalidCastException>(
                        $"IList.this setter requires value assignable to {typeof(T).Name}."
                    );
                    return;
                }

                if (FilteringState == FilteringState.Active)
                {
                    var canonIndex = CanonIndexOf(Read[index]!);
                    CanonicalSupersetProtected[canonIndex] = valueT;
                }
                else
                {
                    CanonicalSupersetProtected[index] = valueT;
                }
            }
        }

        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        int ICollection.Count => Read.Count;
        bool ICollection.IsSynchronized => ((IList)CanonicalSupersetProtected).IsSynchronized;
        object ICollection.SyncRoot => ((IList)CanonicalSupersetProtected).SyncRoot;

        /// <summary>
        /// Adds an item via the routed view, preserving canonical event semantics.
        /// </summary>
        /// <remarks>
        /// - Always mutates the canonical superset.
        /// - In filtering mode, appends to canon since view index has no stable
        ///   canonical meaning.
        /// - Returns the routed index consistent with IList semantics.
        /// - Mental Model:
        ///   "Adds land in the canon; the view reflects position."
        /// </remarks>
        int IList.Add(object value)
        {
            if (value is not T valueT)
            {
                ThrowHard<InvalidCastException>(
                    $"{nameof(IList.Add)} requires value assignable to {typeof(T).Name}."
                );
                return 0;
            }

            // Always append to canonical to preserve stable ordering + INCC semantics
            CanonicalSupersetProtected.Add(valueT);

            // Return routed index (view-relative)
            return ((IList)this).Count - 1;
        }

        /// <summary>
        /// "No surprises" clear on interface.
        /// </summary>
        void IList.Clear() => Clear(all: true);
        bool IList.Contains(object value)
            => value is T valueT && Read.Contains(valueT);

        /// <summary>
        /// Copies the routed view into the specified array.
        /// </summary>
        /// <remarks>
        /// - Uses the routed Read surface to ensure consistency with enumeration
        ///   and indexing semantics.
        /// - Avoids branching on filtering state to preserve a single authority.
        /// - Mental Model:
        ///   "Copy what the view sees."
        /// </remarks>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array is T[] typed)
            {
                for (int i = 0; i < Read.Count; i++)
                {
                    typed[index + i] = (T)Read[i]!;
                }
            }
            else
            {
                ThrowHard<ArrayTypeMismatchException>(
                    $"{nameof(ICollection.CopyTo)} requires array of type {typeof(T).Name}."
                );
            }
        }

        int IList.IndexOf(object item) =>
            item is T itemT ? Read.IndexOf(itemT) : -1;

        /// <summary>
        /// Inserts an item at the specified index via the routed view.
        /// </summary>
        /// <remarks>
        /// - In filtering mode, resolves the routed index to a canonical position
        ///   before insertion.
        /// - In canonical mode, inserts directly by index.
        /// </remarks>
        void IList.Insert(int index, object value)
        {
            if (value is not T valueT)
            {
                ThrowHard<InvalidCastException>(
                    $"{nameof(IList.Insert)} requires value assignable to {typeof(T).Name}."
                );
                return;
            }

            if (FilteringState == FilteringState.Active)
            {
                if (index >= Read.Count)
                {
                    CanonicalSupersetProtected.Add(valueT);
                }
                else
                {
                    var canonIndex = CanonIndexOf(Read[index]!);
                    CanonicalSupersetProtected.Insert(canonIndex, valueT);
                }
            }
            else
            {
                CanonicalSupersetProtected.Insert(index, valueT);
            }
        }

        void IList.Remove(object value)
        {
            if (value is T valueT)
            {
                CanonicalSupersetProtected.Remove(valueT);
            }
        }

        /// <summary>
        /// String formatter for the Model (an XElement).
        /// </summary>
        internal string ToString(FormattingOMC formatting) =>
            CanonicalSupersetProtected.ToString(formatting);

        /// <summary>
        /// String formatter for the EnumHistogrammer
        /// </summary>
        internal string ToString(FormattingEH formatting) =>
            CanonicalSupersetProtected.ToString(formatting);
    }

    partial class ModeledMarkdownContext<T> : IList<T>
    {
        #region R O U T I N G
        /// <summary>
        /// Iterating in this manner saves a tiny amount of Linq overhead.
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (var item in Read)
                yield return (T) item!;
        }
        #endregion R O U T I N G

        /// <summary>
        /// Gets or sets the element at the specified index via the routed view.
        /// </summary>
        /// <remarks>
        /// - Getter reads from the routed surface.
        /// - Setter resolves the routed index back to canonical before mutation.
        /// </remarks>
        [Indexer]
        public T this[int index]
        {
            get => (T)Read[index];
            set
            {
                if (FilteringState == FilteringState.Active)
                {
                    var canonIndex = CanonIndexOf(Read[index]!);
                    CanonicalSupersetProtected[canonIndex] = value;
                }
                else
                {
                    CanonicalSupersetProtected[index] = value;
                }
            }
        }

        T IList<T>.this[int index]
        {
            get => this[index];
            set => this[index] = value;
        }
        public override int CanonicalCount => CanonicalSuperset.Count;
        public override int PredicateMatchCount => PredicateMatchSubset.Count;

        public bool IsReadOnly => ((IList)this).IsReadOnly;

        /// <summary>
        /// Adds an item via the routed view.
        /// </summary>
        /// <remarks>
        /// - Delegates to IList.Add to enforce a single mutation path.
        /// - Always mutates the canonical superset.
        /// </remarks>
        public void Add(T item) => ((IList)this).Add(item);

        /// <summary>
        /// "No surprises" clear on interface.
        /// </summary>
        void ICollection<T>.Clear() => Clear(all: true);

        public bool Contains(T item) => Read.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            var source = Read;
            if (array.Length - arrayIndex < source.Count)
                throw new ArgumentException("Destination array is not large enough.");

            for (int i = 0; i < source.Count; i++)
            {
                array[arrayIndex + i] = (T)source[i]!;
            }
        }

        public int IndexOf(T item) => 
            item is T itemT ? Read.IndexOf(itemT) : -1;

        /// <summary>
        /// Inserts an item at the specified index via the routed view.
        /// </summary>
        /// <remarks>
        /// - In filtering mode, resolves the routed index to a canonical position
        ///   before insertion.
        /// - In canonical mode, inserts directly by index.
        /// - Mental Model:
        ///   "Indices from the view; inserts into the canon."
        /// </remarks>
        public void Insert(int index, T item)
        {
            if (FilteringState == FilteringState.Active)
            {
                if (index >= Read.Count)
                {
                    CanonicalSupersetProtected.Add(item);
                }
                else
                {
                    var canonIndex = CanonIndexOf(Read[index]!);
                    CanonicalSupersetProtected.Insert(canonIndex, item);
                }
            }
            else
            {
                CanonicalSupersetProtected.Insert(index, item);
            }
        }

        public bool Remove(T item)
        {
            if (((IList<T>)this).Contains(item))
            {
                CanonicalSupersetProtected.Remove(item);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the item at the specified index via the routed view.
        /// </summary>
        /// <remarks>
        /// - In filtering mode, the index is resolved against the routed subset and
        ///   removal is applied to the canonical superset to preserve authority.
        /// - In canonical mode, removal is applied directly by index.
        /// - Mental Model:
        ///   "Indices from the view; removals from the canon."
        /// </remarks>
        public void RemoveAt(int index)
        {
            if(FilteringState == FilteringState.Active)
            {
                CanonicalSupersetProtected.Remove((T)Read[index]!);
            }
            else
            {
                CanonicalSupersetProtected.RemoveAt(index);
            }
        }
    }
}