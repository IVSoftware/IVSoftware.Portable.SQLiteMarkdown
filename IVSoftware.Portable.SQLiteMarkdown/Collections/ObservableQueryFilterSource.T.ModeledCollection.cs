using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// 1 of 4 interfaces in this file.
    /// </summary>
    partial class ObservableQueryFilterSource<T> 
        : IModeledCollection
    {
        public XElement Model => CanonicalSupersetProtected.Model;

        public ModelTrackingFlag ModelTracking
        { 
            get => ((IModeledCollection)CanonicalSupersetProtected).ModelTracking; 
            set => ((IModeledCollection)CanonicalSupersetProtected).ModelTracking = value;
        }

        public ModelDataExchangeAuthority ModelDataExchangeAuthority => 
            ((IModeledCollection)CanonicalSupersetProtected).ModelDataExchangeAuthority;

        /// <summary>
        /// Histo is guaranteed not null in this subclass; null is forgiven.
        /// </summary>
        public new IReadOnlyDictionary<StdModelAttribute, int> Histo => base.Histo!;

        public IDictionary<Enum, IAuthorityEpochProvider> AuthorityProviders => 
            ((IModeledCollection)CanonicalSupersetProtected).AuthorityProviders;

        SQLiteQueryOnlyConnection? IModeledCollection.FilterQueryDatabase => 
            ((IModeledCollection)CanonicalSupersetProtected).FilterQueryDatabase;

        public bool HasAuthority(Enum authority)
        {
            return ((IModeledCollection)CanonicalSupersetProtected).HasAuthority(authority);
        }
    }

    /// <summary>
    /// 2 of 4 interfaces in this file.
    /// </summary>
    partial class ObservableQueryFilterSource<T> 
        : INotifyPreviewCollection
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

        [Probationary("When ObservablePreviewRangeCollection<T> becomes a public type, we will upgrade.")]
        public ObservableModeledCollection<T> CanonicalSupersetProtected
        {
            get
            {
                if (_canonicalSupersetProtected is null)
                {
                    _canonicalSupersetProtected = new ObservablePreviewRangeCollection<T>();

                    Model.Attribute(StdModelAttribute.mdc)?.Remove();
                    _ = 
                    Model
                    .WithBoundAttributeValue(this, nameof(StdModelAttribute.mdc), "[MDC]")
                    .WithAttributesInOrder<StdModelAttribute>(
                        reqSMA: (authority) => CanonicalSupersetProtected.RequestAuthority(authority));

                    _canonicalSupersetProtected?.CollectionChanged += CollectionChangedEventForwarder;
                    _canonicalSupersetProtected?.PropertyChanged += PropertyChangedEventForwarder;
                    OnPropertyChanged();
                }
                return _canonicalSupersetProtected!;
            }
        }
        ObservableModeledCollection<T> _canonicalSupersetProtected = null;

        private void CollectionChangedEventForwarder(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Bubble the event up to ONP, unless ONP is the authority to begin with.
            if (CanonicalSupersetProtected.ModelDataExchangeAuthority 
                != ModelDataExchangeAuthority.ObservableNetCollection)
            {
                OnCollectionChanged(e);
            }
        }

        /// <summary>
        /// FORWARDER FOR:
        /// - PropertyChangedEventArgs
        /// - EH_PropertyChangedEventArgs
        /// - ItemPropertyChangedEventArgs
        /// </summary>
        private void PropertyChangedEventForwarder(object sender, PropertyChangedEventArgs eUnk)
        {
            OnPropertyChanged(eUnk);
        }

        public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

