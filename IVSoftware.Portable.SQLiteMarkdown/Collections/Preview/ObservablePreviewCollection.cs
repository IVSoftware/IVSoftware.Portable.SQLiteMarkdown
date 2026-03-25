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
            PreviewCollection.CollectionChanging += (sender, ePre) =>
                OnCollectionChanging(ePre);

            PreviewCollection.CollectionChanged += (sender, ePro) =>
                OnCollectionChanged(ePro);
        }
        protected override void ClearItems() => PreviewCollection.ClearItems(); protected override void InsertItem(int index, T item)
            => PreviewCollection.InsertItem(index, item);

        protected override void SetItem(int index, T item)
            => PreviewCollection.SetItem(index, item);

        protected override void MoveItem(int oldIndex, int newIndex)
            => PreviewCollection.MoveItem(oldIndex, newIndex);
        protected override void RemoveItem(int index)
            => PreviewCollection.RemoveItem(index);

        public IDisposable BeginBatch() => DHostBatch.GetToken(this);
        DHostBatchCollectionChange DHostBatch
        {
            get
            {
                if (_dhostBatch is null)
                {
                    _dhostBatch = new DHostBatchCollectionChange();
                    _dhostBatch.FinalDispose += (sender, e) =>
                    {
                        if (e is BatchFinalDisposeEventArgs eFD)
                        {
                            OnCollectionChanged(eFD.Digest);
                        }
                    };
                }
                return _dhostBatch;
            }
        }
        DHostBatchCollectionChange? _dhostBatch = null;

        bool _isUpdatingBase = false;
        protected virtual void OnCollectionChanging(NotifyCollectionChangingEventArgs e)
        {
            CollectionChanging?.Invoke(this, e);
        }
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

                    if (DHostBatch.TryApply(e))
                    {   /* G T K - N O O P */
                        // Batch is in progress.
                    }
                    else
                    {
                        base.OnCollectionChanged(e);
                    }
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

        /// <summary>
        /// CollectionChanging event that strictly belongs to this 
        /// class and is distinct, and not merely a `new` or `forwarded` 
        /// version of the same event in the Revertable class.
        /// </summary>
        public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;
    }
}
