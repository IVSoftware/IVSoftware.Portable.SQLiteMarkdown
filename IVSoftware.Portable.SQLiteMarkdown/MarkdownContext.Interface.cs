using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext : IList
    {
        public IList? NetVisible
        {
            get => _netVisible;
            set
            {
                if (!Equals(_netVisible, value))
                {
                    _netVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        IList? _netVisible = default;

        public bool IsFixedSize => NetVisible?.IsFixedSize ?? true;

        public bool IsReadOnly => NetVisible?.IsReadOnly ?? false;

        public int Count => NetVisible?.Count ?? 0;

        public bool IsSynchronized => NetVisible?.IsSynchronized ?? false;

        public object SyncRoot => NetVisible?.SyncRoot ?? false;
        public object this[int index]
        {
            get => GetNetVisibleOrThrow()?[index]!;
            set
            {
                var list = GetNetVisibleOrThrow();
                list?[index] = value;
            }
        }

        public int Add(object value)
            => GetNetVisibleOrThrow()?.Add(value) ?? -1;

        public void Clear()
            => GetNetVisibleOrThrow()?.Clear();

        public bool Contains(object value)
            => GetNetVisibleOrThrow()?.Contains(value) ?? false;

        public int IndexOf(object value)
            => GetNetVisibleOrThrow()?.IndexOf(value) ?? -1;

        public void Insert(int index, object value)
            => GetNetVisibleOrThrow()?.Insert(index, value);

        public void Remove(object value)
            => GetNetVisibleOrThrow()?.Remove(value);

        public void RemoveAt(int index)
            => GetNetVisibleOrThrow()?.RemoveAt(index);

        public void CopyTo(Array array, int index)
            => GetNetVisibleOrThrow()?.CopyTo(array, index);

        public IEnumerator GetEnumerator()
            => GetNetVisibleOrThrow()?.GetEnumerator() ?? Array.Empty<object>().GetEnumerator();
        private IList GetNetVisibleOrThrow()
        {
            if (NetVisible is null)
            {
                this.ThrowHard<NullReferenceException>(
                    @"
NetVisible is null.
This library considers IList operations invalid until a backing collection is assigned.
If ThrowHard is suppressed, operation will no-op or return default.");
                return null!; // We warned you.
            }

            return NetVisible;
        }
    }
}
