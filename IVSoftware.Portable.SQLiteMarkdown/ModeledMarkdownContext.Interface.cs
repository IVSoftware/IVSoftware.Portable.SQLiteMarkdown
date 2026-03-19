using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class ModeledMarkdownContext<T> : IList
    {
        public bool IsFixedSize => ((IList)Topology).IsFixedSize;

        public bool IsReadOnly => ((IList)Topology).IsReadOnly;

        public int Count => ((ICollection)Topology).Count;

        public bool IsSynchronized => ((ICollection)Topology).IsSynchronized;

        public object SyncRoot => ((ICollection)Topology).SyncRoot;


        [Indexer]
        public object this[int index] 
        {
            get => ((IList)Topology)[index]; 
            set => ((IList)Topology)[index] = value; 
        }

        public int Add(object value)
        {
            return ((IList)Topology).Add(value);
        }

        public void Clear()
        {
            ((IList)Topology).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)Topology).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)Topology).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)Topology).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)Topology).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)Topology).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)Topology).CopyTo(array, index);
        }
    }

    partial class ModeledMarkdownContext<T> : IList<T>
    {

        [Indexer]
        T IList<T>.this[int index] 
        { 
            get => ((IList<T>)Topology)[index];
            set => ((IList<T>)Topology)[index] = value; 
        }

        public void Add(T item)
        {
            ((ICollection<T>)Topology).Add(item);
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)Topology).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)Topology).CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)Topology).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)Topology).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)Topology).Remove(item);
        }
    }

    partial class ModeledMarkdownContext<T> : IModeledMarkdownContext<T>
    {
        IList? IModeledMarkdownContext.ObservableNetProjection => ObservableNetProjection;
    }
}