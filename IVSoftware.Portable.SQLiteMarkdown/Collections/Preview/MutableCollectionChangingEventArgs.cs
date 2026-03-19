using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;
using IVSoftware.Portable.SQLiteMarkdown.Collections;

namespace IVSoftware.Portable.Collections
{
    /// <summary>
    /// Represents a mutable, pre-commit collection mutation descriptor used during
    /// negotiation of a collection change.
    /// </summary>
    /// <remarks>
    /// This type is intentionally <b>not</b> a passive event snapshot. It is a live,
    /// rewriteable contract that participates in the mutation pipeline. Handlers
    /// receiving an instance may modify any aspect of the pending change, including
    /// <see cref="Action"/>, item payloads, and indices, or cancel the operation
    /// entirely via <see cref="CancelEventArgs.Cancel"/>.
    ///
    /// <para>
    /// <b>WARNING:</b> No invariants are enforced during mutation. Intermediate
    /// states may be incomplete or inconsistent (for example, an <c>Add</c> action
    /// with <c>OldItems</c> populated). This is by design to allow mid-flight
    /// rewrites.
    /// </para>
    ///
    /// <para>
    /// The responsibility for producing a valid and coherent change rests entirely
    /// with the final state of this instance at the point of projection to
    /// <see cref="NotifyCollectionChangedEventArgs"/>. Invalid final states may
    /// result in exceptions or undefined behavior during commit.
    /// </para>
    ///
    /// <para>
    /// This type should be treated as a transactional mutation descriptor:
    /// <list type="bullet">
    /// <item>Mutable during negotiation</item>
    /// <item>Interpreted once at commit</item>
    /// <item>Not safe for reuse or caching</item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class MutableNotifyCollectionChangingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets or sets the action describing the intended mutation.
        /// </summary>
        /// <remarks>
        /// May be reassigned during negotiation. Changing the action does not
        /// automatically reconcile related fields; callers must ensure that
        /// <see cref="NewItems"/>, <see cref="OldItems"/>, and indices are consistent
        /// before commit.
        /// </remarks>
        public NotifyCollectionChangeAction Action { get; set; }
        public NotifyCollectionChangeReason Reason { get; }

        /// <summary>
        /// Gets or sets the items being introduced by the mutation.
        /// </summary>
        /// <remarks>
        /// Typically used for <c>Add</c>, <c>Replace</c>, and <c>Move</c> actions.
        /// May be reassigned or repurposed during negotiation.
        /// </remarks>
        public IList? NewItems { get; set; }

        /// <summary>
        /// Gets or sets the items being removed or replaced by the mutation.
        /// </summary>
        /// <remarks>
        /// Typically used for <c>Remove</c>, <c>Replace</c>, and <c>Move</c> actions.
        /// May be reassigned or repurposed during negotiation.
        /// </remarks>
        public IList? OldItems { get; set; }

        /// <summary>
        /// Gets or sets the starting index for <see cref="NewItems"/>.
        /// </summary>
        /// <remarks>
        /// Interpretation depends on <see cref="Action"/>. May be modified during
        /// negotiation. A value of -1 indicates an unspecified index.
        /// </remarks>
        public int NewStartingIndex { get; set; }

