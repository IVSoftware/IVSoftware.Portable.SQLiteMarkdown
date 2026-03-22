using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    /// <summary>
    /// An action describes either a Changed or Changing event.
    /// </summary>
    internal enum NotifyCollectionChangeAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset
    }
    internal class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        private readonly NotifyCollectionChangedEventArgs @base;

        protected NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs eBCL) => @base = eBCL;
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, IList changedItems,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, changedItems);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, object changedItem,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, changedItem);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, IList newItems, IList oldItems,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, newItems, oldItems);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, IList changedItems, int startingIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, changedItems, startingIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, object changedItem, int index,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, changedItem, index);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, object newItem, object oldItem,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, newItem, oldItem);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, newItems, oldItems, startingIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, changedItems, index, oldIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, changedItem, index, oldIndex);
        }

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedAction action, object newItem, object oldItem, int index,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Reason = reason;
            @base = new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index);
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

        public static implicit operator NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs eBCL)
            => new NotifyCollectionChangingEventArgs(eBCL);

        public static implicit operator NotifyCollectionChangedEventArgs(NotifyCollectionChangingEventArgs ePre)
            => ePre.@base;
    }
}