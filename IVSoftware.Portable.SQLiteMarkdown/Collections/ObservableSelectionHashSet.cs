using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public class ObservableSelectionHashSet<T> : ObservableHashSet<T>, INotifyPropertyChanged
    {
        public SelectionMode SelectionMode
        {
            get => _selectionMode;
            set
            {
                if (!Equals(_selectionMode, value))
                {
                    _selectionMode = value;
                    OnPropertyChanged();
                }
            }
        }
        private SelectionMode _selectionMode = SelectionMode.None;

        public Func<bool> CanMultiselect
        {
            get => _canMultiselect ?? (() => SelectionMode == SelectionMode.Multiple);
            set => _canMultiselect = value;
        }
        private Func<bool> _canMultiselect;

        public new bool Add(T item)
        {
            switch (SelectionMode)
            {
                case SelectionMode.None:
                    return false;
                case SelectionMode.Single:
                    if (!CanMultiselect())
                        ClearItems();
                    break;
            }
            if (item is INotifyPropertyChanged inpc)
                inpc.PropertyChanged += OnItemPropertyChanged;

            base.Add(item);
            return true;
        }

        protected override void ClearItems()
        {
            foreach (var item in this.ToArray())
            {
                if (item is INotifyPropertyChanged inpc)
                    inpc.PropertyChanged -= OnItemPropertyChanged;

                if (item is ISelectable selectable)
                    selectable.Selection = ItemSelection.None;
            }
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];

            if (item is INotifyPropertyChanged inpc)
                inpc.PropertyChanged -= OnItemPropertyChanged;

            base.RemoveItem(index);
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISelectable.Selection)
                && sender is T item && sender is ISelectable selectable)
            {
                switch (selectable.Selection)
                {
                    case ItemSelection.None:
                        Remove(item);
                        break;
                    case ItemSelection.Exclusive:
                        Add(item);
                        break;
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        public new event PropertyChangedEventHandler PropertyChanged
        {
            add => base.PropertyChanged += value;
            remove => base.PropertyChanged -= value;
        }
    }

#if false
    public class ObservableSelectionHashSet<T> : ObservableCollection<T>, INotifyPropertyChanged
    {
        private readonly HashSet<T> _set = new HashSet<T>();
        public new void Add(T item)
        {
            switch (SelectionMode)
            {
                case SelectionMode.None:
                    return;
                case SelectionMode.Single:
                    if (!CanMultiselect())
                    {
                        ClearItems();
                    }
                    break;
                case SelectionMode.Multiple:
                    break;
            }
            if (_set.Add(item))
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged += OnItemPropertyChanged;
                }
                base.Add(item);
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISelectableQueryFilterItem.Selection):
                    if (sender is T item && sender is ISelectableQueryFilterItem selectable)
                    {
                        switch (selectable.Selection)
                        {
                            case ItemSelection.None:
                                Remove(item);
                                break;
                            case ItemSelection.Exclusive:
                                Add(item);
                                break;
                        }
                    }
                    break;
            }
        }

        public Func<bool> CanMultiselect
        {
            get => _canMultiselect is null
                ? () => SelectionMode == SelectionMode.Multiple
                : _canMultiselect;
            set => _canMultiselect = value;
        }
        Func<bool> _canMultiselect = null;

        protected override void ClearItems()
        {
            foreach (var item in _set.ToArray())
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged -= OnItemPropertyChanged;
                }
                if (item is ISelectableQueryFilterItem selectable)
                {
                    selectable.Selection = ItemSelection.None;
                }
            }
            base.ClearItems();
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
                        if (oldItem is INotifyPropertyChanged inpc)
                        {
                            inpc.PropertyChanged -= OnItemPropertyChanged;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _set.Clear();
                    break;
            }
        }
        /// <summary>
        /// Support replace.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];

            if (!Equals(oldItem, item) && _set.Add(item))
            {
                _set.Remove(oldItem);
                base.SetItem(index, item);
            }
        }
        public new bool Contains(T item) => _set.Contains(item);

        public SelectionMode SelectionMode
        {
            get => _selectionMode;
            set
            {
                if (!Equals(_selectionMode, value))
                {
                    _selectionMode = value;
                    OnPropertyChanged();
                }
            }
        }
        SelectionMode _selectionMode = SelectionMode.None;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        public new event PropertyChangedEventHandler PropertyChanged
        {
            add => base.PropertyChanged += value;
            remove => base.PropertyChanged -= value;
        }
    }
#endif
}