        /// <summary>
        /// Gets or sets the starting index for <see cref="OldItems"/>.
        /// </summary>
        /// <remarks>
        /// Interpretation depends on <see cref="Action"/>. May be modified during
        /// negotiation. A value of -1 indicates an unspecified index.
        /// </remarks>
        public int OldStartingIndex { get; set; }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a reset change.
        /// </summary>
        /// <param name="action">The action that caused the event (must be Reset).</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            Action = action;
            Reason = reason;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event; can only be Reset, Add or Remove action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? changedItem,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, reason)
        {
            if (changedItem is not null)
            {
                if (action == NotifyCollectionChangeAction.Add)
                    NewItems = new object[] { changedItem };
                else
                    OldItems = new object[] { changedItem };
            }
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? changedItem,
            int index,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, changedItem, reason)
        {
            if (action == NotifyCollectionChangeAction.Add)
                NewStartingIndex = index;
            else
                OldStartingIndex = index;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList? changedItems,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, reason)
        {
            if (action == NotifyCollectionChangeAction.Add)
                NewItems = changedItems;
            else
                OldItems = changedItems;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item change (or a reset).
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList? changedItems,
            int startingIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, changedItems, reason)
        {
            if (action == NotifyCollectionChangeAction.Add)
                NewStartingIndex = startingIndex;
            else
                OldStartingIndex = startingIndex;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? newItem,
            object? oldItem,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, reason)
        {
            NewItems = newItem is null ? null : new object[] { newItem };
            OldItems = oldItem is null ? null : new object[] { oldItem };
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItem">The new item replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? newItem,
            object? oldItem,
            int index,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, newItem, oldItem, reason)
        {
            NewStartingIndex = index;
            OldStartingIndex = index;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList newItems,
            IList oldItems,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, reason)
        {
            NewItems = newItems;
            OldItems = oldItems;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Replace event.
        /// </summary>
        /// <param name="action">Can only be a Replace action.</param>
        /// <param name="newItems">The new items replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The starting index of the items being replaced.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList newItems,
            IList oldItems,
            int startingIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, newItems, oldItems, reason)
        {
            NewStartingIndex = startingIndex;
            OldStartingIndex = startingIndex;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a one-item Move event.
        /// </summary>
        /// <param name="action">Can only be a Move action.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? changedItem,
            int index,
            int oldIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, changedItem, reason)
        {
            NewStartingIndex = index;
            OldStartingIndex = oldIndex;
        }

        /// <summary>
        /// Construct a NotifyCollectionChangedEventArgs that describes a multi-item Move event.
        /// </summary>
        /// <param name="action">The action that caused the event.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList? changedItems,
            int index,
            int oldIndex,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
            : this(action, changedItems, reason)
        {
            NewStartingIndex = index;
            OldStartingIndex = oldIndex;
        }

        #region I M P L I C I T

        public static implicit operator NotifyCollectionChangedEventArgs(MutableNotifyCollectionChangingEventArgs e)
        {
            return e.Action switch
            {
                NotifyCollectionChangeAction.Add =>
                    e.NewItems is not null && e.NewItems.Count > 1
                        ? new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            e.NewItems,
                            e.NewStartingIndex)
                        : new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            e.NewItems?[0],
                            e.NewStartingIndex),

                NotifyCollectionChangeAction.Remove =>
                    e.OldItems is not null && e.OldItems.Count > 1
                        ? new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            e.OldItems,
                            e.OldStartingIndex)
                        : new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            e.OldItems?[0],
                            e.OldStartingIndex),

                NotifyCollectionChangeAction.Replace =>
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

                NotifyCollectionChangeAction.Move =>
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

                NotifyCollectionChangeAction.Reset =>
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static implicit operator MutableNotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs e)
        {
            var action = (NotifyCollectionChangeAction)e.Action;

            return e.Action switch
            {
                NotifyCollectionChangedAction.Add =>
                    e.NewItems is not null
                        ? new MutableNotifyCollectionChangingEventArgs(action, e.NewItems, e.NewStartingIndex)
                        : new MutableNotifyCollectionChangingEventArgs(action, (object?)null, e.NewStartingIndex),

                NotifyCollectionChangedAction.Remove =>
                    e.OldItems is not null
                        ? new MutableNotifyCollectionChangingEventArgs(action, e.OldItems, e.OldStartingIndex)
                        : new MutableNotifyCollectionChangingEventArgs(action, (object?)null, e.OldStartingIndex),

                NotifyCollectionChangedAction.Replace =>
                    e.NewItems is not null && e.OldItems is not null
                        ? new MutableNotifyCollectionChangingEventArgs(action, e.NewItems, e.OldItems, e.NewStartingIndex)
                        : new MutableNotifyCollectionChangingEventArgs(action, e.NewItems?[0], e.OldItems?[0], e.NewStartingIndex),

                NotifyCollectionChangedAction.Move =>
                    e.NewItems is not null
                        ? new MutableNotifyCollectionChangingEventArgs(action, e.NewItems, e.NewStartingIndex, e.OldStartingIndex)
                        : new MutableNotifyCollectionChangingEventArgs(action, (object?)null, e.NewStartingIndex, e.OldStartingIndex),

                NotifyCollectionChangedAction.Reset =>
                    new MutableNotifyCollectionChangingEventArgs(action),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion
    }
}