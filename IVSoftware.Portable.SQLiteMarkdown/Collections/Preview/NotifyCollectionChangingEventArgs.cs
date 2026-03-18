using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;


namespace IVSoftware.Portable.Collections
{
    public enum NotifyCollectionChangingAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset
    }
    public class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        private readonly NotifyCollectionChangedEventArgs @base;

        private NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs eBCL) => @base = eBCL;
        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action)
            => @base = new NotifyCollectionChangedEventArgs(action);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, IList changedItems)
            => @base = new NotifyCollectionChangedEventArgs(action, changedItems);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, object changedItem)
            => @base = new NotifyCollectionChangedEventArgs(action, changedItem);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
            => @base = new NotifyCollectionChangedEventArgs(action, newItems, oldItems);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
            => @base = new NotifyCollectionChangedEventArgs(action, changedItems, startingIndex);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
            => @base = new NotifyCollectionChangedEventArgs(action, changedItem, index);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem)
            => @base = new NotifyCollectionChangedEventArgs(action, newItem, oldItem);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
            => @base = new NotifyCollectionChangedEventArgs(action, newItems, oldItems, startingIndex);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
            => @base = new NotifyCollectionChangedEventArgs(action, changedItems, index, oldIndex);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
            => @base = new NotifyCollectionChangedEventArgs(action, changedItem, index, oldIndex);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
            => @base = new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index);

        // ----------------------------------------
        // Projected contract
        // ----------------------------------------

        public NotifyCollectionChangedAction Action => @base.Action;

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