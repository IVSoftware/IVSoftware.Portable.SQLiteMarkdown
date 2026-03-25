using IVSoftware.Portable.SQLiteMarkdown.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// Specifies an action associated with either a Changed or Changing event.
    /// </summary>
    public enum NotifyCollectionChangeAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset
    }

    /// <summary>
    /// Reason for Changed or Changing extended actions.
    /// </summary>
    [Flags]
    public enum NotifyCollectionChangeReason
    {
        /// <summary>
        /// This is a pass-though BCL event structure.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// These items (old and new) represent a new canonical recordset.
        /// </summary>
        QueryResult = 0x1000,

        /// <summary>
        /// These items (old and new) represent a narrower subset.
        /// </summary>
        ApplyFilter = 0x2000,

        /// <summary>
        /// These items (old and new) represent a wider subset.
        /// </summary>
        RemoveFilter = 0x4000,

        /// <summary>
        /// These items (old and new) represent a deferred collection change digest.
        /// </summary>
        Batch = 0x8000,
    }

    [DebuggerDisplay("{Action}  OldCount={OldItems?.Count ?? 0}  NewCount={NewItems?.Count ?? 0}  OldIndex={OldStartingIndex}  NewIndex={NewStartingIndex}")]
    public class ModelSettledEventArgs : NotifyCollectionChangedEventArgs
    {
        public static ModelSettledEventArgs FromNotifyCollectionChangedEventArgs(
            NotifyCollectionChangeReason reason,
            NotifyCollectionChangedEventArgs e)
        {
            if(e.NewStartingIndex != -1 || e.OldStartingIndex != -1)
            {
                Debug.Fail($@"IFD ADVISORY - First Time TODO.");
            }

            return e.Action switch
            {
                NotifyCollectionChangedAction.Add =>
                    new ModelSettledEventArgs(
                        e.Action,
                        changedItems: e.NewItems ?? Array.Empty<object>(),
                        reason),

                NotifyCollectionChangedAction.Remove =>
                    new ModelSettledEventArgs(
                        e.Action,
                        changedItems: e.OldItems ?? Array.Empty<object>(),
                        reason),

                NotifyCollectionChangedAction.Replace =>
                    new ModelSettledEventArgs(
                        e.Action,
                        newItems: e.NewItems ?? Array.Empty<object>(),
                        oldItems: e.OldItems ?? Array.Empty<object>(),
                        reason),

                NotifyCollectionChangedAction.Move =>
                new ModelSettledEventArgs(
                        e.Action,
                        changedItems: e.NewItems ?? Array.Empty<object>(),
                        index: e.NewStartingIndex,
                        oldIndex: e.OldStartingIndex,
                        reason),

                NotifyCollectionChangedAction.Reset =>
                    new ModelSettledEventArgs(
                        e.Action,
                        reason),

                _ => throw new ArgumentOutOfRangeException(nameof(e.Action), e.Action, null)
            };
        }
        public NotifyCollectionChangeReason Reason { get; }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction())
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event; can only be Reset, Add or Remove action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action, object? changedItem, NotifyCollectionChangeReason reason) 
            : base(action.ToNotifyCollectionChangedAction(), changedItem)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action, object? changedItem, int index, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), changedItem, index)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action, IList? changedItems, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), changedItems)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change (or a reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action,IList? changedItems, int startingIndex, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), changedItems, startingIndex)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action,object? newItem, object? oldItem, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), newItem, oldItem)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action, object? newItem, object? oldItem, int index, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), newItem, oldItem, index)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action,IList newItems, IList oldItems, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), newItems, oldItems)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The starting index of the items being replaced.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action,IList newItems, IList oldItems, int startingIndex, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), newItems, oldItems, startingIndex)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Move event.
        /// </summary>
        /// <param name="action">Can only be a Move action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action,object? changedItem, int index, int oldIndex, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), changedItem, index, oldIndex)
        {
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Move event.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        public ModelSettledEventArgs(NotifyCollectionChangedAction action,IList? changedItems, int index, int oldIndex, NotifyCollectionChangeReason reason)
            : base(action.ToNotifyCollectionChangedAction(), changedItems, index, oldIndex)
        {
            Reason = reason;
        }
    }
}
