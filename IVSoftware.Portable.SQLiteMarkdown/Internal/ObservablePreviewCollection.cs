using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    class ObservablePreviewCollection<T> : ObservableCollection<T>
    {
        protected override void ClearItems() => PreviewCollection.ClearItems(); protected override void InsertItem(int index, T item)
            => PreviewCollection.InsertItem(index, item);

        protected override void SetItem(int index, T item)
            => PreviewCollection.SetItem(index, item);

        protected override void MoveItem(int oldIndex, int newIndex)
            => PreviewCollection.MoveItem(oldIndex, newIndex);

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Forward only after canonical reconciliation (or suppress if Preview owns it)
            base.OnCollectionChanged(e);
        }
        RevertableObservableCollection PreviewCollection
        {
            get
            {
                if (_previewCollection is null)
                {
                    _previewCollection = new RevertableObservableCollection(this);
                }
                return _previewCollection;
            }
        }
        RevertableObservableCollection? _previewCollection = null;

        class RevertableObservableCollection : ObservableCollection<T>
        {
            public RevertableObservableCollection(ObservableCollection<T> previewCollection)
            {
                PreviewCollection = previewCollection;
            }
            ObservableCollection<T> PreviewCollection;
            public new void ClearItems() => base.ClearItems();

            public new void InsertItem(int index, T item)
                => base.InsertItem(index, item);

            public new void RemoveItem(int index)
                => base.RemoveItem(index);

            public new void SetItem(int index, T item)
                => base.SetItem(index, item);

            public new void MoveItem(int oldIndex, int newIndex)
                => base.MoveItem(oldIndex, newIndex);
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}
