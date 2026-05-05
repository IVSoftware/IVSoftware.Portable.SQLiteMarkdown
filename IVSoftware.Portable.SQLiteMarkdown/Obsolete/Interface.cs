using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Obsolete
{

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
    [Careful("Must *never* implement INotifyCollectionChanged - this is reserved to detect inheritance.")]
    [PublishedContract("2.0.0-alpha31", typeof(IMarkdownContext))]
    public interface IMarkdownContext : INotifyPropertyChanged
    {
        #region P A R S E
        /// <summary>
        /// The canonical contract type that defines the authoritative table shape for this context.
        /// </summary>
        Type ContractType { get; }

        /// <summary>
        /// The type whose attributes define the parsing behavior.
        /// </summary>
        /// <remarks>
        /// Must be a non-interface type. Abstract types are permitted.
        /// The proxy type must resolve to the same underlying table as <see cref="ContractType"/>.
        /// 
        /// Multiple proxy types may target the same table schema, each providing a different
        /// attribute-driven interpretation for parsing and filtering.
        /// </remarks>
        Type ProxyType { get; }

        /// <summary>
        /// Use the current value of InputText to parse an expression against ContractType.
        /// </summary>
        string ParseSqlMarkdown();
        string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast);
        string ParseSqlMarkdown<T>();
        string ParseSqlMarkdown<T>(string expr, QueryFilterMode qfMode = QueryFilterMode.Query);

        /// <summary>
        /// Parses the current <see cref="InputText"/> and raises the <see cref="RecordsetRequest"/> event.
        /// </summary>
        /// <remarks>
        /// Represents the transition point between input parsing and recordset acquisition.
        /// Subscribers may use the current SQL expression to supply a recordset, but are not required to do so.
        ///
        /// This method defines the execution boundary for Query mode. Unlike Filter mode, which
        /// applies changes after a debounced settling interval, Query mode does not impose a
        /// settling timeout on input changes and instead requires an explicit commit.
        /// </remarks>
        void Commit();
        #endregion P A R S E

        #region B I N D A B L E    P R O P E R T I E S

        /// <summary>
        /// Bindable property intended for external IME.
        /// </summary>
        /// <remarks>
        /// In Filter states, text changes are given a setting time before 
        /// entering a query epoch on the internal SQLite filtering database.
        /// </remarks>
        string InputText { get; set; }

        /// <summary>
        /// Bindable property intended for UI configuration swaps. 
        /// </summary>
        /// <remarks>
        /// - Placeholder text Search or Filter
        /// - Search Icon or Filter Icon, where colors follow verbose states.
        /// </remarks>
        bool IsFiltering { get; }

        /// <summary>
        /// Nuanced state that takes InputText length into account.
        /// </summary>
        FilteringState FilteringState { get; }

        /// <summary>
        /// Bindable property intended for visual colors, icon swaps, and placeholder text.
        /// </summary>
        SearchEntryState SearchEntryState { get; }

        #endregion B I N D A B L E    P R O P E R T I E S

        #region C O N F I G U R A T I O N
        /// <summary>
        /// Constrains the state machine to Query or Filter semantics only, or give the FSM full access to both.
        /// </summary>
        QueryFilterConfig QueryFilterConfig { get; set; }
        #endregion C O N F I G U R A T I O N

        /// <summary>
        /// Default LIMIT term for SQLite term generation.
        /// </summary>
        /// <remarks>
        /// LIMIT term is skipped when set to the default value of uint.MinValue
        /// </remarks>
        uint DefaultLimit { get; set; }

        /// <remarks>
        /// As a defining feature, the Clear method is a progressive state 
        /// demotion. An actively filtering collection UI will take:
        /// [X] to clear the filter term IME while armed for a new filter term.
        /// [X] to return to query state, leaving the list items (if any) populated.
        /// [X] to clear the visible list, ready for a new query.
        /// </remarks>
        FilteringState Clear(bool all);

        TimeSpan InputTextSettlingTime { get; set; }

        event EventHandler? InputTextSettled;

        #region D I S P O S A B L E
        /// <summary>
        /// Bindable property that returns true when the busy count is < 1;
        /// </summary>
        bool Busy { get; }

        /// <summary>
        /// Increments the internal busy count and returns a disposable token to decrement it on dispose.
        /// </summary>
        IDisposable BeginBusy();

        #endregion D I S P O S A B L E

        /// <summary>
        /// Gets the total number of items in the canonical ledger.
        /// </summary>
        int CanonicalCount { get; }

        /// <summary>
        /// Gets the number of canonical items that satisfy the active predicate.
        /// </summary>
        int PredicateMatchCount { get; }

        /// <summary>
        /// Provides the current table names of the internal SQLite database used for filtering.
        /// </summary>
        string[] GetTableNames();
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
    [PublishedContract("2.0.0-alpha31", typeof(IPredicateMarkdownContext))]
    [Careful("This interface is not allowed to implement INotifyCollectionChanged.")]
    public interface IPredicateMarkdownContext : IMarkdownContext
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
    public interface ITopology
    {
        #region C O N F I G U R A T I O N    P R O P E R T I E S
        /// <summary>
        /// OPT-IN that allows MarkdownContext to modify the ObservableNetCollection directly.
        /// </summary>
        NetProjectionTopology ProjectionTopology { get; }

        /// <summary>
        /// Determines whether filter update events are provided as structural changes
        /// with old-new item semantics, alternatively as a bulk reset, or both.
        /// </summary>
        /// <remarks>
        /// Some UI platforms respond more efficiently to a raw reset.
        /// </remarks>

        [Careful("Not to be conflated with CollectionChangingEventingPolicy")]
        ReplaceItemsEventingPolicy ReplaceItemsEventingPolicy { get; set; }
        #endregion C O N F I G U R A T I O N    P R O P E R T I E S

        #region P R O J E C T I O N
        /// <summary>
        /// Represents a bindable and observable collection representing 'net visible' filtered items.
        /// </summary>
        IList? ObservableNetProjection { get; }

        public IList CanonicalSuperset { get; }

        public IList PredicateMatchSubset { get; }
        #endregion  P R O J E C T I O N

        public int Count { get; }
    }

#if false
    public interface IModeledMarkdownContext
        : IMarkdownContext
        , ITopology
    {
        #region M O D E L
        /// <summary>
        /// Maintains the canonical recordset as a hierarchy.
        /// </summary>
        /// <remarks>
        /// - The string value of the property designated as the PK is 
        ///   used as the address in the item.
        /// - For item types that support a FullPath property, the
        ///   model can also represent depth.
        /// </remarks>
        XElement Model { get; }

        /// <summary>
        /// Requests that an external host raise a collection change notification.
        /// </summary>
        /// <remarks>
        /// <see cref="MarkdownContext"/> itself does not implement <see cref="INotifyCollectionChanged"/>.
        /// Instead, canonical mutations are surfaced through this request so that an owning
        /// surface—often a derived type or UI adapter that *does* implement
        /// <see cref="INotifyCollectionChanged"/>—may relay the corresponding
        /// <see cref="NotifyCollectionChangedEventArgs"/> to observers.
        ///
        /// Mental Model: "Filtering model has been reconfigured. Ask the host to raise INCC."
        /// </remarks>
        event EventHandler ModelSettled;
        #endregion M O D E L

        #region P R O J E C T I O N

        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        void LoadCanon(IEnumerable? recordset);

        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        [Probationary]
        Task LoadCanonAsync(IEnumerable? recordset);
        #endregion P R O J E C T I O N

        #region D I S P O S A B L E
        /// <summary>
        /// Guards receptivity of the unfiltered items collection.
        /// </summary> 
        /// <remarks>
        /// Intended use: EpochFinalizing should be wrapped with this reference counter.
        /// </remarks>
        IDisposable BeginCollectionChangeAuthority(CollectionChangeAuthority authority);

        /// <summary>
        /// Returns the current collection DDX authority.
        /// </summary>
        CollectionChangeAuthority Authority { get; }

        #endregion D I S P O S A B L E
    }
    public interface IModeledMarkdownContext<T> : IModeledMarkdownContext
    {
        /// <summary>
        /// Represents a bindable and observable collection representing 'net visible' filtered items.
        /// </summary>
        new ObservableCollection<T>? ObservableNetProjection { get; }
        void SetObservableNetProjection(
            ObservableCollection<T>? onp,
            NetProjectionTopology? option = null);

        new IReadOnlyList<T> CanonicalSuperset { get; }

        new IReadOnlyList<T> PredicateMatchSubset { get; }
    }
#endif
}
