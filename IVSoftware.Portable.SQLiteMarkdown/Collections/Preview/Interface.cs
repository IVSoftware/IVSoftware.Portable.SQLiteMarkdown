using IVSoftware.Portable.Collections.Common;
using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.StateRunner.Preview;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace IVSoftware.Portable.Collections.Preview
{
    /// <summary>
    /// Defines a modeled contract for <see cref="INotifyCollectionChanged"/> 
    /// under ModelDataExchange authority.
    /// </summary>
    /// <remarks>
    /// Represents a collection whose change notifications are governed by MDX,
    /// where authority determines both:
    /// - The direction of updates (source vs. projection), and
    /// - Whether events are immediate or deferred.
    ///
    /// In deferred modes, intermediate changes may be accumulated and coalesced
    /// into either:
    /// - A single BCL-compatible event, or
    /// - A non-compatible Add event whose NewItems form an event playlist.
    /// </remarks>
    internal interface IModeledNotifyCollectionChanged : IList
    {
        /// <summary>
        /// Increments the ref count for the suppression epoch.
        /// </summary>
        /// <remarks>
        /// When the ref count returns to zero, disposal raises a final event
        /// with a coalesced <see cref="NotifyCollectionChangingEventArgs"/> instance.
        /// </remarks>
        IDisposable RequestModelEpochAuthority(ModelDataExchangeAuthority authority, IList source);

        /// <summary>
        /// Sets an internal flag indicating that the final emission for the current
        /// suppression epoch should include a void marker.
        /// </summary>
        /// <remarks>
        /// This method does not terminate the suppression scope or affect the reference
        /// count. Disposal proceeds normally via the <see cref="IDisposable"/> tokens
        /// returned by <see cref="RequestModelEpochAuthority"/>. Instead, it alters the semantics
        /// of the final emission, signaling that the coalesced result should be disregarded.
        /// </remarks>
        void CancelSuppress();

        /// <summary>
        /// Gets the current phase of the suppression epoch.
        /// </summary>
        /// <remarks>
        /// Indicates whether changes are being staged under suppression or the final
        /// coalesced result is being emitted.
        /// </remarks>
        ModelDataExchangeAuthority Phase { get; }

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

    internal interface IModeledNotifyCollectionChanged<T> 
        : IList<T>
        , IModeledNotifyCollectionChanged { } 

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
