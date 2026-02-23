using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Defines a self-indexing class that updates special, reserved property names within a database 
    /// or other keyed collection. This interface provides mechanisms for in-memory filtering, 
    /// partial and exact term matching, and efficient serialization.
    /// </summary>
    /// <remarks>
    /// - The implementing class should use the generic IVSoftware.Portable.SQLiteMarkdown.ParseSqlMarkdown
    ///   expression to parse and index terms.
    /// - A typical usage pattern includes a brief settling timer in an OnPropertyChanged override to update 
    ///   the indexed terms and the Values backing store.
    /// - Most queries operate on the exposed indexed values, reducing the need for each property to consume 
    ///   individual columns (e.g., in an SQLite schema).
    /// </remarks>
    public interface ISelfIndexedMarkdown : INotifyPropertyChanged
    {
        [PrimaryKey]
        string PrimaryKey { get; }

        /// <summary>
        /// - An aggregated indexed term for retrievals in the Query state. 
        /// - Use the [SelfIndexed] attribute to route a property to this index.
        /// - Properties can be [SQLite.Ignore] and still participate in this
        ///   index which is persisted in the database as a JSON blob.
        /// </summary>
        string QueryTerm { get; set; }

        /// <summary>
        /// - An aggregated indexed term for retrievals in the Filter state. 
        /// - Use the [SelfIndexed] attribute to route a property to this index.
        /// - Properties can be [SQLite.Ignore] and still participate in this
        ///   index which is persisted inthe database as a JSON blob.
        /// </summary>
        string FilterTerm { get; set; }

        /// <summary>
        /// Supports exact matching of terms enclosed in square brackets (e.g., "[tag]") for specific tags or values 
        /// during querying. The string is trimmed and any internal whitespace is normalized to a single space character.
        /// </summary>
        string TagMatchTerm { get; set; }

        /// <summary>
        /// Persisted JSON representation of the Values dictionary, stored for efficient access.
        /// </summary>
        string Properties
        {
            // When IsSerializationRequired is true due to changes in the dictionary, 
            // a private accessor re-serializes the dictionary to JSON to avoid circular references.
            get;
            set;
        }
    }

    public interface IObservableQueryFilterSource
        : IList
        , INotifyCollectionChanged
        , INotifyPropertyChanged
    {
        bool IsFiltering { get; }
        string InputText { get; set; }
        SearchEntryState SearchEntryState { get; }
        FilteringState FilteringState { get; }
        string Placeholder { get; }
        bool Busy { get; }
        QueryFilterConfig QueryFilterConfig { get; set; }
        string Title { get; set; }
        string SQL { get; }
        SQLiteConnection MemoryDatabase { get; set; }
        FilteringState Clear(bool all = false);
        void Commit();
        event EventHandler? InputTextSettled;

        event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;
    }
    public interface IObservableQueryFilterSource<T>
        : IObservableQueryFilterSource
    {
        void InitializeFilterOnlyMode(IEnumerable<T> items);
        void ReplaceItems(IEnumerable<T> items);
        Task ReplaceItemsAsync(IEnumerable<T> items);
        DisposableHost DHostBusy { get; }

#if false
        new IList<T> SelectedItems { get; }
        event EventHandler SelectionChanged;
#endif
    }

    /// <summary>
    /// Specifies the selection state of an item in a CollectionView. 
    /// This enumeration supports bitwise operations to allow combinations of selection states.
    /// </summary>
    [Flags]
    public enum ItemSelection
    {
        /// <summary>
        /// The item is not selected.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// The item is the only selection.
        /// This state cannot coexist with other states.
        /// </summary>
        Exclusive = 0x1,

        /// <summary>
        /// The item is one of multiple selected items.
        /// </summary>
        Multi = 0x2,

        /// <summary>
        /// The item is the most recently selected and is always part of a multi-selection.
        /// </summary>
        Primary = 0x6,
    }

    /// <summary>
    /// Describes how a collection or host control governs item selection behavior.
    /// </summary>
    /// <remarks>
    /// This enumeration defines the allowed interaction model for selection at the
    /// collection level. Individual items typically expose a <see cref="ISelectable"/>
    /// contract so their selection state can be coordinated with the active
    /// <see cref="SelectionMode"/>.
    /// </remarks>
    public enum SelectionMode
    {
        /// <summary>
        /// Selection not allowed.
        /// </summary>
        None,

        /// <summary>
        /// One-hot selection.
        /// </summary>
        Single,

        /// <summary>
        /// Allow modifiers like SHIFT and CONTROL
        /// </summary>
        SingleWithModifiers,

        /// <summary>
        /// Multiple selection allowed.
        /// </summary>
        Multiple
    }

    /// <summary>
    /// Defines item-level selection and editing semantics for participation in
    /// a host-controlled selection model.
    /// </summary>
    /// <remarks>
    /// <see cref="Selection"/> reflects the current selection state of the item
    /// as coordinated by the hosting collection and its active <see cref="SelectionMode"/>.
    /// 
    /// <see cref="IsEditing"/> acts as a semantic switch indicating that the item
    /// is currently in edit mode. This flag is intended for UI coordination
    /// (for example, toggling templates or enabling inline editors) and does not
    /// imply persistence, validation, or commit behavior.
    /// </remarks>
    public interface ISelectable
    {
        ItemSelection Selection { get; set; }
        bool IsEditing { get; set; }
    }

    /// <summary>
    /// Identifies an item that records its moment of origin.
    /// </summary>
    /// <remarks>
    /// <see cref="Created"/> represents the timestamp at which the item
    /// was first created. It is intended to remain stable and may be used
    /// for ordering or basic lifecycle analysis.
    /// 
    /// This contract establishes temporal origin only. Authorship,
    /// permissions, or external identity semantics (for example,
    /// Google Drive–style ownership models) are separate concerns.
    /// </remarks>
    public interface IGenesis
    {
        DateTimeOffset Created { get; }
    }

    /// <summary>
    /// Defines ledger-style custody tracking for local edits and remote receipts.
    /// </summary>
    /// <remarks>
    /// This contract models a lightweight chain-of-custody system in which
    /// edits and receipts are recorded as temporal events. It enables detection
    /// of overlapping or divergent epochs (for example, during synchronization)
    /// without enforcing a specific merge policy.
    /// 
    /// <see cref="CommitLocalEdit"/> records a locally authored modification
    /// and returns the timestamp assigned to that event.
    /// 
    /// <see cref="CommitRemoteReceipt"/> records acknowledgement or ingestion
    /// of a remotely authored change and returns a token describing the
    /// resulting custody state.
    /// 
    /// Conflict detection is supported, but conflict resolution is intentionally
    /// left to the consumer.
    /// </remarks>
    public interface IChainOfCustody
    {
        Task<DateTimeOffset> CommitLocalEdit(string identity);
        Task<ChainOfCustodyToken> CommitRemoteReceipt(string identity, DateTimeOffset remoteTimeStamp);
    }

    /// <summary>
    /// User area where value strings are indexed by key strings.
    /// Are the values JSON? XML? You tell me! 
    /// </summary>
    public interface ICustomProperties
    {
        IDictionary<string, string?> CustomProperties { get; }
    }

    /// <summary>
    /// Relational distinction that behaves differently for fixed and floating items.
    /// </summary>
    /// <remarks>
    /// AFFINITY FIELD
    /// - An AffinityField can be in an active Running state, i.e., "I'm working on it".
    /// - In UI terms, this is often expressed in Play-Pause visual states.
    /// 
    /// FIXED v FLOAT
    /// - First of all, an AFFINITY ITEM isn't required to be temporal, but if it opts in:
    /// - FLOAT refers to positioning relative to Now.
    /// - However, a root item can be designated as FIXED to a time, a date, or a fully-qualified DateTimeOffset.
    /// - FIXED items have an "available time before" dictated by the time-space continuum.
    /// - FIXED items have an "available time after" that is infinite, unless constrained by the Duration property.
    /// 
    /// FIXED Root Example - 'Available Time Before' is inherently bounded:
    /// - Available time compresses as the fixed time approaches, and child items above react.
    /// - When the affinity field is in a Running state, child items adjust remaining time accordingly.
    /// - However, in a !Running state, child items can fall into the past with unsatisfied remaining times.
    /// - In UI terms, past due affinity items are often flagged in red.
    /// 
    /// FIXED Root Example - When 'Available Time After' is explicitly bounded using Duration property.
    /// - This state enforces a deadline for the fixed epoch.
    /// - Child items below can now also react to Running.
    /// </remarks>
    public enum AffinityTimeDomain : byte
    {
        /// <summary>
        /// UtcEnd <= UtcNow
        /// </summary>
        Past,

        /// <summary>
        /// UtcStart <= UtcNow <= UtcEnd.
        /// </summary>
        Present,

        /// <summary>
        /// UtcStart > UtcNow
        /// </summary>
        Future,
    }

    /// <summary>
    /// Supports temporal semantics for AffinityItem.
    /// </summary>
    /// <remarks> 
    /// Typically, the property is nullable and represents an OPT-IN to temporal characteristics.
    /// </remarks>
    [Flags]
    public enum AffinityMode : byte
    {
        /// <summary>
        /// Begins as soon as possible relative to UtcNow and Position.
        /// </summary>
        Asap = 0x0,

        /// <summary>
        /// Begins daily at a specified UtcStart and ends Duration later.
        /// </summary>
        /// <remarks>
        /// The containing AFFINITY FIELD can be:
        /// - RUNNING where time compression shrinks both Duration and Remaining in
        ///   the sense that "things are going according to plan" and "we will
        ///   begin and end on time". Deadlines respected, promises kept.
        /// - !RUNNING where Remaining shrinks while the Duration (i.e., the time 
        ///   commitment) does not. Such items can fall into the red.
        /// </remarks>
        FixedTime = 0x1,

        /// <summary>
        /// Begins daily at zero hour and has no concept of Now.
        /// </summary>
        FixedDate = 0x2,

        /// <summary>
        /// You have an appointment.
        /// </summary>
        /// <remarks>
        /// Specifying the Duration is strictly optional.
        /// </remarks>
        FixedDateAndTime = FixedDate | FixedTime,
    }

    /// <summary>
    /// Conditional semantics that are mutually exclusive to FIXED.
    /// </summary>
    /// <remarks>
    /// Child items *are not* required to be temporal, but if they *are* then thay are ASAP and never FIXED.
    /// </remarks>
    public enum ChildAffinityMode : sbyte
    {
        /// <summary>
        /// Begins at a specified UtcStart and ends Remaining later.
        /// </summary>
        Above = -1,

        /// <summary>
        /// Begins as soon as possible relative to UtcNow and Position.
        /// </summary>
        Below = +1,
    }

    #region F I X E D    E P O C H
    /// <summary>
    /// Strategy options for Fixed + Duration field when early item completion creates a surplus.
    /// </summary>
    /// <remarks>
    /// Mental Model: In every scenario, "Topics A-E must be covered inside of TimeSpan T".
    /// </remarks>
    public enum FreeTimeMode
    {
        /// <summary>
        /// Use free time surplus now, within the Present affinity time domain.
        /// </summary>
        /// Mental Model: 
        /// - "We got through Topic A more quickly than expected."
        /// - "We will take a short break and come back on time for Topic B."
        /// </remarks>
        FreeTimeAtStart,

        /// <summary>
        /// Distribute free time surplus uniformly.
        /// </summary>
        /// Mental Model: 
        /// - "We got through Topic A more quickly than expected."
        /// - "We can slow the pace for B-E and, for example, allow more discussion."
        /// </remarks>
        FreeTimeDistributed,

        /// <summary>
        /// Use free time surplus at the end.
        /// </summary>
        /// Mental Model: 
        /// - "We can go home early."
        /// - "The room will be free between T1 and T2 where we had planned to use it."
        /// - Here, the distinction is Duration. Without it, tasks would take their course
        ///   and any surplus time would be yielded to the field. But having duration means,
        ///   colloquially speaking, that "the room was reserved" and finishing early doesn't
        ///   mean that the next session can start any earlier than scheduled.
        /// </remarks>
        FreeTimeAtEnd,
    }

    /// <summary>
    /// Strategy options for Fixed + Duration field that creates scarcity by Not running.
    /// </summary>
    /// <remarks>
    /// Mental Model: "There is less time than we planned to complete the promised deliverable."
    /// 
    /// - Items that are ChildAffinityMode.Below can feel the squeeze when AffinityParent has Duration.
    /// - When IsRunning (or "Play" in the UI) is active, it signals that everything is proceeding
    ///   according to plan; the Remaining time will be consumed creating no deficits. Conversely,
    ///   when !IsRunning (or "Pause" in the UI) this will categorically produce deficits as remaining 
    ///   time inevitably shrinks.
    /// </remarks>
    public enum ScarceTimeMode
    {
        /// <summary>
        /// Items at the head of the epoch are marked PastDue as remaining time shrinks.
        /// </summary>
        /// <remarks>
        /// This is the default, and behaves the same as ChildAffinityMode.Above.
        /// Mental Model: 
        /// - "In order to get to B-E, topic A might need to be cut short."
        /// </remarks>
        LossOccursAtStart,

        /// <summary>
        /// Items uniformly shrink as deadline approaches.
        /// </summary>
        /// Mental Model: 
        /// - "We got started late, and will have to move at a faster pace generally."
        /// </remarks>
        LossIsDistributed,

        /// <summary>
        /// Items at the tail of the epoch are marked PastDue as remaining time shrinks.
        /// </summary>
        /// Mental Model: 
        /// - "Topics are progressive; A must be covered, even if we can't get to E."
        /// </remarks>
        LossOccursAtEnd,
    }

    /// <summary>
    /// Strategy options for fixed epochs that overlap.
    /// </summary>
    public enum OverlapTimeMode
    {
        /// <summary>
        /// Displaced older items are marked PastDue as remaining time shrinks.
        /// </summary>
        /// Mental Model: 
        /// - Here, the PastDue flag is interpreted more broadly
        ///   as "This item, as it stands, is not going to get done."
        /// </remarks>
        FutureDisplacesPast,

        PastDisplacesFuture,
    }

    #endregion F I X E D    E P O C H

    /// <summary>
    /// Represents a time slice snapshot using a captured UtcEpochNow that preempts race conditions.
    /// </summary>
    public interface IAffinityItem
    {
        /// <summary>
        /// Globally unique identifier.
        /// </summary>
        /// <remarks>
        /// Typically, this refers to the primary key of the model.
        /// </remarks>
        string Id { get; set; }

        /// <summary>
        /// Defines a hierarchal position where user definse the policy.
        /// </summary>
        /// <remarks>
        /// TYPICAL POLICIES
        /// - Materialized Path Policy (preferred):
        ///     Forward-slash delimited values always end with Id 
        ///     where more memory allows fewer queries.
        /// - Adjacency List Policy:
        ///     Stored only parent Id (if any) and requires recursive
        ///     calls to compute full path.
        /// </remarks>
        string Path { get; set; }

        /// <summary>
        /// Sortable key that is typically based on DateTimeOffset.Ticks.
        /// </summary>
        /// <remarks>
        /// While not exactly a time and date quantity, consider
        /// that DateTime.Now.Ticks will place an item last, but
        /// also that one might insert Item B as "halfway between"
        /// Item A and Item C.
        /// </remarks>
        long Position { get; set; }

        /// <summary>
        /// Reference point for AffinityMode.Fixed.
        /// </summary>
        DateTimeOffset? UtcStart { get; set; }

        /// <summary>
        /// Represents the original time commitment associated with this item.
        /// </summary>
        /// <remarks>
        /// Duration is the promised allocation of effort or engagement,
        /// independent of current progress or temporal compression.
        /// 
        /// It does not shrink simply because wall-clock time advances.
        /// Instead, it reflects the contractual scope of the commitment,
        /// while Remaining reflects the balance due.
        /// 
        /// In constrained epochs, Duration may exceed Available time,
        /// indicating that the commitment can no longer be satisfied
        /// within the current window.
        /// </remarks>
        TimeSpan? Duration { get; set; }

        /// <summary>
        /// Represents the remaining balance due on this commitment.
        /// </summary>
        /// <remarks>
        /// Remaining reflects the unfulfilled portion of the original Duration.
        /// When the field is running, this value decreases as effort is applied.
        /// 
        /// It is independent of wall-clock passage when the field is paused.
        /// If Remaining exceeds Available time within a fixed epoch,
        /// the item is effectively past due.
        /// </remarks>
        TimeSpan? Remaining { get; set; }

        /// <summary>
        /// Unlike Duration (the commitment) and Remaining (the balance due),
        /// Available represents the constraint window imposed by fixed epochs
        /// within the field.
        /// </summary>
        /// <remarks>
        /// Free-floating items without a fixed ancestor have effectively
        /// unbounded Available time and therefore no scarcity.
        ///
        /// Items positioned Above a fixed root are constrained by the
        /// approach to its start and always live within a finite window.
        ///
        /// Items positioned Below a fixed root are unconstrained unless
        /// the root defines a bounded Duration, in which case scarcity
        /// is imposed by the closing interval.
        ///
        /// Duration is aspirational. Availability is (sometimes harsh) reality.
        /// </remarks>
        TimeSpan? Available { get; }

        /// <summary>
        /// UtcStart + UtcRemaining.
        /// </summary>
        DateTimeOffset? UtcEnd { get; }

        /// <summary>
        /// Math.Max(TimeSpan.Zero, Remaining) == TimeSpan.Zero 
        /// </summary>
        bool? IsDone { get; }

        /// <summary>
        /// Derived mode when UtcStart and/or Duration are not null.
        /// </summary>
        AffinityMode? AffinityMode { get; }

        /// <summary>
        /// Reference PK for AffinityMode.Before and AffinityMode.After
        /// </summary>
        string? AffinityParent { get; }

        /// <summary>
        /// Derived mode when UtcParent is not null.
        /// </summary>
        ChildAffinityMode? AffinityChildMode { get; set; }

        /// <summary>
        /// Aspirational mode that requires UtcStart
        /// </summary>
        AffinityTimeDomain? AffinityTimeDomain { get; }

        /// <summary>
        /// Returns false if !(AffinityMode.ASAP). 'Net True' decrements Remaining when item is AffinityTimeDomain.Present.
        /// </summary>
        bool? IsRunning { get; }

        /// <summary>
        /// Partial epochs that float around fixed epochs.
        /// </summary>
        List<AffinitySlot> Slots { get; }
    }

    /// <summary>
    /// Quantizes DateTimeOffset and emits pulses in an intentionally lossy manner.
    /// </summary>
    /// <remarks>
    /// Allows IAffinityField to operate without race conditions.
    /// TYPICAL POLICY:
    /// - Start signal is issued on the UI thread and modifies an interlocked 'run' value.
    /// - Sub-second intervals raise UtcEpochNow
    /// </remarks>
    public interface IAffinitySliceEmitter : INotifyPropertyChanged
    {
        /// <summary>
        /// Captured epoch reference.
        /// </summary>
        DateTimeOffset AffinityEpochTimeSource { get; set; }

        /// <summary>
        /// Fast twitch display time.
        /// </summary>
        /// <remarks>
        /// TYPICAL POLICY:
        /// - Intended for minimal workload (e.g., update a single clock time display string).
        /// - Be mindful of the synchronous UI-thread load on this property.
        /// </remarks>
        DateTimeOffset DisplayTime { get; set; }

        int Second { get; }
        int Minute { get; }
        int Hour { get; }
        int Day { get; }
        int Month { get; }
        int Year { get; }
    }

    /// <summary>
    /// Represents a temporal constraint field responsible for interpreting
    /// scarcity, compression, and constraint inheritance across Affinity items.
    /// </summary>
    /// <remarks>
    /// The field is insulated from direct exposure to the wall clock and listens
    /// for quantized pulses from IAffinitySliceEmitter. These pulses are advisory
    /// and discardable.
    ///
    /// The field attempts to reconcile each injected snapshot coherently. If it
    /// cannot keep pace with the emitter, intermediate pulses are ignored rather
    /// than queued. Time does not accumulate; only the most recent snapshot is
    /// authoritative.
    ///
    /// The snapshot is guaranteed to be internally consistent, though in rare
    /// cases it may lag behind real time. When scarcity requires allocation among 
    /// competing items, resolution strategies are deterministic, idempotent, and 
    /// applied only through explicit, opt-in arbitration policies.
    /// </remarks>
    public interface IAffinityField : INotifyPropertyChanged
    {
        /// <summary>
        /// Colloquially speaking, this is another way of saying "I'm Working On It!"
        /// </summary>
        /// <remarks>
        /// RUNNING:
        /// This represents an aspirational approximation, that every second of effort
        /// reduces the "balance due" on the "commitment" by the same amount.
        /// 
        /// NOT RUNNING (unconstrained):
        /// In contrast, every second that transpires pushes back the delivery time.
        /// 
        /// NOT RUNNING (constrained):
        /// Fixed items in the field can impose an availability vector, making it
        /// possible for an item to "go into the red" in terms of a shortfall of
        /// what can be delivered compared to what was promised
        /// </remarks>
        bool IsRunning { get; set; }

        /// <summary>
        /// Canonical receptor of time emissions from arbitrary sources.
        /// </summary>
        /// <remarks>
        /// TYPICAL:
        /// - A periodic emitter is running.
        /// - @This is listening and injects those time updates into this property.
        /// - When changes are detected, if we're done with the last tick, then we process the new tick.
        /// COMMON:
        /// - A periodic emitter is present but stopped.
        /// - A unit test injects known time constants for testing.
        /// </remarks>
        DateTimeOffset AffinityEpochTimeSink { get; set; }


        /// <summary>
        /// Customizable finite state machine definition.
        /// </summary>
        Enum AffinityFSM { get; }
    }

    public enum AffinityStateMachine
    {
        /// <summary>
        /// Root affinities that are designated AffinityMode? > 0
        /// </summary>
        PlaceFixed,

        /// <summary>
        /// Calculate child affinities to N depth.
        /// </summary>
        /// <remarks>
        /// When scarcity is present due to a duration specification,
        /// the child affinities must be compressed in this state.
        /// </remarks>
        PlaceChildAffinitiesBelow,
    }
}