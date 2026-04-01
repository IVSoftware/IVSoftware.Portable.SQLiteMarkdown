using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.StateRunner.Preview;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace IVSoftware.Portable.Collections.Preview
{
    /// <summary>
    /// Specifies an action associated with either a Changed or Changing event.
    /// </summary>
    internal enum NotifyCollectionChangeAction
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
    internal enum NotifyCollectionChangeReason
    {
        /// <summary>
        /// This is a pass-though BCL event structure.
        /// </summary>
        None = 0x0000,

#if false && RESERVED
        /// <summary>
        /// These items (old and new) represent a new canonical recordset.
        /// </summary>
        QueryResult = 0x1000,

        /// <summary>
        /// These items (old and new) represent a narrower subset.
        /// </summary>
        ApplyFilter = QueryResult << 1,
#else
        /// <summary>
        /// These items (old and new) represent a narrower subset.
        /// </summary>
        ApplyFilter = 0x2000,
#endif

        /// <summary>
        /// These items (old and new) represent a wider subset.
        /// </summary>
        RemoveFilter = ApplyFilter << 1,

        /// <summary>
        /// These items (old and new) represent a deferred collection change digest.
        /// </summary>
        Batch = RemoveFilter << 1,

        /// <summary>
        /// Indicates that a batch was canceled while it was building.
        /// </summary>
        Cancel = Batch << 1,

        /// <summary>
        /// Attributes a Reset action produced by an illegal configuration request.
        /// </summary>
        Exception = Cancel << 1,
    }

    /// <summary>
    /// Defines the extent to which a preview handler may interact with a pending
    /// collection change proposal.
    /// </summary>
    /// <remarks>
    /// This enumeration constrains what a handler is permitted to do during the
    /// preview (Changing) phase. It does not describe the change itself, but rather
    /// the allowed level of participation in shaping or rejecting it.
    ///
    /// - ReadOnly   : Observe only. No modification or cancellation is permitted.
    /// - CancelOnly : The proposal may be rejected but not altered.
    /// - FullControl: The proposal may be rewritten or rejected entirely.
    ///
    /// These flags are enforced by the preview pipeline. Handlers opting into
    /// higher scopes assume responsibility for producing a valid and internally
    /// consistent change contract.
    /// </remarks>
    [Flags]
    internal enum NotifyCollectionChangeScope
    {
        /// <summary>
        /// Observe the proposal without modifying or canceling it.
        /// </summary>
        ReadOnly = 0x0,

        /// <summary>
        /// Allows the proposal to be canceled but not modified.
        /// </summary>
        CancelOnly = 0x1,

        /// <summary>
        /// Allows full control over the proposal, including rewriting or canceling it.
        /// </summary>
        FullControl = 0x3,
    }

    internal interface INotifyCollectionChanging
    {
        public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;

        /// <summary>
        /// Defines the extent to which a preview handler may interact with a pending
        /// collection change proposal.
        /// </summary>
        /// <remarks>
        /// This enumeration constrains what a handler is permitted to do during the
        /// preview (Changing) phase. It does not describe the change itself, but rather
        /// the allowed level of participation in shaping or rejecting it.
        ///
        /// - ReadOnly   : Observe only. No modification or cancellation is permitted.
        /// - CancelOnly : The proposal may be rejected but not altered.
        /// - FullControl: The proposal may be rewritten or rejected entirely.
        ///
        /// These flags are enforced by the preview pipeline. Handlers opting into
        /// higher scopes assume responsibility for producing a valid and internally
        /// consistent change contract.
        /// </remarks>
        NotifyCollectionChangeScope EventScope { get; }
    }

    /// <summary>
    /// Represents the current phase of a suppression epoch for collection change notifications.
    /// </summary>
    /// <remarks>
    /// Given that the core overridable surface of ObservableCollection{T} is being
    /// invoked, this value indicates under what authority that is taking place.
    /// </remarks>
    [NotFlags]
    public enum SuppressionPhase
    {
        /// <summary>
        /// Suppression has not been requested; collection change notifications propagate normally.
        /// </summary>
        None = FsmReserved.NoAuthority,

        /// <summary>
        /// Suppression has been requested; and a preview event is under construction.
        /// </summary>
        /// <remarks>
        /// Typical Response: 
        /// - On detecting that an epoch has been released under Preview authority, the
        ///   consumer generally forwards a coalesced ledger so that can be variously
        ///   read, canceled and/or mutated <see cref="NotifyCollectionChangeScope"/>.
        /// </remarks>
        Preview,

        /// <summary>
        /// Suppression has been requested; and a preview event is being applied.
        /// </summary>
        /// <remarks>
        /// Typical Response: 
        /// - On detecting that an epoch has been released under Commit authority, the
        ///   consumer generally forwards a monolithic multi-change or Reset (BCL) event.
        /// </remarks>
        Commit,
    }

    /// <summary>
    /// Provides a suppression mechanism for <see cref="INotifyCollectionChanged"/> notifications.
    /// </summary>
    /// <remarks>
    /// Defines a scoped model for temporarily suppressing collection change notifications
    /// during coordinated or batched updates with the goal of reducing or eliminating churn.
    /// </remarks>
    internal interface INotifyCollectionChangedSuppressible
    {
        /// <summary>
        /// Increments the ref count for the suppression epoch.
        /// </summary>
        /// <remarks>
        /// When the ref count returns to zero, disposal raises a final event
        /// with a coalesced <see cref="NotifyCollectionChangingEventArgs"/> instance.
        /// </remarks>
        IDisposable BeginSuppressNotify(SuppressionPhase phase);

        /// <summary>
        /// Sets an internal flag indicating that the final emission for the current
        /// suppression epoch should include a void marker.
        /// </summary>
        /// <remarks>
        /// This method does not terminate the suppression scope or affect the reference
        /// count. Disposal proceeds normally via the <see cref="IDisposable"/> tokens
        /// returned by <see cref="BeginSuppressNotify"/>. Instead, it alters the semantics
        /// of the final emission, signaling that the coalesced result should be disregarded.
        /// </remarks>
        void CancelSuppressNotify();

        /// <summary>
        /// Gets the current phase of the suppression epoch.
        /// </summary>
        /// <remarks>
        /// Indicates whether changes are being staged under suppression or the final
        /// coalesced result is being emitted.
        /// </remarks>
        SuppressionPhase Phase { get; }
    }

    internal interface IRangeable
    {
        void AddRange(IEnumerable items);
        int AddRangeDistinct(IEnumerable items);
        void InsertRange(int startingIndex, IEnumerable items);
        void RemoveRange(int startingIndex, int endingIndex);
        int RemoveMultiple(IEnumerable items);
    }
    internal interface IRangeable<T> : IRangeable
    {
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Addin multiple items that are individually validated as distinct..
        /// </summary>
        int AddRangeDistinct(IEnumerable<T> items);

        /// <summary>
        /// Removal of a multiple contiguous items.
        /// </summary>
        void InsertRange(int startingIndex, IEnumerable<T> newItems);

        /// <summary>
        /// Removal of a multiple items that aren't necessarily contiguous.
        /// </summary>
        int RemoveMultiple(IEnumerable<T> items);
    }
}
