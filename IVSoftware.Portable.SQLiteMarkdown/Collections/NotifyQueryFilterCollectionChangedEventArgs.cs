using IVSoftware.Portable.SQLiteMarkdown.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    [Flags]
    public enum NotifyCollectionChangedQueryFilterFlags
    {
        #region C O N T E X T
        /// <summary>
        /// The changes are taking place inside a QueryResult action
        /// </summary>
        QueryResult = 0x1000,
        ApplyFilter = 0x2000,
        RemoveFilter = 0x4000,
        #endregion C O N T E X T
    }
    [Flags]
    public enum NotifyQueryFilterCollectionChangedAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset,

        #region C O N T E X T
        /// <summary>
        /// The changes are taking place inside a QueryResult action
        /// </summary>
        QueryResult = NotifyCollectionChangedQueryFilterFlags.QueryResult,
        ApplyFilter = NotifyCollectionChangedQueryFilterFlags.ApplyFilter,
        RemoveFilter = NotifyCollectionChangedQueryFilterFlags.RemoveFilter,
        #endregion C O N T E X T
    }

    public sealed class NotifyQueryFilterCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
    {
        public static NotifyQueryFilterCollectionChangedEventArgs FromNotifyCollectionChangedEventArgs(
            NotifyCollectionChangedQueryFilterFlags extended,
            NotifyCollectionChangedEventArgs e)
        {
            if(e.NewStartingIndex != -1 || e.OldStartingIndex != -1)
            {
                Debug.Fail($@"IFD ADVISORY - First Time TODO.");
            }
            var actionQF = (NotifyQueryFilterCollectionChangedAction)((int)extended | (int)e.Action);

            return e.Action switch
            {
                NotifyCollectionChangedAction.Add =>
                    new NotifyQueryFilterCollectionChangedEventArgs(
                        actionQF,
                        changedItems: e.NewItems ?? Array.Empty<object>()),

                NotifyCollectionChangedAction.Remove =>
                    new NotifyQueryFilterCollectionChangedEventArgs(
                        actionQF,
                        changedItems: e.OldItems ?? Array.Empty<object>()),

                NotifyCollectionChangedAction.Replace =>
                    new NotifyQueryFilterCollectionChangedEventArgs(
                        actionQF,
                        oldItems: e.OldItems ?? Array.Empty<object>(),
                        newItems: e.NewItems ?? Array.Empty<object>()),

                NotifyCollectionChangedAction.Move =>
                new NotifyQueryFilterCollectionChangedEventArgs(
                    actionQF,
                    changedItems: e.NewItems ?? Array.Empty<object>(),
                    index: e.NewStartingIndex,
                    oldIndex: e.OldStartingIndex),

                NotifyCollectionChangedAction.Reset =>
                    new NotifyQueryFilterCollectionChangedEventArgs(
                        action: actionQF),

                _ => throw new ArgumentOutOfRangeException(nameof(e.Action), e.Action, null)
            };
        }
        public new NotifyQueryFilterCollectionChangedAction Action { get; }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action)
            : base(action.ToBaseAction<NotifyCollectionChangedAction>())
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event; can only be Reset, Add or Remove action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, object? changedItem)
            : base(action.ToBaseAction(), changedItem)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, object? changedItem, int index)
            : base(action.ToBaseAction(), changedItem, index)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList? changedItems)
            : base(action.ToBaseAction(), changedItems)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change (or a reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList? changedItems, int startingIndex)
            : base(action.ToBaseAction(), changedItems, startingIndex)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, object? newItem, object? oldItem)
            : base(action.ToBaseAction(), newItem, oldItem)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, object? newItem, object? oldItem, int index)
            : base(action.ToBaseAction(), newItem, oldItem, index)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList newItems, IList oldItems)
            : base(action.ToBaseAction(), newItems, oldItems)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The starting index of the items being replaced.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
            : base(action.ToBaseAction(), newItems, oldItems, startingIndex)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Move event.
        /// </summary>
        /// <param name="action">Can only be a Move action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, object? changedItem, int index, int oldIndex)
            : base(action.ToBaseAction(), changedItem, index, oldIndex)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Move event.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList? changedItems, int index, int oldIndex)
            : base(action.ToBaseAction(), changedItems, index, oldIndex)
        {
            Action = action;
        }
    }
}
