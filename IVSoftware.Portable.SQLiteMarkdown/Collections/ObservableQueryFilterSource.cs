using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// POC as a separate-but-equal generic IList impl, but not a BC so far.
    /// </summary>
    [Canonical("CTor partial")]
    public abstract class ObservableQueryFilterSource
        : MarkdownContext<object>
        , IObservableQueryFilterSource<object>
        , IModeledCollection
        , IList
        , IList<object>
    {
        DisposableHost IXObjectChangeEventSink.DisableXObjectChangeEvents => throw new NotImplementedException("ToDo");
        public object this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public XElement Model => throw new NotImplementedException();
        public new IReadOnlyDictionary<StdModelAttribute, int> Histo => throw new NotImplementedException();

        public ModelTrackingFlag ModelTracking { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IDictionary<Enum, IAuthorityEpochProvider> AuthorityProviders => throw new NotImplementedException();

        public ModelDataExchangeAuthority ModelDataExchangeAuthority => throw new NotImplementedException();

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public string Placeholder => throw new NotImplementedException();

        public string Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string SQL => throw new NotImplementedException();

        SQLiteQueryOnlyConnection? IModeledCollection.FilterQueryDatabase => throw new NotImplementedException();

        public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool HasAuthority(Enum authority)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void InitializeFilterOnlyMode(IEnumerable<object> items)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void ReplaceItems(IEnumerable<object> items)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceItemsAsync(IEnumerable<object> items)
        {
            throw new NotImplementedException();
        }

        void ICollection<object>.Add(object item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        bool ICollection<object>.Remove(object item)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public ListOptimizationMode OptimizationMode { get; set; } = ListOptimizationMode.None;
    }
}
