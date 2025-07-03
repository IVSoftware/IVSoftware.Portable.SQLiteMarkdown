using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public class ObservableHashSet<T> : ObservableCollection<T>
    {
        private readonly HashSet<T> _set = new HashSet<T>();
        public new void Add(T item)
        {
            if (_set.Add(item))
            {
                base.Add(item);
            }
        }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in e.OldItems.OfType<T>())
                    {
                        _set.Remove(oldItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _set.Clear();
                    break;
            }
        }
        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];

            if (!Equals(oldItem, item) && _set.Add(item))
            {
                _set.Remove(oldItem);
                base.SetItem(index, item);
            }
        }
    }
}
