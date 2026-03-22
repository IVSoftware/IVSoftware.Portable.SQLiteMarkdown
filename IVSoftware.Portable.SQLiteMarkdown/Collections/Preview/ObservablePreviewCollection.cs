using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class ObservablePreviewCollection<T> : ObservableCollection<T>
    {
        public ObservablePreviewCollection(bool useMutablePreviewEvents = false)
        {
            PreviewCollection = new RevertableObservableCollection<T>(this, useMutablePreviewEvents);

            // The preview collection doesn't always change.
            // This event signals that it has done so, i.e., the action was not canceled.
            PreviewCollection.CollectionChanged += (sender, e) =>
                OnCollectionChanged(e);
        }
        protected override void ClearItems() => PreviewCollection.ClearItems(); protected override void InsertItem(int index, T item)
            => PreviewCollection.InsertItem(index, item);

        protected override void SetItem(int index, T item)
            => PreviewCollection.SetItem(index, item);

        protected override void MoveItem(int oldIndex, int newIndex)
            => PreviewCollection.MoveItem(oldIndex, newIndex);
        protected override void RemoveItem(int index)
            => PreviewCollection.RemoveItem(index);

        bool _isUpdatingBase = false;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_isUpdatingBase)
            {
                try
                {
                    _isUpdatingBase = true;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.NewItems is not null)
                            {
                                for (int i = 0; i < e.NewItems.Count; i++)
                                {
                                    base.InsertItem(e.NewStartingIndex + i, (T)e.NewItems[i]!);
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            if (e.OldItems is not null)
                            {
                                for (int i = 0; i < e.OldItems.Count; i++)
                                {
                                    base.RemoveItem(e.OldStartingIndex);
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            if (e.NewItems is not null)
                            {
                                for (int i = 0; i < e.NewItems.Count; i++)
                                {
                                    base.SetItem(e.NewStartingIndex + i, (T)e.NewItems[i]!);
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            if (e.NewItems is not null)
                            {
                                for (int i = 0; i < e.NewItems.Count; i++)
                                {
                                    base.MoveItem(e.OldStartingIndex + i, e.NewStartingIndex + i);
                                }
                            }
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            base.ClearItems();
                            break;
                    }
                    base.OnCollectionChanged(e);
                }
                finally
                {
                    _isUpdatingBase = false;
                }
            }
            else
            {   /* G T K - N O O P */
                // Expected reentrancy.
            }
        }
        RevertableObservableCollection<T> PreviewCollection { get; }

        public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging
        {
            add => PreviewCollection.CollectionChanging += value;
            remove => PreviewCollection.CollectionChanging -= value;
        }
    }
}
