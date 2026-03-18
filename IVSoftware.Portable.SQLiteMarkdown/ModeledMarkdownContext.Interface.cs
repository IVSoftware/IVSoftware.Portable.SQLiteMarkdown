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
            get => ((IList)Routing.Read)[index];
            set
            {
                if (value is T valueT)
                {
                    ((IList)Routing.Write)[index] = valueT;
                }
                else
                {
                    ThrowHard<InvalidCastException>(
                        $"[Indexer] setter requires value assignable to {typeof(T).Name}."
                    );
                }
            }
        }

        bool IList.IsFixedSize => Routing.IsFixedSize;
        bool IList.IsReadOnly => Routing.IsReadOnly;

        int ICollection.Count => Routing.Count;
        bool ICollection.IsSynchronized => Routing.IsSynchronized;
        object ICollection.SyncRoot => Routing.SyncRoot;

        int IList.Add(object value)
        {
            if (value is T valueT)
            {
                return Routing.Write.Add(valueT);
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
            => Routing.Write.Clear();

        bool IList.Contains(object value)
            => value is T valueT && Routing.Contains(valueT);

        void ICollection.CopyTo(Array array, int index)
            => Routing.CopyTo(array, index);

        int IList.IndexOf(object value)
            => value is T valueT ? ((IList)Routing.Read).IndexOf(valueT) : -1;

        void IList.Insert(int index, object value)
        {
            if (value is T valueT)
            {
                Routing.Write.Insert(index, valueT);
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
                Routing.Write.Remove(valueT);
            }
        }

        void IList.RemoveAt(int index)
            => Routing.Write.RemoveAt(index);
    }

    partial class ModeledMarkdownContext<T> : IList<T>
    {
        T IList<T>.this[int index]
        {
            get => (T)((IList)Routing.Read)[index];
            set => Routing.Write[index] = value!;
        }

        int ICollection<T>.Count => Routing.Count;
        bool ICollection<T>.IsReadOnly => Routing.IsReadOnly;

        void ICollection<T>.Add(T item)
            => Routing.Write.Add(item!);

        void ICollection<T>.Clear()
            => Routing.Write.Clear();

        bool ICollection<T>.Contains(T item)
            => Routing.Contains(item!);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            => Routing.Read.CopyTo(array, arrayIndex);

        int IList<T>.IndexOf(T item)
            => ((IList)Routing.Read).IndexOf(item!);

        void IList<T>.Insert(int index, T item)
            => Routing.Write.Insert(index, item!);

        bool ICollection<T>.Remove(T item)
        {
            bool exists = Routing.Contains(item);
            Routing.Write.Remove(item!);
            return exists;
        }

        void IList<T>.RemoveAt(int index)
            => Routing.Write.RemoveAt(index);
    }
}