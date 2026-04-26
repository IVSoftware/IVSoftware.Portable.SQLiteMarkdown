using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// Legacy change-action enum with BCL-aligned core values.
    /// </summary>
    /// <remarks>
    /// <see cref="Add"/>, <see cref="Remove"/>, <see cref="Replace"/>,
    /// <see cref="Move"/>, and <see cref="Reset"/> remain numerically aligned
    /// with <see cref="System.Collections.Specialized.NotifyCollectionChangedAction"/>.
    ///
    /// Use <see cref="IVSoftware.Portable.Collections.NotifyCollectionChangeAction"/>
    /// for the current generalized contract. Legacy-only extensions are
    /// <see cref="QueryResult"/>, <see cref="ApplyFilter"/>, and
    /// <see cref="RemoveFilter"/>.
    /// </remarks>
    [Flags]
    [Obsolete($"Backward compatibility only; use {nameof(IVSoftware.Portable.Collections.NotifyCollectionChangeAction)}")]
    public enum NotifyQueryFilterCollectionChangedAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset,

        /// <summary>
        /// These items (old and new) represent a new canonical recordset.
        /// </summary>
        QueryResult = 0x1000,

        /// <summary>
        /// These items (old and new) represent a narrower subset.
        /// </summary>
        ApplyFilter = NotifyCollectionChangeReason.ApplyFilter,

        /// <summary>
        /// These items (old and new) represent a wider subset.
        /// </summary>
        RemoveFilter = NotifyCollectionChangeReason.RemoveFilter,
    }

    /// <summary>
    /// Compatibility shim retaining the legacy SQLiteMarkdown type identity.
    /// </summary>
    /// <remarks>
    /// Strategy 3: the implementation now lives in the shared collections
    /// dependency, while this wrapper preserves the historical namespace and
    /// manifest-facing surface for compatibility checks and existing callers.
    /// </remarks>
    public class NotifyQueryFilterCollectionChangedEventArgs : IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs
    {
        public NotifyQueryFilterCollectionChangedEventArgs(
            NotifyQueryFilterCollectionChangedAction action,
            IList changedItems)
            : base(
                action: localToCanonicalAction(action),
                reason: localToCanonicalReason(action),
                newItems: localGetNewItems(action, changedItems),
                oldItems: localGetOldItems(action, changedItems)) { }

        public NotifyQueryFilterCollectionChangedEventArgs(
            NotifyCollectionChangingEventArgs ePre, 
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None, 
            NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly) : base(ePre, reason, scope) { }

        public NotifyQueryFilterCollectionChangedEventArgs(
            NotifyCollectionChangedEventArgs eBCL,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None,
            NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly)
            : base(eBCL, reason, scope) { }

        public NotifyQueryFilterCollectionChangedEventArgs(
            NotifyCollectionChangeAction action, 
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None, 
            NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly, IList? newItems = null, IList? oldItems = null, int newStartingIndex = -1, int oldStartingIndex = -1) 
            : base(action, reason, scope, newItems, oldItems, newStartingIndex, oldStartingIndex) { }

        public new NotifyQueryFilterCollectionChangedAction Action =>
            localToLegacyAction(base.Action, Reason);

        static NotifyCollectionChangeAction localToCanonicalAction(
            NotifyQueryFilterCollectionChangedAction action)
            => action switch
            {
                NotifyQueryFilterCollectionChangedAction.Add =>
                    NotifyCollectionChangeAction.Add,
                NotifyQueryFilterCollectionChangedAction.Remove =>
                    NotifyCollectionChangeAction.Remove,
                NotifyQueryFilterCollectionChangedAction.Replace =>
                    NotifyCollectionChangeAction.Replace,
                NotifyQueryFilterCollectionChangedAction.Move =>
                    NotifyCollectionChangeAction.Move,
                NotifyQueryFilterCollectionChangedAction.Reset =>
                    NotifyCollectionChangeAction.Reset,
                NotifyQueryFilterCollectionChangedAction.QueryResult =>
                    NotifyCollectionChangeAction.Reset,
                NotifyQueryFilterCollectionChangedAction.ApplyFilter =>
                    NotifyCollectionChangeAction.Digest,
                NotifyQueryFilterCollectionChangedAction.RemoveFilter =>
                    NotifyCollectionChangeAction.Digest,
                _ => NotifyCollectionChangeAction.Digest,
            };

        static NotifyCollectionChangeReason localToCanonicalReason(
            NotifyQueryFilterCollectionChangedAction action)
            => action switch
            {
                NotifyQueryFilterCollectionChangedAction.QueryResult =>
                    NotifyCollectionChangeReason.Digest,
                NotifyQueryFilterCollectionChangedAction.ApplyFilter =>
                    NotifyCollectionChangeReason.ApplyFilter,
                NotifyQueryFilterCollectionChangedAction.RemoveFilter =>
                    NotifyCollectionChangeReason.RemoveFilter,
                _ => NotifyCollectionChangeReason.None,
            };

        static IList? localGetNewItems(
            NotifyQueryFilterCollectionChangedAction action,
            IList changedItems)
            => action switch
            {
                NotifyQueryFilterCollectionChangedAction.Add =>
                    changedItems,
                NotifyQueryFilterCollectionChangedAction.Replace =>
                    changedItems,
                NotifyQueryFilterCollectionChangedAction.Move =>
                    changedItems,
                NotifyQueryFilterCollectionChangedAction.QueryResult =>
                    changedItems,
                NotifyQueryFilterCollectionChangedAction.ApplyFilter =>
                    changedItems,
                NotifyQueryFilterCollectionChangedAction.RemoveFilter =>
                    changedItems,
                _ => null,
            };

        static IList? localGetOldItems(
            NotifyQueryFilterCollectionChangedAction action,
            IList changedItems)
            => action switch
            {
                NotifyQueryFilterCollectionChangedAction.Remove =>
                    changedItems,
                NotifyQueryFilterCollectionChangedAction.Replace =>
                    changedItems,
                NotifyQueryFilterCollectionChangedAction.Move =>
                    changedItems,
                _ => null,
            };

        static NotifyQueryFilterCollectionChangedAction localToLegacyAction(
            NotifyCollectionChangeAction action,
            NotifyCollectionChangeReason reason)
            => action switch
            {
                NotifyCollectionChangeAction.Add =>
                    NotifyQueryFilterCollectionChangedAction.Add,
                NotifyCollectionChangeAction.Remove =>
                    NotifyQueryFilterCollectionChangedAction.Remove,
                NotifyCollectionChangeAction.Replace =>
                    NotifyQueryFilterCollectionChangedAction.Replace,
                NotifyCollectionChangeAction.Move =>
                    NotifyQueryFilterCollectionChangedAction.Move,
                NotifyCollectionChangeAction.Reset
                    when reason == NotifyCollectionChangeReason.Digest =>
                        NotifyQueryFilterCollectionChangedAction.QueryResult,
                NotifyCollectionChangeAction.Reset =>
                    NotifyQueryFilterCollectionChangedAction.Reset,
                NotifyCollectionChangeAction.Digest
                    when reason == NotifyCollectionChangeReason.ApplyFilter =>
                        NotifyQueryFilterCollectionChangedAction.ApplyFilter,
                NotifyCollectionChangeAction.Digest
                    when reason == NotifyCollectionChangeReason.RemoveFilter =>
                        NotifyQueryFilterCollectionChangedAction.RemoveFilter,
                NotifyCollectionChangeAction.Digest =>
                    NotifyQueryFilterCollectionChangedAction.QueryResult,
                _ => NotifyQueryFilterCollectionChangedAction.Reset,
            };
    }
}
