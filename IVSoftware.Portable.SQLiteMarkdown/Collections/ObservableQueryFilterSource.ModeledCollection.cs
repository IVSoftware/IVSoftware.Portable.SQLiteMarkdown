using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    partial class ObservableQueryFilterSource<T> : IModeledCollection
    {
        public XElement Model => ((IModeledCollection)ModeledCollection).Model;

        public ModelTrackingFlag ModelTracking { get => ((IModeledCollection)ModeledCollection).ModelTracking; set => ((IModeledCollection)ModeledCollection).ModelTracking = value; }
        SQLiteQueryOnlyConnection? IModeledCollection.FilterQueryDatabase => ((IModeledCollection)ModeledCollection).FilterQueryDatabase;

        public void SetObservableNetProjection(INotifyPreviewCollection? onp, NetProjectionTopology? topology = null)
        {
            ((IModeledCollection)ModeledCollection).SetObservableNetProjection(onp, topology);
        }

        public IEnumerator GetEnumerator(Enum route)
        {
            return ((IRoutedEnumerable)ModeledCollection).GetEnumerator(route);
        }

        public int GetCount(Enum route)
        {
            return ((IRoutedEnumerable)ModeledCollection).GetCount(route);
        }

        public INotifyPreviewCollection? ObservableNetProjection 
            => ((IModeledCollection)ModeledCollection).ObservableNetProjection;
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
        public IReadOnlyCollection<T> ModeledCollection
        {
            get
            {
                if (_modeledCollection is null)
                {
                    _modeledCollection = new ReadOnlyCollection<T>(ModeledCollectionProtected);
                }
                return _modeledCollection;
            }
        }
        IReadOnlyCollection<T>? _modeledCollection = null;


        enum TGFindMe_CanonicalSupersetProtected { }

        protected ObservableModeledCollection<T> ModeledCollectionProtected
        {
            get
            {
                if (_modeledCollectionProtected is null)
                {
                    _modeledCollectionProtected = new ObservableModeledCollection<T>();
                    // NOTE: This is *not* an INotifyCollectionChanging API per published contract.
                    ((INotifyCollectionChanging)_modeledCollectionProtected)
                        .CollectionChanging += (sender, e) =>
                    {
                    };

                    _modeledCollectionProtected.CollectionChanged += (sender, e) =>
                    {
                    };
                }
                return _modeledCollectionProtected;
            }
        }

        public bool IsFixedSize => ((IList)ModeledCollection).IsFixedSize;

        public bool IsReadOnly => ((IList)ModeledCollection).IsReadOnly;

        public int Count => ((ICollection)ModeledCollection).Count;

        public bool IsSynchronized => ((ICollection)ModeledCollection).IsSynchronized;

        public object SyncRoot => ((ICollection)ModeledCollection).SyncRoot;

        T IList<T>.this[int index] { get => ((IList<T>)ModeledCollection)[index]; set => ((IList<T>)ModeledCollection)[index] = value; }
        public object this[int index] { get => ((IList)ModeledCollection)[index]; set => ((IList)ModeledCollection)[index] = value; }

        ObservableModeledCollection<T>? _modeledCollectionProtected = null;

        public int Add(object value)
        {
            return ((IList)ModeledCollection).Add(value);
        }

        public void Clear()
        {
            ((IList)ModeledCollection).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)ModeledCollection).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)ModeledCollection).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)ModeledCollection).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)ModeledCollection).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)ModeledCollection).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)ModeledCollection).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)ModeledCollection).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)ModeledCollection).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)ModeledCollection).Insert(index, item);
        }

        public void Add(T item)
        {
            ((ICollection<T>)ModeledCollection).Add(item);
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)ModeledCollection).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)ModeledCollection).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)ModeledCollection).Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)ModeledCollection).GetEnumerator();
        }
    }
}
