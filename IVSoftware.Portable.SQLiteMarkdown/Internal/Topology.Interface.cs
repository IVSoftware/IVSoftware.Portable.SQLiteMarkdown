using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    [JsonObject]
    partial class Topology<T> : IList
    {
        #region P O L I C Y    A R B I T R A T I O N
        bool IList.IsFixedSize => throw new NotImplementedException();

        bool IList.IsReadOnly => throw new NotImplementedException();

        int ICollection.Count => throw new NotImplementedException();

        bool ICollection.IsSynchronized => throw new NotImplementedException();

        object ICollection.SyncRoot => throw new NotImplementedException();

        object IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int IndexOf(T value)
            => Read is IList list ? list.IndexOf(value) : -1;

        public object GetAt(int index)
            => ((IList)Read)[index];

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => Read.GetEnumerator();
        #endregion P O L I C Y    A R B I T R A T I O N
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
