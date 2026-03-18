using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{

    class RevertableObservableCollection<T> : ObservableCollection<T>
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

        bool _isReverting = false;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_isReverting)
            {
                bool? isMutableB4 = NotifyCollectionChangingEventArgs.IsMutableDefault;
                using (this.WithOnDispose(
                    onInit: (sender, e) =>
                    {
                        NotifyCollectionChangingEventArgs.IsMutableDefault = true;
                    },
                    onDispose: (sender, e) =>
                    {
                        NotifyCollectionChangingEventArgs.IsMutableDefault = isMutableB4;
                    }))
                {
                    NotifyCollectionChangingEventArgs ePre = e;
                    CollectionChanging?.Invoke(this, ePre);
                    if(ePre.Cancel)
                    {
                        try
                        {
                            _isReverting = true;
                            {
                                Clear();
                                foreach (var item in PreviewCollection)
                                {
                                    Add(item);
                                }
                            }
                        }
                        finally
                        {
                            _isReverting = false;
                        }
                    }
                    else
                    {
                        // Copy with possible mutations.
                        base.OnCollectionChanged(ePre);
                    }
                }
            }
            else
            {   /* G T K - N O O P */
            }
        }
        public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;
    }
}
