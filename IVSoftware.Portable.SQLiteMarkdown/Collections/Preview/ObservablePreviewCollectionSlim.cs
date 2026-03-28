using IVSoftware.Portable.Disposable;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using IVSoftware.Portable.SQLiteMarkdown;
using System.Collections;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class ObservablePreviewCollectionSlim<T>
        : ObservableCollection<T>
        , INotifyCollectionChanging
    {
        /// <summary>
        /// Defines the extent to which a preview handler may interact with a pending
        /// collection change proposal.
        /// </summary>
        /// <remarks>
        /// This enumeration constrains what a handler is permitted to do during the
        /// preview (Changing) phase. It does not describe the change itself, but rather
        /// the allowed level of participation in shaping or rejecting it.
        ///
        /// - ReadOnly   : Observe only. No modification or cancellation is permitted.
        /// - CancelOnly : The proposal may be rejected but not altered.
        /// - FullControl: The proposal may be rewritten or rejected entirely.
        ///
        /// These flags are enforced by the preview pipeline. Handlers opting into
        /// higher scopes assume responsibility for producing a valid and internally
        /// consistent change contract.
        /// </remarks>
        public NotifyCollectionChangeScope EventScope { get; }

        protected override void InsertItem(int index, T item)
        {
            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Add,
                scope: EventScope,
                newItems: new[] { item },
                newStartingIndex: index);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);
                if (!e.Cancel)
                {
                    using (DHostApply.GetToken())
                    {
                        this.Apply(e);
                    }
                }
            }
            else
            {
                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Replace,
                scope: EventScope,
                newItems: new[] { item },
                oldItems: new[] { oldItem },
                newStartingIndex: index,
                oldStartingIndex: index);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);
                if (!e.Cancel)
                {
                    using (DHostApply.GetToken())
                    {
                        this.Apply(e);
                    }
                }
            }
            else
            {
                base.SetItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Remove,
                scope: EventScope,
                oldItems: new[] { item },
                oldStartingIndex: index);

            if(DHostApply.IsZero())
            {
                OnCollectionChanging(e);
                if (!e.Cancel)
                {
                    if (e.IsModified)
                    {
                        using (DHostApply.GetToken())
                        {
                            this.Apply(e);
                        }
                    }
                    else 
                    {
                        base.RemoveItem(index);
                    }
                }
            }
            else
            {
                base.RemoveItem(index);
            }
        }
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var item = this[oldIndex];

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Move,
                scope: EventScope,
                newItems: new[] { item },
                oldItems: new[] { item },
                newStartingIndex: newIndex,
                oldStartingIndex: oldIndex);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);
                if (!e.Cancel)
                {
                    using (DHostApply.GetToken())
                    {
                        this.Apply(e);
                    }
                }
            }
            else
            {
                base.MoveItem(oldIndex, newIndex);
            }
        }

        protected override void ClearItems()
        {
            var snapshot = this.ToArray();

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Reset,
                scope: EventScope,
                oldItems: snapshot,
                oldStartingIndex: -1);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);
                if (!e.Cancel)
                {
                    using (DHostApply.GetToken())
                    {
                        this.Apply(e);
                    }
                }
            }
            else
            {
                base.ClearItems();
            }
        }

        protected virtual void OnCollectionChanging(NotifyCollectionChangingEventArgs e)
        {
            CollectionChanging?.Invoke(this, e);
        }
        public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;

        DisposableHost DHostApply { get; } = new();
    }
}
