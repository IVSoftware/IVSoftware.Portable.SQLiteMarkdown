using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        private readonly NotifyCollectionChangedEventArgs @base;

        public static implicit operator NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs eBCL)
            => new NotifyCollectionChangingEventArgs(eBCL);

        public static implicit operator NotifyCollectionChangedEventArgs(NotifyCollectionChangingEventArgs ePre)
            => ePre.@base;
        protected NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs eBCL) => @base = eBCL;
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, IList changedItems,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, changedItems);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, object changedItem,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, changedItem);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, IList newItems, IList oldItems,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, newItems, oldItems);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, IList changedItems, int startingIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, changedItems, startingIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, object changedItem, int index,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, changedItem, index);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, object newItem, object oldItem,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, newItem, oldItem);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, IList newItems, IList oldItems, int startingIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, newItems, oldItems, startingIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, IList changedItems, int index, int oldIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, changedItems, index, oldIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, object changedItem, int index, int oldIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, changedItem, index, oldIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action, object newItem, object oldItem, int index,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs((NotifyCollectionChangedAction)action, newItem, oldItem, index);
        }

        // ----------------------------------------
        // Projected contract
        // ----------------------------------------

        public NotifyCollectionChangeAction Action => (NotifyCollectionChangeAction)@base.Action;

        public NotifyCollectionChangeReason Reason { get; }

        public IList? NewItems => @base.NewItems;

        public IList? OldItems => @base.OldItems;

        public int NewStartingIndex => @base.NewStartingIndex;

        public int OldStartingIndex => @base.OldStartingIndex;
    }
}