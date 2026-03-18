using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;


namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public enum NotifyCollectionChangingAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset
    }

    public sealed class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        public NotifyCollectionChangingAction Action { get; }

        public IList? NewItems { get; }
        public IList? OldItems { get; }

        public int NewStartingIndex { get; } = -1;
        public int OldStartingIndex { get; } = -1;

        public static bool? IsMutableDefault { get; set; }
        public bool IsMutable { get; private set; }

        static IList? Freeze(IList? source, bool isMutable)
        {
            if (source is null || isMutable)
                return source;

            return source.IsReadOnly ? source : ArrayList.ReadOnly(source);
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a Reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            bool? isMutable = null)
        {
            Action = action;
            IsMutable = isMutable ?? IsMutableDefault ?? false;
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a one-item change.
        /// </summary>
        /// <param name="action">
        /// The action that caused the event; can only be Reset, Add, or Remove action.
        /// </param>
        /// <param name="changedItem">The item affected by the change.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            object? changedItem)
            : this(action)
        {
            if (changedItem is not null)
            {
                if (action == NotifyCollectionChangingAction.Add)
                    NewItems = Freeze(new object[] { changedItem }, IsMutable);
                else
                    OldItems = Freeze(new object[] { changedItem }, IsMutable);
            }
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            object? changedItem,
            int index)
            : this(action, changedItem)
        {
            if (action == NotifyCollectionChangingAction.Add)
                NewStartingIndex = index;
            else
                OldStartingIndex = index;
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            IList? changedItems)
            : this(action)
        {
            if (action == NotifyCollectionChangingAction.Add)
                NewItems = Freeze(changedItems, IsMutable);
            else
                OldItems = Freeze(changedItems, IsMutable);
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a multi-item change (or a Reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            IList? changedItems,
            int startingIndex)
            : this(action, changedItems)
        {
            if (action == NotifyCollectionChangingAction.Add)
                NewStartingIndex = startingIndex;
            else
                OldStartingIndex = startingIndex;
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            object? newItem,
            object? oldItem)
            : this(action)
        {
            NewItems = newItem is null ? null : Freeze(new object[] { newItem }, IsMutable);
            OldItems = oldItem is null ? null : Freeze(new object[] { oldItem }, IsMutable);
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            object? newItem,
            object? oldItem,
            int index)
            : this(action, newItem, oldItem)
        {
            NewStartingIndex = index;
            OldStartingIndex = index;
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            IList newItems,
            IList oldItems)
            : this(action)
        {
            NewItems = Freeze(newItems, IsMutable);
            OldItems = Freeze(oldItems, IsMutable);
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The starting index of the items being replaced.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            IList newItems,
            IList oldItems,
            int startingIndex)
            : this(action, newItems, oldItems)
        {
            NewStartingIndex = startingIndex;
            OldStartingIndex = startingIndex;
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a one-item Move event.
        /// </summary>
        /// <param name="action">Can only be a Move action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            object? changedItem,
            int index,
            int oldIndex)
            : this(action, changedItem)
        {
            NewStartingIndex = index;
            OldStartingIndex = oldIndex;
        }

        /// <summary>
        /// Initializes a new instance of the NotifyCollectionChangingEventArgs class
        /// that describes a multi-item Move event.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangingAction action,
            IList? changedItems,
            int index,
            int oldIndex)
            : this(action, changedItems)
        {
            NewStartingIndex = index;
            OldStartingIndex = oldIndex;
        }

        #region I M P L I C I T
        public static implicit operator NotifyCollectionChangedEventArgs(
    NotifyCollectionChangingEventArgs e)
        {
            return e.Action switch
            {
                NotifyCollectionChangingAction.Add =>
                    e.NewItems is not null && e.NewItems.Count > 1
                        ? new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            e.NewItems,
                            e.NewStartingIndex)
                        : new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            e.NewItems?[0],
                            e.NewStartingIndex),

                NotifyCollectionChangingAction.Remove =>
                    e.OldItems is not null && e.OldItems.Count > 1
                        ? new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            e.OldItems,
                            e.OldStartingIndex)
                        : new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            e.OldItems?[0],
                            e.OldStartingIndex),

                NotifyCollectionChangingAction.Replace =>
                    e.NewItems is not null && e.NewItems.Count > 1
                        ? new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            e.NewItems,
                            e.OldItems,
                            e.NewStartingIndex)
                        : new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            e.NewItems?[0],
                            e.OldItems?[0],
                            e.NewStartingIndex),

                NotifyCollectionChangingAction.Move =>
                    e.NewItems is not null && e.NewItems.Count > 1
                        ? new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Move,
                            e.NewItems,
                            e.NewStartingIndex,
                            e.OldStartingIndex)
                        : new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Move,
                            e.NewItems?[0],
                            e.NewStartingIndex,
                            e.OldStartingIndex),

                NotifyCollectionChangingAction.Reset =>
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static implicit operator NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedEventArgs e)
        {
            var action = (NotifyCollectionChangingAction)e.Action;

            return e.Action switch
            {
                NotifyCollectionChangedAction.Add =>
                    e.NewItems is not null
                        ? new NotifyCollectionChangingEventArgs(action, e.NewItems, e.NewStartingIndex)
                        : new NotifyCollectionChangingEventArgs(action, (object?)null, e.NewStartingIndex),

                NotifyCollectionChangedAction.Remove =>
                    e.OldItems is not null
                        ? new NotifyCollectionChangingEventArgs(action, e.OldItems, e.OldStartingIndex)
                        : new NotifyCollectionChangingEventArgs(action, (object?)null, e.OldStartingIndex),

                NotifyCollectionChangedAction.Replace =>
                    e.NewItems is not null && e.OldItems is not null
                        ? new NotifyCollectionChangingEventArgs(action, e.NewItems, e.OldItems, e.NewStartingIndex)
                        : new NotifyCollectionChangingEventArgs(action, e.NewItems?[0], e.OldItems?[0], e.NewStartingIndex),

                NotifyCollectionChangedAction.Move =>
                    e.NewItems is not null
                        ? new NotifyCollectionChangingEventArgs(action, e.NewItems, e.NewStartingIndex, e.OldStartingIndex)
                        : new NotifyCollectionChangingEventArgs(action, (object?)null, e.NewStartingIndex, e.OldStartingIndex),

                NotifyCollectionChangedAction.Reset =>
                    new NotifyCollectionChangingEventArgs(action),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
        #endregion I M P L I C I T
    }
}