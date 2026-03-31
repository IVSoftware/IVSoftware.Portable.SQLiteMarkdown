using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
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
        ApplyFilter = QueryResult << 1,

        /// <summary>
        /// These items (old and new) represent a wider subset.
        /// </summary>
        RemoveFilter = ApplyFilter << 1,

        /// <summary>
        /// These items (old and new) represent a deferred collection change digest.
        /// </summary>
        Batch = RemoveFilter << 1,

        /// <summary>
        /// Attributes a Reset action produced by an illegal configuration request.
        /// </summary>
        Exception = Batch << 1,
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
    public enum NotifyCollectionChangeScope
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

        event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;
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
