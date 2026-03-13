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
    public enum NotifyCollectionChangedReason
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
    public enum ModelSettledAction
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
        QueryResult = NotifyCollectionChangedReason.QueryResult,
        ApplyFilter = NotifyCollectionChangedReason.ApplyFilter,
        RemoveFilter = NotifyCollectionChangedReason.RemoveFilter,
        #endregion C O N T E X T
    }

    [DebuggerDisplay("{Action}  OldCount={OldItems?.Count ?? 0}  NewCount={NewItems?.Count ?? 0}  OldIndex={OldStartingIndex}  NewIndex={NewStartingIndex}")]
    public sealed class ModelSettledEventArgs : NotifyCollectionChangedEventArgs
    {
        public static ModelSettledEventArgs FromNotifyCollectionChangedEventArgs(
            NotifyCollectionChangedReason reason,
            NotifyCollectionChangedEventArgs e)
        {
            if(e.NewStartingIndex != -1 || e.OldStartingIndex != -1)
            {
                Debug.Fail($@"IFD ADVISORY - First Time TODO.");
            }
            var actionQF = (ModelSettledAction)((int)reason | (int)e.Action);

            return e.Action switch
            {
                NotifyCollectionChangedAction.Add =>
                    new ModelSettledEventArgs(
                        actionQF,
                        changedItems: e.NewItems ?? Array.Empty<object>()),

                NotifyCollectionChangedAction.Remove =>
                    new ModelSettledEventArgs(
                        actionQF,
                        changedItems: e.OldItems ?? Array.Empty<object>()),

                NotifyCollectionChangedAction.Replace =>
                    new ModelSettledEventArgs(
                        actionQF,
                        oldItems: e.OldItems ?? Array.Empty<object>(),
                        newItems: e.NewItems ?? Array.Empty<object>()),

                NotifyCollectionChangedAction.Move =>
                new ModelSettledEventArgs(
                    actionQF,
                    changedItems: e.NewItems ?? Array.Empty<object>(),
                    index: e.NewStartingIndex,
                    oldIndex: e.OldStartingIndex),

                NotifyCollectionChangedAction.Reset =>
                    new ModelSettledEventArgs(
                        action: actionQF),

                _ => throw new ArgumentOutOfRangeException(nameof(e.Action), e.Action, null)
            };
        }
        public new ModelSettledAction Action { get; }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public ModelSettledEventArgs(ModelSettledAction action)
            : base(action.ToNotifyCollectionChangedAction())
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event; can only be Reset, Add or Remove action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        public ModelSettledEventArgs(ModelSettledAction action, object? changedItem)
            : base(action.ToNotifyCollectionChangedAction(), changedItem)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public ModelSettledEventArgs(ModelSettledAction action, object? changedItem, int index)
            : base(action.ToNotifyCollectionChangedAction(), changedItem, index)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public ModelSettledEventArgs(ModelSettledAction action, IList? changedItems)
            : base(action.ToNotifyCollectionChangedAction(), changedItems)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change (or a reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public ModelSettledEventArgs(ModelSettledAction action, IList? changedItems, int startingIndex)
            : base(action.ToNotifyCollectionChangedAction(), changedItems, startingIndex)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public ModelSettledEventArgs(ModelSettledAction action, object? newItem, object? oldItem)
            : base(action.ToNotifyCollectionChangedAction(), newItem, oldItem)
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
        public ModelSettledEventArgs(ModelSettledAction action, object? newItem, object? oldItem, int index)
            : base(action.ToNotifyCollectionChangedAction(), newItem, oldItem, index)
        {
            Action = action;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public ModelSettledEventArgs(ModelSettledAction action, IList newItems, IList oldItems)
            : base(action.ToNotifyCollectionChangedAction(), newItems, oldItems)
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
        public ModelSettledEventArgs(ModelSettledAction action, IList newItems, IList oldItems, int startingIndex)
            : base(action.ToNotifyCollectionChangedAction(), newItems, oldItems, startingIndex)
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
        public ModelSettledEventArgs(ModelSettledAction action, object? changedItem, int index, int oldIndex)
            : base(action.ToNotifyCollectionChangedAction(), changedItem, index, oldIndex)
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
        public ModelSettledEventArgs(ModelSettledAction action, IList? changedItems, int index, int oldIndex)
            : base(action.ToNotifyCollectionChangedAction(), changedItems, index, oldIndex)
        {
            Action = action;
        }
    }
}
