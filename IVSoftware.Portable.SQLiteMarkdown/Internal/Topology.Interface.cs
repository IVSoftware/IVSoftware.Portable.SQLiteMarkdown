using IVSoftware.Portable.Common.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

        #region P O L I C Y    A R B I T R A T I O N
        public int Count => Read.Count;

        [JsonIgnore]
        public bool IsFixedSize =>
            ((IList?)ObservableNetProjection)?.IsFixedSize
            ?? ((IList)CanonicalSupersetInternal).IsFixedSize;

        [JsonIgnore]
        public bool IsSynchronized =>
            ((IList?)ObservableNetProjection)?.IsSynchronized
            ?? ((IList)CanonicalSupersetInternal).IsSynchronized;

        [JsonIgnore]
        public object SyncRoot =>
            ((IList?)ObservableNetProjection)?.SyncRoot
            ?? ((IList)CanonicalSupersetInternal).SyncRoot;

        [JsonIgnore]
        public bool IsReadOnly => ((ICollection<T>)CanonicalSupersetInternal).IsReadOnly;
        #endregion P O L I C Y    A R B I T R A T I O N

        [Indexer]
        object? IList.this[int index] 
        { 
            get => Read[index];
            set => ((IList)CanonicalSupersetInternal)[index] = value;
        }
        public int Add(object value)
        {
            return ((IList)CanonicalSupersetInternal).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList)CanonicalSupersetInternal).Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)CanonicalSupersetInternal).CopyTo(array, index);
        }

        public int IndexOf(object value)
        {
            return ((IList)CanonicalSupersetInternal).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)CanonicalSupersetInternal).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)CanonicalSupersetInternal).Remove(value);
        }

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
            get => ((IList<T>)CanonicalSupersetInternal)[index];
            set => ((IList<T>)CanonicalSupersetInternal)[index] = value;
        }

        public void Add(T item)
        {
            ((ICollection<T>)CanonicalSupersetInternal).Add(item);
        }

        public void Clear()
        {
            ((ICollection<T>)CanonicalSupersetInternal).Clear();
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)CanonicalSupersetInternal).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)CanonicalSupersetInternal).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)CanonicalSupersetInternal).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)CanonicalSupersetInternal).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)CanonicalSupersetInternal).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)CanonicalSupersetInternal).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)CanonicalSupersetInternal).RemoveAt(index);
        }
    }

    partial class Topology<T> // IModeledMarkdownContext
    {
        /// <summary>
        /// Reports on whether this object is inherited or composed.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ProjectionTopology ProjectionTopology
        {
            get => _isInherited
                    ? ProjectionTopology.Inheritance
                    : ObservableNetProjection is null
                         ? ProjectionTopology.None
                         : ProjectionTopology.Composition;
        }

        readonly bool _isInherited;
    }
}
