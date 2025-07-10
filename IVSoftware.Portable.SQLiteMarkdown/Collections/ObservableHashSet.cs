using IVSoftware.Portable.Disposable;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
	public class ObservableHashSet : ObservableCollection<object>
	{
		private readonly HashSet<object> _hashSet = new HashSet<object>();
        public new bool Add(object item)
        {
            lock (_lock)
            {
                if (_hashSet.Add(item))
                {
                    base.InsertItem(Count, item);
                    return true;
                }
                return false;
            }
        }

        public new bool Insert(int index, object item)
        {
            lock (_lock)
            {
                if (_hashSet.Add(item))
                {
                    base.InsertItem(index, item);
                    return true;
                }
                return false;
            }
        }

        protected override void SetItem(int index, object item)
		{
			var oldItem = this[index];
			if (Equals(oldItem, item)) return;

			if (_hashSet.Contains(item)) return; // no dupes

			_hashSet.Remove(oldItem);
			_hashSet.Add(item);
			base.SetItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			var item = this[index];
			_hashSet.Remove(item);
			base.RemoveItem(index);
		}
        public new bool Contains(object item) => _hashSet.Contains(item);

        protected override void ClearItems()
        {
            lock (_lock)
            {
                var oldItems = _hashSet.ToList();
                _hashSet.Clear();

                // Skip base.ClearItems to avoid double event
                // Instead, manually remove items to avoid calling SetItem or similar
                base.Items.Clear();

                // Custom event with old items
                OnCollectionChanged(new NotifyCollectionResetEventArgs(oldItems));
            }
        }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }
        readonly object _lock = new object();
    }
    public class ObservableHashSet<T> : ObservableCollection<T>
    {
        private readonly HashSet<T> _hashSet = new HashSet<T>();
        private readonly object _lock = new object();

        public new bool Add(T item)
        {
            lock (_lock)
            {
                if (_hashSet.Add(item))
                {
                    base.InsertItem(Count, item);
                    return true;
                }
                return false;
            }
        }

        public new bool Insert(int index, T item)
        {
            lock (_lock)
            {
                if (_hashSet.Add(item))
                {
                    base.InsertItem(index, item);
                    return true;
                }
                return false;
            }
        }

        public new bool Contains(T item) => _hashSet.Contains(item); // O(1) lookup

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];
            if (Equals(oldItem, item)) return;

            if (_hashSet.Contains(item)) return;

            _hashSet.Remove(oldItem);
            _hashSet.Add(item);
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            _hashSet.Remove(item);
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            lock (_lock)
            {
                var oldItems = _hashSet.ToList();
                _hashSet.Clear();

                // Skip base.ClearItems to avoid double event
                // Instead, manually remove items to avoid calling SetItem or similar
                base.Items.Clear();

                // Custom event with old items
                OnCollectionChanged(new NotifyCollectionResetEventArgs(oldItems));
            }
        }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }
    }
    public class NotifyCollectionResetEventArgs : NotifyCollectionChangedEventArgs
    {
        public NotifyCollectionResetEventArgs(IList oldItems) : base(NotifyCollectionChangedAction.Reset)
        {
            OldItems = oldItems;
        }
        public new IList OldItems { get; }
    }
}
