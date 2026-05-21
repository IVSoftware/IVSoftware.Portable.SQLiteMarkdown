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
using System.Xml.Linq;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// 1 of 4 interfaces in this file.
    /// </summary>
    partial class ObservableQueryFilterSource<T> 
        : IModeledCollection
        , IModelAuthorityContext // Allows delegation of MAC to CSS, to be consumed by MDC.
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

        SQLiteQueryOnlyConnection? IModeledCollection.FilterQueryDatabase
            => CanonicalSupersetProtected.FilterQueryDatabase;

        public bool HasAuthority(Enum authority)
            => CanonicalSupersetProtected.HasAuthority(authority);

        public IDisposable RequestAuthority(ModelDataExchangeAuthority authority) 
            => CanonicalSupersetProtected.RequestAuthority(authority);

        public IDisposable RequestAuthority(StdModelAuthority authority) 
            => CanonicalSupersetProtected.RequestAuthority(authority);
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
        public IReadOnlyList<T> CanonicalSuperset
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
        IReadOnlyList<T>? _canonicalSuperset = null;

        protected ObservableModeledCollection<T> CanonicalSupersetProtected
        {
            get => _canonicalSupersetProtected;
            set
            {
                if(value is null)
                {
                    this.ThrowHard<NullReferenceException>($"{nameof(CanonicalSuperset)} cannot be null.");
                }
                else
                {
                    if (!ReferenceEquals(_canonicalSuperset, value))
                    {
                        #region P R E
                        _canonicalSupersetProtected?.CollectionChanged -= CollectionChangedEventForwarder;
                        _canonicalSupersetProtected?.PropertyChanged -= PropertyChangedEventForwarder;
                        Model.Attribute(StdModelAttribute.mdc)?.Remove();
                        FilterQueryDatabase = null!;
                        #endregion P R E

                        _canonicalSupersetProtected = value;

                        #region P O S T
                        _ =
                        Model
                        .WithBoundAttributeValue(this, nameof(StdModelAttribute.mdc), "[MDC]")
                        .WithAttributesInOrder<StdModelAttribute>();

                        if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                        {
                            _canonicalSupersetProtected!.ModelTracking |= ModelTrackingFlag.ItemQueries;
                        }
                        _canonicalSupersetProtected?.CollectionChanged += CollectionChangedEventForwarder;
                        _canonicalSupersetProtected?.PropertyChanged += PropertyChangedEventForwarder;
                        OnPropertyChanged();
                        #endregion P O S T
                    }
                }
            }
        }
        ObservableModeledCollection<T> _canonicalSupersetProtected = null!;

        private void CollectionChangedEventForwarder(object sender, NotifyCollectionChangedEventArgs e)
            => OnCollectionChanged(e);

        /// <summary>
        /// FORWARDER FOR:
        /// - PropertyChangedEventArgs
        /// - EH_PropertyChangedEventArgs
        /// - ItemPropertyChangedEventArgs
        /// </summary>
        private void PropertyChangedEventForwarder(object sender, PropertyChangedEventArgs eUnk)
            => OnPropertyChanged(eUnk);

        public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public bool IsFixedSize => ((IList)CanonicalSuperset).IsFixedSize;

        public bool IsReadOnly => ((IList)CanonicalSuperset).IsReadOnly;

        [Careful("ROUTED: Do *not* cast. Not to IList. Not to ICollection.")]
        public int Count => CanonicalSupersetProtected.Count;

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
            using (RequestAuthority(StdModelAuthority.TerminalClear))
            {
                Clear(all: true);
                using (RequestAuthority(StdModelAuthority.SuspendForwardPropertyChange))
                {
                    InputText = string.Empty;
                }
            }
        }
        void IList.Clear() => Clear();
        void ICollection<T>.Clear() => Clear();

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


        [Careful("ROUTED: Do *not* cast. Not to ICollection. Not to IList.")]
        public IEnumerator GetEnumerator()
        {
            return CanonicalSupersetProtected.GetEnumerator();
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
