using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    partial class ObservableQueryFilterSource<T> : INotifyPreviewCollection
    {
        public NotifyCollectionChangeScope EventScope
        { 
            get;
            set; 
        }

        public CollectionChangingEventingPolicy CollectionChangingEventingPolicy
        {
            get;
            set;
        }

        public event NotifyCollectionChangingEventHandler? CollectionChanging;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;
    }
    partial class ObservableQueryFilterSource<T> 
        : IList
        , IList<T>
    {
        public ObservableModeledCollection<T> CanonicalSupersetProtected
        {
            get
            {
                if (_canonicalSupersetProtected is null)
                {
                    _canonicalSupersetProtected = new ObservableModeledCollection<T>();
                }
                return _canonicalSupersetProtected;
            }
        }

        public bool IsFixedSize => ((IList)CanonicalSupersetProtected).IsFixedSize;

        public bool IsReadOnly => ((IList)CanonicalSupersetProtected).IsReadOnly;

        public int Count => ((ICollection)CanonicalSupersetProtected).Count;

        public bool IsSynchronized => ((ICollection)CanonicalSupersetProtected).IsSynchronized;

        public object SyncRoot => ((ICollection)CanonicalSupersetProtected).SyncRoot;

        T IList<T>.this[int index] { get => ((IList<T>)CanonicalSupersetProtected)[index]; set => ((IList<T>)CanonicalSupersetProtected)[index] = value; }
        public object this[int index] { get => ((IList)CanonicalSupersetProtected)[index]; set => ((IList)CanonicalSupersetProtected)[index] = value; }

        ObservableModeledCollection<T>? _canonicalSupersetProtected = null;

        public int Add(object value)
        {
            return ((IList)CanonicalSupersetProtected).Add(value);
        }

        public void Clear()
        {
            ((IList)CanonicalSupersetProtected).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)CanonicalSupersetProtected).Contains(value);
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

        public void RemoveAt(int index)
        {
            ((IList)CanonicalSupersetProtected).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)CanonicalSupersetProtected).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)CanonicalSupersetProtected).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)CanonicalSupersetProtected).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)CanonicalSupersetProtected).Insert(index, item);
        }

        public void Add(T item)
        {
            ((ICollection<T>)CanonicalSupersetProtected).Add(item);
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)CanonicalSupersetProtected).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)CanonicalSupersetProtected).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)CanonicalSupersetProtected).Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)CanonicalSupersetProtected).GetEnumerator();
        }
    }
}
