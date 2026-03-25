using IVSoftware.Portable.Common.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class ModeledMarkdownContext<T> : IList
    {
        [Indexer]
        public T this[int index]
        {
            get => Read[index];
            set
            {
                if (value is T valueT)
                {
                    CanonicalSupersetProtected[index] = valueT;
                }
                else
                {
                    ThrowHard<InvalidCastException>(
                        $"[Indexer] setter requires value assignable to {typeof(T).Name}."
                    );
                }
            }
        }
        [Indexer]
        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is T valueT)
                {
                    this[index] = valueT;
                }
                else
                {
                    ThrowHard<InvalidCastException>(
                        $"{nameof(IList.Add)} requires value assignable to {typeof(T).Name}."
                    );
                }
            }
        }

        IList<T> Read =>
            IsFiltering
            ? PredicateMatchSubsetPrivate
            : CanonicalSupersetProtected;

        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        int ICollection.Count => Read.Count;
        bool ICollection.IsSynchronized => ((IList)CanonicalSupersetProtected).IsSynchronized;
        object ICollection.SyncRoot => ((IList)CanonicalSupersetProtected).SyncRoot;

        int IList.Add(object value)
        {
            if (value is T valueT)
            {
                var index = ((IList)this).Count;
                CanonicalSupersetProtected.Insert(index, valueT);
                return index;
            }
            else
            {
                ThrowHard<InvalidCastException>(
                    $"{nameof(IList.Add)} requires value assignable to {typeof(T).Name}."
                );
                return 0;
            }
        }

        bool IList.Contains(object value) =>
            value is T valueT
            ? IsFiltering
                ? PredicateMatchSubset.Contains(valueT)
                : CanonicalSuperset.Contains(valueT)
            : false;

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is T[] typed)
            {
                if (IsFiltering)
                {
                    PredicateMatchSubsetPrivate.CopyTo(typed, index);
                }
                else
                {
                    CanonicalSupersetProtected.CopyTo(typed, index);
                }
            }
            else 
            { 
                ThrowHard<ArrayTypeMismatchException>(
                    $"{nameof(ICollection.CopyTo)} requires array of type {typeof(T).Name}."
                );
            }
        }

        int IList.IndexOf(object value) =>
            IsFiltering
            ? ((IList)PredicateMatchSubsetPrivate).IndexOf(value)
            : ((IList)CanonicalSupersetProtected).IndexOf(value);

        void IList.Insert(int index, object value)
        {
            if (value is T valueT)
            {
                CanonicalSupersetProtected.Insert(index, valueT);
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
                CanonicalSupersetProtected.Remove(valueT);
            }
        }
    }

    partial class ModeledMarkdownContext<T> : IList<T>
    {
        int IndexOfAction(CollectionChangeAction action, int index)
        {
            if(IsFiltering)
            {
                if(IsEphemeralSort)
                {

                }
                throw new NotImplementedException("ToDo");
            }
            else
            {
                return index;
            }
        }
        T IList<T>.this[int index]
        {
            get => (T)((IList)Read)[index];
            set => CanonicalSupersetProtected[index] = value!;
        }
        public override int CanonicalCount => CanonicalSuperset.Count;
        public override int PredicateMatchCount => PredicateMatchSubset.Count;
        public int Count =>
            IsFiltering
            ? PredicateMatchCount
            : CanonicalCount;

        public bool IsReadOnly => ((IList)this).IsReadOnly;

        public void Add(T item) => CanonicalSupersetProtected.Add(item);

        public void Clear() => CanonicalSupersetProtected.Clear();

        public bool Contains(T item) => Read.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item) => Read.IndexOf(item);

        public void Insert(int index, T item) => CanonicalSupersetProtected.Insert(index, item);

        public bool Remove(T item)
        {
            if (((IList<T>)this).Contains(item))
            {
                CanonicalSupersetProtected.Remove(item);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveAt(int index) => Read.RemoveAt(index);
    }
}