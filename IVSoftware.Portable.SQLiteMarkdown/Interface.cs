using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public interface IModelAuthorityContext
    {
        XElement Model { get; }
        IDisposable RequestAuthority(ModelDataExchangeAuthority authority);
        IDisposable RequestAuthority(StdModelAuthority authority);

        /// <summary>
        /// Dedicated primary authority.
        /// </summary>
        /// <remarks>
        /// Unlike StdModelAuthority which is always checked with HasAuthority.
        /// </remarks>
        ModelDataExchangeAuthority ModelDataExchangeAuthority { get; }

        bool HasAuthority(Enum authority);
    }

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
    [PublishedContract("1.x", typeof(IObservableQueryFilterSource))]
    [Careful("Must *never* implement IMarkdownContext (newer) - Breaks the contract - Shouldn't be public anyway.")]
    public interface IObservableQueryFilterSource
        : IList
        , INotifyCollectionChanged
        , INotifyPropertyChanged
    {
        /// <summary>
        /// Indicates that search input filters the current working set, including an armed empty input.
        /// </summary>
        bool IsFiltering { get; }
        string InputText { get; set; }
        SearchEntryState SearchEntryState { get; }
        FilteringState FilteringState { get; }
        string Placeholder { get; }
        bool Busy { get; }
        QueryFilterConfig QueryFilterConfig { get; set; }
        string Title { get; set; }
        string SQL { get; }

        /// <summary>
        /// Canonical persistent data set (when available).
        /// </summary>
        /// <remarks>
        /// EXAMPLE: Searchable settings.
        /// </remarks>
        SQLiteConnection MemoryDatabase { get; set; }

        FilteringState Clear(bool all = false);
        void Commit();

        event EventHandler? InputTextSettled;
        event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;
    }

    /// <summary>
    /// Strongly typed variant of IObservableQueryFilterSource.
    /// </summary>
    [PublishedContract("1.x")]
    public interface IObservableQueryFilterSource<T>
        : IObservableQueryFilterSource
    {
        void InitializeFilterOnlyMode(IEnumerable<T> items);
        void ReplaceItems(IEnumerable<T> items);
        Task ReplaceItemsAsync(IEnumerable<T> items);
        DisposableHost DHostBusy { get; }
    }

    /// <summary>
    /// Legacy marker interface preserved for v1 contract compatibility.
    /// </summary>
    [Obsolete("Backward compatibility only.")]
    public interface IEditableQueryFilterItem { }

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
    [NotFlags]
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
}
