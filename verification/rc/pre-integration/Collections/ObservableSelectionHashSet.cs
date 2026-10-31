using IVSoftware.Portable.Common.Attributes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// Legacy selection helper retained to fulfill the SQLiteMarkdown v1 public contract.
    /// </summary>
    /// <remarks>
    /// - Backed by the legacy indexed <see cref="ObservableHashSet{T}"/> contract.
    /// - <see cref="ItemSelection"/> values intentionally remain compatible with
    ///   Xml.Linq.Collections tracking state values.
    /// - New modeled-collection work should prefer the TrackAttribute/ITrackContext tracking pipeline.
    /// </remarks>
    [PublishedContract("1.x")]
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
        private Func<bool>? _canMultiselect;

        public new bool Add(T item)
        {
            switch (SelectionMode)
            {
                case SelectionMode.None:
                    return false;
                case SelectionMode.Single:
                    if (!CanMultiselect())
                    {
                        Clear();
                    }
                    break;
            }

            return base.Add(item);
        }

        protected override void InsertItem(int index, T item)
        {
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
            CheckReentrancy();
            if (!Contains(item))
            {
                Subscribe(item);
                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];
            if (Equals(oldItem, item) || Contains(item)) return;
            CheckReentrancy();
            Unsubscribe(oldItem);
            Subscribe(item);
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            CheckReentrancy();
            Unsubscribe(item);
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            CheckReentrancy();
            foreach (var item in this.ToArray())
            {
                Unsubscribe(item);

                if (item is ISelectable selectable)
                {
                    selectable.Selection = ItemSelection.None;
                }
            }
            base.ClearItems();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISelectable.Selection)
                && sender is T item
                && sender is ISelectable selectable)
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

        private void Subscribe(T item)
        {
            if (item is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += OnItemPropertyChanged;
            }
        }

        private void Unsubscribe(T item)
        {
            if (item is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        public new event PropertyChangedEventHandler? PropertyChanged
        {
            add => base.PropertyChanged += value;
            remove => base.PropertyChanged -= value;
        }
    }
}
