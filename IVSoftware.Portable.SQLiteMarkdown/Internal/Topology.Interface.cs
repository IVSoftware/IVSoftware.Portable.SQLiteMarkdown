using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    partial class Topology<T> // : IList
    {
        public int IndexOf(T value)
            => Read is IList list ? list.IndexOf(value) : -1;

        public object GetAt(int index)
            => ((IList)Read)[index];

        #region P O L I C Y    A R B I T R A T I O N
        public int Count => Read.Count;

        [JsonIgnore]
        public bool IsSynchronized => Write.IsSynchronized;

        [JsonIgnore]
        public object SyncRoot => Write.SyncRoot;

        [JsonIgnore]
        public bool IsFixedSize { get; internal set; }

        [JsonIgnore]
        public bool IsReadOnly { get; internal set; }

        public object this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public int IndexOf(object value)
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

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        #endregion P O L I C Y    A R B I T R A T I O N
    }


    partial class Topology<T> //  : IList<T>
    {

        public void CopyTo(T[] array, int index)
        {
            for (int i = 0; i < Read.Count; i++)
            {
                array[index + i] = Read[i];
            }
        }
        public bool Contains(T value)
        {
            if (value?.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
            {
                return PlacerResult.Exists == Model.Place(full, PlacerMode.FindOrPartial);
            }
            else
            {
                return false;
            }
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
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
