using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;

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
        /// Initializes a new instance describing a Reset change.
        /// </summary>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action)
        {
            Action = action;
        }

        /// <summary>
        /// Initializes a new instance describing a one-item change.
        /// </summary>
        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? changedItem)
            : this(action)
        {
            if (changedItem is not null)
            {
                if (action == NotifyCollectionChangeAction.Add)
                    NewItems = new object[] { changedItem };
                else
                    OldItems = new object[] { changedItem };
            }
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? changedItem,
            int index)
            : this(action, changedItem)
        {
            if (action == NotifyCollectionChangeAction.Add)
                NewStartingIndex = index;
            else
                OldStartingIndex = index;
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList? changedItems)
            : this(action)
        {
            if (action == NotifyCollectionChangeAction.Add)
                NewItems = changedItems;
            else
                OldItems = changedItems;
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList? changedItems,
            int startingIndex)
            : this(action, changedItems)
        {
            if (action == NotifyCollectionChangeAction.Add)
                NewStartingIndex = startingIndex;
            else
                OldStartingIndex = startingIndex;
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? newItem,
            object? oldItem)
            : this(action)
        {
            NewItems = newItem is null ? null : new object[] { newItem };
            OldItems = oldItem is null ? null : new object[] { oldItem };
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? newItem,
            object? oldItem,
            int index)
            : this(action, newItem, oldItem)
        {
            NewStartingIndex = index;
            OldStartingIndex = index;
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList newItems,
            IList oldItems)
            : this(action)
        {
            NewItems = newItems;
            OldItems = oldItems;
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList newItems,
            IList oldItems,
            int startingIndex)
            : this(action, newItems, oldItems)
        {
            NewStartingIndex = startingIndex;
            OldStartingIndex = startingIndex;
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            object? changedItem,
            int index,
            int oldIndex)
            : this(action, changedItem)
        {
            NewStartingIndex = index;
            OldStartingIndex = oldIndex;
        }

        public MutableNotifyCollectionChangingEventArgs(
            NotifyCollectionChangeAction action,
            IList? changedItems,
            int index,
            int oldIndex)
            : this(action, changedItems)
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