using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ephemeral = SQLite.IgnoreAttribute;

[assembly: InternalsVisibleTo("IVSoftware.Portable.SQLiteMarkdown.MSTest, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e1f164d857333dfcf4553776c3113969fc04991b6e0d72a969cd4c2d53341fd200e8c850935fd1e11adf7eac3fd9d50de3198aebbe2b6486c72ca205603fd12dc794bbd315e404e3d1f2e1256895a5e9739006d1f69b45de219c738f0c70cd0d6de5cff93f31e361907c09a653c584a51b9ff5201cde3c7ae681c16caa4579ce")]
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
        /// Engine-managed JSON persistence envelope.
        /// </summary>
        /// <remarks>
        /// Serialized projection of properties decorated with [SelfIndexed]
        /// where PersistenceMode includes Json. This column exists to support
        /// indexing and search (e.g. json_extract) and is not a general-purpose
        /// user metadata store.
        /// </remarks>
        string Properties
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a bindable collection governed by Markdown-based query and filter semantics.
    /// </summary>
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

    /// <summary>
    /// Strongly typed variant of IObservableQueryFilterSource.
    /// </summary>
    public interface IObservableQueryFilterSource<T>
        : IObservableQueryFilterSource
    {
        void InitializeFilterOnlyMode(IEnumerable<T> items);
        void ReplaceItems(IEnumerable<T> items);
        Task ReplaceItemsAsync(IEnumerable<T> items);
        DisposableHost DHostBusy { get; }
    }

    /// <summary>
    /// Lightweight mapper that eliminates guesswork e.g. as
    /// to how the `Selection` property contributes to filter.
    /// </summary>
    public enum StdPredicate
    {
        /// <summary>
        /// Selected items without regard to e.g. Primary, Multi, Exclusive etc.
        /// </summary>
        [Where("Selection", WherePredicate.IsNotZero)]
        IsSelected,

        /// <summary>
        /// Items that are affirmatively checked. As in "show only the items that are checked".
        /// </summary>
        [Where("IsChecked", WherePredicate.IsTrue)]
        IsChecked,

        /// <summary>
        /// Items that are affirmatively unchecked. As in "show only the items that are unchecked".
        /// </summary>
        [Where("IsChecked", WherePredicate.IsFalse)]
        IsUnchecked,
    }

    /// <summary>
    /// MarkdownContext role for implementation by composition.
    /// </summary>
    /// <remarks>
    /// Epistemically:
    /// - State can be managed even though the 'query' database is unknown.
    /// - Filtering is different. Items of any type can be set as the canonical
    ///   unfiltered source. Using markdown semantics, an internal sqlite database
    ///   it typically wired to produce PKs that match the query (reusing the 
    ///   actual references of the canonical list). When a filtered collection
    ///   is modified by UI interaction, the canonical unfiltered items must track.
    /// </remarks>
    [Probationary("Maintain as Internal until stable.")]
    internal interface IMarkdownContext
    {
        XElement Model { get; }
        uint DefaultLimit { get; set; }

        /// <summary>
        /// Bindable property that notifies when the "net filtered" list
        /// departs from sequence equals, based on primary keys.
        /// </summary>
        bool IsFiltering { get; }

        /// <summary>
        /// Nuanced state that takes InputText length into account.
        /// </summary>
        FilteringState FilteringState { get; }

        /// <remarks>
        /// As a defining feature, the Clear method is a progressive state 
        /// demotion. An actively filtering collection UI will take:
        /// [X] to clear the filter term IME while armed for a new filter term.
        /// [X] to return to query state, leaving the list items (if any) populated.
        /// [X] to clear the visible list, ready for a new query.
        /// </remarks>
        FilteringState Clear(bool all = false);

        string InputText { get; set; }
        QueryFilterConfig QueryFilterConfig { get; set; }
        SearchEntryState SearchEntryState { get; }

        event EventHandler? InputTextSettled;

        string ParseSqlMarkdown();
        string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast);
        string ParseSqlMarkdown<T>();
        string ParseSqlMarkdown<T>(string expr, QueryFilterMode qfMode = QueryFilterMode.Query);

        /// <summary>
        /// Initializes the canonical unfiltered collection.
        /// </summary>
        /// <remarks>
        /// This property is not intended for binding; this is enforced as set only 
        /// and represents a stateful and semantically meaningful replacement.
        /// </remarks>
        IEnumerable Recordset { set; }

        /// <summary>
        /// Represents an observable collection representing 'net visible' filtered items.
        /// </summary>
        /// <remarks>
        /// This property is not intended for binding; this is enforced as 
        /// set only and will be detached if set to null..
        /// </remarks>
        INotifyCollectionChanged ObservableProjection { set; }

        /// <summary>
        /// Guards receptivity of the unfiltered items collection.
        /// </summary>
        IDisposable BeginUIAction();

        int UnfilteredCount { get; }
    }

    /// <summary>
    /// Extends MarkdownContext with a predicate AND clause that is property-based.
    /// </summary>
    /// <remarks>
    /// Query and filter modes are equally affected.
    /// EXAMPLE:
    /// Activating this StdPredicate value will hide checked items in queries 
    /// and filters, and checking the item in the UI will filter that item out.
    /// <c>
    /// [Where("IsChecked", WherePredicate.IsFalse)]
    /// IsUnchecked,
    /// </c>
    /// </remarks>
    [Probationary("Maintain as Internal until stable.")]
    internal interface IPredicateMarkdownContext : IMarkdownContext
    {
        /// <summary>
        /// Obtain a token that suspends updates.
        /// </summary>
        /// <remarks>
        /// Avoids intermediate transitions when multiple predicates change state 
        /// simultaneously (e.g., ShowChecked v ShowUnchecked radio buttons).
        /// </remarks>
        IDisposable BeginPredicateAtom();

        IReadOnlyDictionary<string, Enum> ActiveFilters { get; }

        void ActivatePredicates(Enum stdPredicate, params Enum[] more);

        void DeactivatePredicates(Enum stdPredicate, params Enum[] more);

        void ClearPredicates(bool clearInputText = true);
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
    internal interface IChainOfCustody
    {
        DateTimeOffset Created { get; }
        Task<DateTimeOffset> CommitLocalEdit(string identity);
        Task<ChainOfCustodyToken> CommitRemoteReceipt(string identity, DateTimeOffset remoteTimeStamp);
    }

    /// <summary>
    /// Json property bag for user-defined value;
    /// </summary>
    internal interface ICustomProperties
    {
        IDictionary<string, string> CustomProperties { get; }
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
    internal enum AffinityTimeDomain : byte
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
    internal enum TemporalAffinity : byte
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
    internal enum ChildAffinityMode : sbyte
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
    internal enum FreeTimeMode
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
    /// Mental Model:
    /// "There is less time than we planned to complete the promised deliverable."
    /// 
    /// - Items that are ChildAffinityMode.Below can feel the squeeze when AffinityParent has Duration.
    /// - When IsRunning (or "Play" in the UI) is active, it signals that everything is proceeding
    ///   according to plan; the Remaining time will be consumed creating no deficits. Conversely,
    ///   when !IsRunning (or "Pause" in the UI) this will categorically produce deficits as remaining 
    ///   time inevitably shrinks.
    /// </remarks>
    internal enum ScarceTimeMode
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
    /// <remarks>
    /// Mental Model: 
    /// - Here, the PastDue flag is interpreted more broadly: "This item, as it stands, is simply not going to get done."
    /// - Typically, a UI will make this item "go into the red" with an appropriate visual state.
    /// Remember: 
    /// - Most collections are populated by queries, and overlaps can occur when a well-meaning 
    ///   query fails to return the conflicting epoch. It falls to the user implementation, to
    ///   manage this, and often involves the ability to perform a DateTimeRange x Resource matrix query.
    /// </remarks>
    internal enum OverlapTimeMode
    {
        /// <summary>
        /// Displaced older items are marked PastDue as remaining time shrinks.
        /// </summary>
        /// <remarks>
        /// Mental Model: 
        /// "We simply must move onto B, leaving A in its present state of (e.g., partial) completion."
        /// </remarks>
        FutureDisplacesPast,

        /// <summary>
        /// Displaced newer items are marked PastDue as remaining time shrinks.
        /// </summary>
        /// <remarks>
        /// Mental Model: 
        /// "We simply cannot move onto B without first completing A."
        /// </remarks>
        PastDisplacesFuture,
    }
    #endregion F I X E D    E P O C H

    internal enum AffinityRole
    {
        Prev,
    }

    /// <summary>
    /// Represents a prioritized node whose relational context ("affinity") is 
    /// implicitly restored when materialized from a query.
    /// </summary>
    /// <remarks>
    /// EXAMPLE:
    /// If the raw recordset of a query returns ONE child item at depth = 2, then the
    /// net query returns THREE. The UI now has greater opportunity to display context.
    /// </remarks>
    internal interface IPrioritizedAffinity
    {
        /// <summary>
        /// Provides access to the hierarchical structure and hosts lateral expansion.
        /// </summary>
        /// <remarks>
        /// XBoundObject is used extensively to attach service objects to XAttributes of this node.
        /// </remarks>
        XElement XAF { get; } // Xml affinity model.

        /// <summary>
        /// Globally unique identifier.
        /// </summary>
        /// <remarks>
        /// Typically, this refers to the primary key of the model.
        /// </remarks>
        string Id { get; set; }

        /// <summary>
        /// Adjacency List Policy defines a hierarchal position.
        /// </summary>
        string ParentId { get; }

        /// <summary>
        /// Materialized Path Policy defines a hierarchal position.
        /// </summary>
        public string ParentPath { get; set; }

        /// <summary>
        /// Linted concatenation of ParentPath and Id
        /// </summary>
        [Ephemeral]
        string FullPath { get; }

        [Ephemeral]
        bool IsRoot { get; }

        /// <summary>
        /// Sortable key that is typically based on DateTimeOffset.Ticks.
        /// </summary>
        /// <remarks>
        /// While not exactly a time and date quantity, consider
        /// that DateTime.Now.Ticks will place an item last, but
        /// also that one might insert Item B as "halfway between"
        /// Item A and Item C.
        /// </remarks>
        long Priority { get; set; }

        /// <summary>
        /// Contextual priority used for non-persistent ordering scenarios.
        /// </summary>
        /// <remarks>
        /// Supports UI-driven ordering such as column-header sorting.
        /// Does not modify or replace the canonical Priority value.
        /// </remarks>
        [Ephemeral]
        long? PriorityOverride { get; set; }
    }

    /// <summary>
    /// Represents a time slice snapshot using a captured UtcEpochNow that preempts race conditions.
    /// </summary>
    internal interface ITemporalAffinity : IPrioritizedAffinity
    {
        /// <summary>
        /// Affinity behavior
        /// </summary>
        TemporalAffinity? TemporalAffinity { get; set; }


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
        TimeSpan Duration { get; set; }

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
        TimeSpan Remaining { get; set; }
        /// <summary>
        /// Marks the beginning of this item's epoch and may be floating or fixed.
        /// </summary>
        [Ephemeral]
        DateTimeOffset? UtcStart { get; set; }

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
        [Ephemeral]
        TimeSpan? Available { get; }

        /// <summary>
        /// UtcStart + UtcRemaining.
        /// </summary>
        [Ephemeral]
        DateTimeOffset? UtcEnd { get; }

        /// <summary>
        /// Partial epochs that float around fixed epochs.
        /// </summary>
        IList<AffinitySlot> Slots { get; }

        /// <summary>
        /// Derived mode when UtcParent is not null.
        /// </summary>
        [Ephemeral]
        ChildAffinityMode? AffinityChildMode { get; }

        /// <summary>
        /// Aspirational mode that requires UtcStart and UtcEnd
        /// </summary>
        [Ephemeral]
        AffinityTimeDomain? AffinityTimeDomain { get; }

        /// <summary>
        /// Available < Remaining.
        /// </summary>
        /// <remarks>
        /// Signals a pathological time scarcity where the available time has
        /// fallen below the promised commitment. Typically this occurs when
        /// the AffinityField is paused (i.e., not going according to plan).
        /// </remarks>
        [Ephemeral]
        bool? IsPastDue { get; }

        /// <summary>
        /// Remaining = TimeSpan.Zero but !IsChecked.
        /// </summary>
        /// <remarks>
        /// Signals the benign version of running out of time, where 
        /// the affinity field has been running and decrementing remaining.
        /// </remarks>
        [Ephemeral]
        bool? OutOfTime { get; }

        /// <summary>
        /// Ephemeral two way binding that typically maps to a persistent IsChecked style property.
        /// </summary>
        /// <remarks>
        /// 1. The IsDone flag "goes true" when remaining time hits TimeSpan.Zero.
        /// 2. Setting IsDone, affirmatively, clears remaining time to TimeSpan.Zero.
        /// Mental Model:
        /// - The UI can opt to visually "go yellow" in a figurative, purgatory sense to
        ///   indicate a "Run-Not-Done". This shorthand term for a run-induced done state
        ///   is acknowleding a reality: The item ran out of time. But is it really done
        ///   to where "I can check it off my list?" This checkbox [X] is exactly what
        ///   many UI implementations allow the user to do.
        /// </remarks>
        [Ephemeral]
        bool? IsDone { get; set; }
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
    internal interface IAffinitySliceEmitter : INotifyPropertyChanged
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
    internal interface IAffinityField : INotifyPropertyChanged
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
        /// possible for an item to figuratively or visually "go into the red" in 
        /// terms of a shortfall of what can be delivered compared to promises made.
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
        /// Current State for a customizable finite state machine .
        /// </summary>
        Enum AffinityFsmState { get; }
    }
    internal enum ReservedAffinityStateMachine
    { 
        Idle = -1, 
    }

    internal enum AffinityFsm
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