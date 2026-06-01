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
    /// - Backed by the legacy <see cref="ObservableHashSet{T}"/> type identity, which now laterals to
    ///   the canonical composed hash set in Xml.Linq.Collections.
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

            if (base.Add(item))
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged += OnItemPropertyChanged;
                }
                return true;
            }
            return false;
        }

        public new bool Remove(T item)
        {
            if (base.Remove(item))
            {
                Unsubscribe(item);
                return true;
            }
            return false;
        }

        public new void Clear()
        {
            foreach (var item in this.ToArray())
            {
                Unsubscribe(item);

                if (item is ISelectable selectable)
                {
                    selectable.Selection = ItemSelection.None;
                }
            }
            base.Clear();
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

        private void Unsubscribe(T item)
        {
            if (item is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
