using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    partial class ObservableQueryFilterSource<T> : IModeledCollection
    {
        public XElement Model => ((IModeledCollection)ObservableModeledCollection).Model;

        public ModelTrackingFlag ModelTracking { get => ((IModeledCollection)ObservableModeledCollection).ModelTracking; set => ((IModeledCollection)ObservableModeledCollection).ModelTracking = value; }
        SQLiteQueryOnlyConnection? IModeledCollection.FilterQueryDatabase => ((IModeledCollection)ObservableModeledCollection).FilterQueryDatabase;

        public void SetObservableNetProjection(INotifyPreviewCollection? onp, NetProjectionTopology? topology = null)
        {
            ((IModeledCollection)ObservableModeledCollection).SetObservableNetProjection(onp, topology);
        }

        public INotifyPreviewCollection ObservableNetProjection 
            => ((IModeledCollection)ObservableModeledCollection).ObservableNetProjection;

    }
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
        public ObservableModeledCollection<T> ObservableModeledCollection
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

        public bool IsFixedSize => ((IList)ObservableModeledCollection).IsFixedSize;

        public bool IsReadOnly => ((IList)ObservableModeledCollection).IsReadOnly;

        public int Count => ((ICollection)ObservableModeledCollection).Count;

        public bool IsSynchronized => ((ICollection)ObservableModeledCollection).IsSynchronized;

        public object SyncRoot => ((ICollection)ObservableModeledCollection).SyncRoot;

        T IList<T>.this[int index] { get => ((IList<T>)ObservableModeledCollection)[index]; set => ((IList<T>)ObservableModeledCollection)[index] = value; }
        public object this[int index] { get => ((IList)ObservableModeledCollection)[index]; set => ((IList)ObservableModeledCollection)[index] = value; }

        ObservableModeledCollection<T>? _canonicalSupersetProtected = null;

        public int Add(object value)
        {
            return ((IList)ObservableModeledCollection).Add(value);
        }

        public void Clear()
        {
            ((IList)ObservableModeledCollection).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)ObservableModeledCollection).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)ObservableModeledCollection).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)ObservableModeledCollection).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)ObservableModeledCollection).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)ObservableModeledCollection).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)ObservableModeledCollection).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)ObservableModeledCollection).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)ObservableModeledCollection).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)ObservableModeledCollection).Insert(index, item);
        }

        public void Add(T item)
        {
            ((ICollection<T>)ObservableModeledCollection).Add(item);
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)ObservableModeledCollection).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)ObservableModeledCollection).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)ObservableModeledCollection).Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)ObservableModeledCollection).GetEnumerator();
        }
    }
}
