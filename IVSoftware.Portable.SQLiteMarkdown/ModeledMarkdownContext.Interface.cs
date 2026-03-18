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
        [Indexer]
        object IList.this[int index]
        {
            get => ((IList)Topology.Read)[index];
            set
            {
                if (value is T valueT)
                {
                    ((IList)Topology.Write)[index] = valueT;
                }
                else
                {
                    ThrowHard<InvalidCastException>(
                        $"[Indexer] setter requires value assignable to {typeof(T).Name}."
                    );
                }
            }
        }

        bool IList.IsFixedSize => Topology.IsFixedSize;
        bool IList.IsReadOnly => Topology.IsReadOnly;

        int ICollection.Count => Topology.Count;
        bool ICollection.IsSynchronized => Topology.IsSynchronized;
        object ICollection.SyncRoot => Topology.SyncRoot;

        int IList.Add(object value)
        {
            if (value is T valueT)
            {
                Topology.Write.Add(valueT);
                return Topology.Count - 1;
            }
            else
            {
                ThrowHard<InvalidCastException>(
                    $"{nameof(IList.Add)} requires value assignable to {typeof(T).Name}."
                );
                return 0;
            }
        }

        void IList.Clear()
            => Topology.Write.Clear();

        bool IList.Contains(object value)
            => value is T valueT && Topology.Contains(valueT);
        void ICollection.CopyTo(Array array, int index)
        {
            if (array is T[] typed)
            {
                Topology.CopyTo(typed, index);
            }
            else 
            { 
                ThrowHard<ArrayTypeMismatchException>(
                    $"{nameof(ICollection.CopyTo)} requires array of type {typeof(T).Name}."
                );
            }
        }

        int IList.IndexOf(object value)
            => value is T valueT ? Topology.IndexOf(valueT) : -1;

        void IList.Insert(int index, object value)
        {
            if (value is T valueT)
            {
                Topology.Write.Insert(index, valueT);
            }
            else
            {
                ThrowHard<InvalidCastException>(
                    $"{nameof(IList.Insert)} requires value assignable to {typeof(T).Name}."
                );
            }
        }

        void IList.Remove(object value)
        {
            if (value is T valueT)
            {
                Topology.Write.Remove(valueT);
            }
        }

        void IList.RemoveAt(int index)
        {
            var item = Topology.Read[index];
            Topology.Write.Remove(item);
        }
    }

    partial class ModeledMarkdownContext<T> : IList<T>
    {
        T IList<T>.this[int index]
        {
            get => (T)((IList)Topology.Read)[index];
            set => Topology.Write[index] = value!;
        }

        int ICollection<T>.Count => Topology.Count;
        bool ICollection<T>.IsReadOnly => Topology.IsReadOnly;

        void ICollection<T>.Add(T item)
            => Topology.Write.Add(item!);

        void ICollection<T>.Clear()
            => Topology.Write.Clear();

        bool ICollection<T>.Contains(T item)
            => Topology.Contains(item!);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            => Topology.CopyTo(array, arrayIndex);

        int IList<T>.IndexOf(T item)
            => Topology.IndexOf(item!);

        void IList<T>.Insert(int index, T item)
            => Topology.Write.Insert(index, item!);

        bool ICollection<T>.Remove(T item)
        {
            bool exists = Topology.Contains(item);
            Topology.Write.Remove(item!);
            return exists;
        }

        void IList<T>.RemoveAt(int index)
        {
            var item = Topology.Read[index];
            Topology.Write.Remove(item);
        }
    }
}