#if false && SAVE_FOR_NOW
        protected ObservableModeledCollection<T> CanonicalSupersetProtected
        {
            get
            {
                if (_canonicalSupersetProtected is null)
                {
                    _canonicalSupersetProtected = new ObservableModeledCollection<T>();

                    // [Careful]
                    // Must be unsubscribable, not lambda.
                    _canonicalSupersetProtected.CollectionChanged += CollectionChangedEventForwarder;
                }
                return _canonicalSupersetProtected;
            }
            set
            {
                if(value is null)
                {
                    this.ThrowHard<InvalidOperationException>(
                        $"{nameof(CanonicalSupersetProtected)} cannot be null. This path is intended for interface upgrades.");
                }
                else if(!ReferenceEquals(value, CanonicalSuperset))
                {
                    CanonicalSupersetProtected.CollectionChanged -= CollectionChangedEventForwarder;
                    _canonicalSuperset = value;
                    CanonicalSupersetProtected.CollectionChanged += CollectionChangedEventForwarder;
                }
            }
        }
        ObservableModeledCollection<T>? _canonicalSupersetProtected = null;


        private void CollectionChangedEventForwarder(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCanonicalSupersetCollectionChanged(e);
        }

        protected virtual void OnCanonicalSupersetCollectionChanged(NotifyCollectionChangedEventArgs e) 
        {
            if(ModelTracking.HasFlag(ModelTrackingFlag.ItemPropertyChanges))
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems is not null)
                        {
                            foreach (var item in e.NewItems.OfType<T>())
                            {
                                localAddINPC(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems is not null)
                        {
                            foreach (var item in e.OldItems.OfType<T>())
                            {
                                localRemoveINPC(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                }
            }
            if(ModelTracking.HasFlag(ModelTrackingFlag.ItemQueries))
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems is not null)
                        {
                            foreach (var item in e.NewItems.OfType<T>())
                            {
                                localInsertOrReplace(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems is not null)
                        {
                            foreach (var item in e.OldItems.OfType<T>())
                            {
                                localDelete(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                }
            }
            #region L o c a l F x
            void localAddINPC(T item)
            {
                if(item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged += PropertyChangedEventForwarder;
                }
            }
            void localRemoveINPC(T item)
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged -= PropertyChangedEventForwarder;
                }
            }
            void localInsertOrReplace(T item)
            {
                FilterQueryDatabase.InsertOrReplace(item);
            }
            void localDelete(T item)
            {
                FilterQueryDatabase.Delete(item);
            }
            #endregion L o c a l F x
        }

        private void PropertyChangedEventForwarder(object sender, PropertyChangedEventArgs e)
        {
            if (sender is T itemT)
            {
                OnPropertyChanged(new ItemPropertyChangedEventArgs(e.PropertyName, itemT));
            }
            else
            {
                this.ThrowHard<InvalidCastException>(
                    $"Expecting INPC senders will be {typeof(T).Name} at all times.");
            }
        }
#endif

        public bool IsFixedSize => ((IList)CanonicalSuperset).IsFixedSize;

        public bool IsReadOnly => ((IList)CanonicalSuperset).IsReadOnly;

        public int Count => ((ICollection)CanonicalSuperset).Count;

        public bool IsSynchronized => ((ICollection)CanonicalSuperset).IsSynchronized;

        public object SyncRoot => ((ICollection)CanonicalSuperset).SyncRoot;

        public T this[int index]
        {
            get => CanonicalSupersetProtected[index];
            set => CanonicalSupersetProtected[index] = value;
        }

        object IList.this[int index]
        {
            get => ((IList)CanonicalSuperset)[index];
            set => CanonicalSupersetProtected[index] = (T)value;
        }

        public int Add(object value)
        {
            T valueT = (T)value;
            CanonicalSupersetProtected.Add((T)valueT);
            return CanonicalSupersetProtected.IndexOf(valueT);
        }

        /// <summary>
        /// No Surprises IList.Clear
        /// </summary>
        public void Clear()
        {
            Clear(all: true);
            CanonicalSupersetProtected.Clear();
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
            CanonicalSupersetProtected.Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            CanonicalSupersetProtected.Remove((T)value);
        }

        public void RemoveAt(int index)
        {
            CanonicalSupersetProtected.RemoveAt(index);
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
            return CanonicalSupersetProtected.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            CanonicalSupersetProtected.Insert(index, item);
        }

        public void Add(T item)
        {
            CanonicalSupersetProtected.Add(item);
        }

        public bool Contains(T item)
        {
            return CanonicalSupersetProtected.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CanonicalSupersetProtected.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return CanonicalSupersetProtected.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return CanonicalSupersetProtected.GetEnumerator();
        }
    }
}
