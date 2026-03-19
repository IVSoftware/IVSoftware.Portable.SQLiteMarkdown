using IVSoftware.Portable.Common.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    partial class Topology<T> : IList
    {
        public bool IsFixedSize =>
            ((IList?)ObservableNetCollection)?.IsFixedSize
            ?? ((IList)CanonicalSupersetProtected).IsFixedSize;

        public bool IsSynchronized =>
            ((IList?)ObservableNetCollection)?.IsSynchronized
            ?? ((IList)CanonicalSupersetProtected).IsSynchronized;

        public object SyncRoot =>
            ((IList?)ObservableNetCollection)?.SyncRoot
            ?? ((IList)CanonicalSupersetProtected).SyncRoot;

        #region P O L I C Y    A R B I T R A T I O N

        [Indexer]
        object IList.this[int index] 
        { 
            get => Read[index];
            set
            {
                using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Projection))
                { 
                    ((IList)CanonicalSupersetProtected)[index] = value;
                }
            }
        }
        public int Add(object value)
        {
            return ((IList)CanonicalSupersetProtected).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList)CanonicalSupersetProtected).Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)CanonicalSupersetProtected).CopyTo(array, index);
        }

        public int IndexOf(object value)
        {
            return ((IList)CanonicalSupersetProtected).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)CanonicalSupersetProtected).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)CanonicalSupersetProtected).Remove(value);
        }
        #endregion P O L I C Y    A R B I T R A T I O N

        IEnumerator IEnumerable.GetEnumerator() => Read.GetEnumerator();
    }

    /// <summary>
    /// Read routed, Write with authority.
    /// </summary>
    partial class Topology<T> : IList<T>
    {
        [Indexer]
        public T this[int index]
        { 
            get => ((IList<T>)CanonicalSupersetProtected)[index];
            set 
            { 
                ((IList<T>)CanonicalSupersetProtected)[index] = value;
            }
        }

        public int Count => ((ICollection<T>)CanonicalSupersetProtected).Count;

        public bool IsReadOnly => ((ICollection<T>)CanonicalSupersetProtected).IsReadOnly;

        public void Add(T item)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Projection))
            {
                ((ICollection<T>)CanonicalSupersetProtected).Add(item);
            }
        }

        public void Clear()
        {
            ((ICollection<T>)CanonicalSupersetProtected).Clear();
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)CanonicalSupersetProtected).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)CanonicalSupersetProtected).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)CanonicalSupersetProtected).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)CanonicalSupersetProtected).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)CanonicalSupersetProtected).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)CanonicalSupersetProtected).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)CanonicalSupersetProtected).RemoveAt(index);
        }
    }

    partial class Topology<T> // : IModeledMarkdownContext<T>
    {
        public ProjectionTopology ProjectionTopology { get; }

        public NetProjectionOption ProjectionOption { get; set; }
        public ReplaceItemsEventingOption ReplaceItemsEventingOptions { get; set; }
        public ObservableCollection<T>? ObservableNetProjection { get; set; }

        public event NotifyCollectionChangedEventHandler? ModelSettled;

        public void LoadCanon(IEnumerable? recordset)
        {
            throw new NotImplementedException();
        }

        public Task LoadCanonAsync(IEnumerable? recordset)
        {
            throw new NotImplementedException();
        }
    }
}
