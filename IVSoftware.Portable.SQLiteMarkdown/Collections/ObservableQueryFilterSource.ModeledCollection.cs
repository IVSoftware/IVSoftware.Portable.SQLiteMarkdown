using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// 1 of 4 interfaces in this file.
    /// </summary>
    partial class ObservableQueryFilterSource<T> : IModeledCollection
    {
        public XElement Model => CanonicalSupersetProtected.Model;

        public ModelTrackingFlag ModelTracking { get => ((IModeledCollection)CanonicalSupersetProtected).ModelTracking; set => ((IModeledCollection)CanonicalSupersetProtected).ModelTracking = value; }

        public IList? ObservableNetProjection => ((IModeledCollection)CanonicalSupersetProtected).ObservableNetProjection;

        SQLiteQueryOnlyConnection? IModeledCollection.FilterQueryDatabase => ((IModeledCollection)CanonicalSupersetProtected).FilterQueryDatabase;

        public void SetObservableNetProjection(INotifyPreviewCollection? onp, NetProjectionTopology? topology = null)
        {
            ((IModeledCollection)CanonicalSupersetProtected).SetObservableNetProjection(onp, topology);
        }
    }

    /// <summary>
    /// 2 of 4 interfaces in this file.
    /// </summary>
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

    /// <summary>
    /// 3 of 4 interfaces in this file.
    /// </summary>
    partial class ObservableQueryFilterSource<T> 
        : IList
        , IList<T>
    {
        public IReadOnlyCollection<T> CanonicalSuperset
        {
            get
            {
                if (_canonicalSuperset is null)
                {
                    _canonicalSuperset = new ReadOnlyCollection<T>(CanonicalSupersetProtected);
                }
                return _canonicalSuperset;
            }
        }
        IReadOnlyCollection<T>? _canonicalSuperset = null;

        protected ObservableModeledCollection<T> CanonicalSupersetProtected
        {
            get
            {
                if (_canonicalSupersetProtected is null)
                {
                    _canonicalSupersetProtected = new ObservableModeledCollection<T>();

                    // NOTE: This is *not* an INotifyCollectionChanging API per published contract.
                    _canonicalSupersetProtected.CollectionChanged += (sender, e) =>
                    {
                    };
                }
                return _canonicalSupersetProtected;
            }
        }

        ObservableModeledCollection<T>? _canonicalSupersetProtected = null;

        public bool IsFixedSize => ((IList)CanonicalSuperset).IsFixedSize;

        public bool IsReadOnly => ((IList)CanonicalSuperset).IsReadOnly;

        public int Count => ((ICollection)CanonicalSuperset).Count;

        public bool IsSynchronized => ((ICollection)CanonicalSuperset).IsSynchronized;

        public object SyncRoot => ((ICollection)CanonicalSuperset).SyncRoot;

        T IList<T>.this[int index] 
        { 
            get => ((IList<T>)CanonicalSuperset)[index];
            set => ((IList<T>)CanonicalSupersetProtected)[index] = value;
        }
        public object this[int index] 
        { 
            get => ((IList)CanonicalSuperset)[index];
            set => ((IList)CanonicalSupersetProtected)[index] = value;
        }

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
            return ((IList)CanonicalSuperset).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)CanonicalSuperset).IndexOf(value);
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
            ((ICollection)CanonicalSuperset).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)CanonicalSuperset).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)CanonicalSuperset).IndexOf(item);
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
            return ((ICollection<T>)CanonicalSuperset).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)CanonicalSuperset).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)CanonicalSupersetProtected).Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)CanonicalSuperset).GetEnumerator();
        }
    }

    /// <summary>
    /// 4 of 4 interfaces in this file.
    /// </summary>
    partial class ObservableQueryFilterSource<T>
    : IRoutedEnumerable<T, RoutingOQFS>
    {
        public int GetCount(RoutingOQFS route)
        {
            var e = GetEnumerator(route);

            int count = 0;
            while (e.MoveNext())
            {
                count++;
            }
            return count;
        }

        public int GetCount(Enum route)
            => GetCount((RoutingOQFS)route);

        public IEnumerator<T> GetEnumerator(RoutingOQFS route)
        {
            switch (route)
            {
                case RoutingOQFS.CanonicalSuperset:
                    // TODO: Routing
                    return ((IEnumerable<T>)this).GetEnumerator();

                case RoutingOQFS.PredicateMatchSubset:
                    // TODO: Routing
                    return ((IEnumerable<T>)this).GetEnumerator();

                default:
                    this.ThrowFramework<NotSupportedException>(
                        $"The {route.ToFullKey()} case is not supported.");
                    return ((IEnumerable<T>)this).GetEnumerator();
            }
        }

        public IEnumerator GetEnumerator(Enum route)
        {
            var e = GetEnumerator((RoutingOQFS)route);

            while (e.MoveNext())
            {
                yield return e.Current!;
            }
        }

        #region L E G A C Y    H O O K S
        public override int CanonicalCount
            => GetCount(RoutingOQFS.CanonicalSuperset);

        public override int PredicateMatchCount
            => GetCount(RoutingOQFS.PredicateMatchSubset);
        #endregion
    }
}
