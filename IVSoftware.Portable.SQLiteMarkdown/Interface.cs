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
    /// Identifies an item that exposes a single, sortable position value.
    /// </summary>
    /// <remarks>
    /// <see cref="Position"/> represents a canonical ordering index relative
    /// to peer items within a given collection. The value establishes a
    /// total ordering and is intended for deterministic sorting or stable
    /// reordering scenarios.
    /// 
    /// This contract defines a single intrinsic ordering axis only. It does
    /// not imply multi-dimensional coordinates, contextual sorting rules,
    /// or view-specific projections. Such concerns belong to the hosting
    /// collection or projection layer.
    /// </remarks>
    public interface IPositional
    {
        long Position { get; set; }
    }

    public interface ICustomProperties
    {
        IDictionary<string, string?> CustomProperties { get; }
    }

    /// <summary>
    /// Intermediary snapshot of a moment in time that must complete before advancing.
    /// </summary>
    /// <remarks>
    /// Allows IUtcEpoch to operate without race conditions.
    /// </remarks>
    public interface UtcEpochClock
    {
        /// <summary>
        /// Captured epoch reference.
        /// </summary>
        DateTimeOffset? UtcEpochNow { get; set; }

        int Second { get; }
        int Minute { get; }
        int Hour { get; }
        int Day { get; }
        int Month { get; }
        int Year { get; }

        event EventHandler NewSecond;
        event EventHandler NewMinute;
        event EventHandler NewHour;
        event EventHandler NewDay;
        event EventHandler NewMonth;
        event EventHandler NewYear;
    }

    public enum UtcEpochTimeDomain
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

    [Flags]
    public enum UtcEpochMode
    {
        /// <summary>
        /// Begins at a specified UtcStart and ends Remaining later.
        /// </summary>
        Fixed = 0x0,

        /// <summary>
        /// Begins as soon as possible relative to UtcNow and Position.
        /// </summary>
        Asap = 0x1,

        AsapBefore = Asap | Asap << 1,

        AsapAfter =  Asap | Asap << 2,
    }

    /// <summary>
    /// Represents a time slice snapshot using a captured UtcEpochNow that preempts race conditions.
    /// </summary>
    public interface IUtcEpoch
    {
        /// <summary>
        /// Reference point for UtcEpochMode.Fixed.
        /// </summary>
        DateTimeOffset? UtcStart { get; set; }

        /// <summary>
        /// Total epoch length, if any.
        /// </summary>
        /// <remarks>
        /// UtcEpochMode.Fixed + (Duration != TimeSpan.Zero) means a
        /// deadline of UtcStart + Duration for AsapBelow items.. 
        /// </remarks>
        TimeSpan? Duration { get; set; }

        /// <summary>
        /// Uncompleted epoch length, if any.
        /// </summary>
        TimeSpan? Remaining { get; set; }

        /// <summary>
        /// UtcStart + UtcRemaining.
        /// </summary>
        DateTimeOffset? UtcEnd { get; }

        /// <summary>
        /// Math.Max(TimeSpan.Zero, Remaining) == TimeSpan.Zero 
        /// </summary>
        bool? IsDone { get; }

        /// <summary>
        /// Aspirational mode that requires UtcStart
        /// </summary>
        UtcEpochMode? EpochMode { get; set; }

        /// <summary>
        /// Aspirational mode that requires UtcStart
        /// </summary>
        UtcEpochTimeDomain? EpochTimeDomain { get; }

        /// <summary>
        /// Returns false if !(UtcEpochMode.ASAP). 'Net True' decrements Remaining when item is UtcEpochTimeDomain.Present.
        /// </summary>
        bool? IsRunning { get; }

        /// <summary>
        /// Reference PK for UtcEpochMode.Before and UtcEpochMode.After
        /// </summary>
        string? Parent { get; }

        /// <summary>
        /// Partial epochs that float around fixed epochs.
        /// </summary>
        IList<IUtcEpochSlot> Slots { get; }
    }

    /// <summary>
    /// Partial epoch that is constrained by fixed time epochs in the collection.
    /// </summary>
    public interface IUtcEpochSlot
    {
        /// <summary>
        /// Earliest available start time in context,
        /// </summary>
        DateTimeOffset UtcStart { get; set; }

        /// <summary>
        /// Latest available start time in context,
        /// </summary>
        DateTimeOffset UtcEnd { get; set; }

        /// <summary>
        /// UtcEnd - UtcStart
        /// </summary>
        TimeSpan AvailableTime { get; }
    }
}