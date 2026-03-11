using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// This is marked as a [Flags] enum in spite of the inference that
    /// the Invalid and Valid could be combined, which of course they can't.
    /// </summary>
    [Flags]
    public enum ValidationState
    {
        // Search entry is empty. It contains no content characters or operators.
        Empty = 0,

        // Non-empty search entry that does not meet the validation predicate.
        Invalid = 1,

        // Search entry represents a valid query, but has not been submitted
        Valid = 2,

        DisableMinLength = 0x8,
    }
    public enum TermDelimiter
    {
        Comma,
        Semicolon,
        Tilde,
    }
    public enum PersistenceMode
    {
        /// <summary>
        /// Property is [SQLiteIgnore] and is Ephemeral.
        /// </summary>
        None,

        /// <summary>
        /// Property is [SQLiteIgnore] but is persisted in the SQLite record..
        /// </summary>
        Json,
    }

    /// <summary>
    /// Flags-based enum controlling the allowable states of the FSM.
    /// </summary>
    [Flags]
    public enum QueryFilterConfig
    {
        /// <summary>
        /// A configuration that provides Query behavior only.
        /// </summary>
        /// <remarks>
        /// Mental Model:
        /// - *Does Not* employ default settling semantics.
        /// - Observes query syntax validation as new input is received.
        /// - Performs a query only on receiving a UI-specific commit action.
        /// </remarks>
        Query = 0x00040000,

        /// <summary>
        /// A configuration that provides Filter behavior only.
        /// </summary>
        /// <remarks>
        /// Mental Model:
        /// - *Does* employ default settling semantics.
        /// - Filters a collection of two or more items after IMS input settles.
        /// </remarks>
        Filter = 0x00100000,

        /// <summary>
        /// A configuration that provides both Query and Filter state-based behaviors.
        /// </summary>
        /// Mental Model:
        /// - Host an initial Query state and arms Filter mode if recordset count is <= 2.
        /// - The initial query state must receive a UI-specific action to execute.
        /// - The conditional filter state reacts to *new* settled IME changes.
        /// </remarks>
        QueryAndFilter = Query | Filter,
    }

    /// <summary>
    /// State machine whose specific behavior depends on QueryFilterConfig
    /// </summary>
    public enum FilteringState
    {
        /// <summary>
        /// One of:
        /// - Filtering is either globally disabled.
        /// - The minimum item count of 2 UNFILTERED items is not present.
        /// </summary>
        Ineligible,

        /// <summary>
        /// - The canonical recordset epoch meets minumum requirement and will now adhere to Filter semantics.
        /// </summary>
        /// <remarks>
        /// - Settled text changes now invoke internal filter queries.
        /// - This epoch cannot fall below Armed without a Clear.
        /// - Specifically, empty IME alone does not make this happen.
        /// </remarks>
        Armed,

        /// <summary>
        /// The 'net projection' items represent a filtered subset of records that match the predicate.
        /// </summary>
        /// <remarks>
        /// When the regressive Clear is called in this state:
        /// - The IME is emptied.
        /// - The FilteredItems collection (e.g. 'visible items') matched the canonical UnfilteredItems collection.
        /// </remarks>
        Active,
    }

    /// <summary>
    /// IME flat state machine. Takes on directionality in conjunction with FilteringState.
    /// </summary>
    public enum SearchEntryState
    {
        /// <summary>
        /// The 'absolute zero' of the query-filter state machine that represents a regressive Clear response.
        /// </summary>
        /// <remarks>
        /// - The IME has been cleared.
        /// - The FilteredItems collection (e.g. "visible items") *has* (also) been cleared.
        /// </remarks>
        Cleared,

        /// <summary>
        /// Intermediate response to regressive Clear.
        /// </summary>
        /// <remarks>
        /// - The IME has been cleared.
        /// - The projection (e.g. "visible items") *has not* been cleared.
        /// - WHEN COMBINED WITH FilteringState.Ineligible, new keystrokes form a markdown query and require Commit.
        /// - WHEN COMBINED WITH FilteringState.Armed, new keystrokes will settle and filter.
        /// </remarks>
        [Description("The IME is empty whether IsFiltering or not.")]
        QueryEmpty,

        /// <summary>
        /// IME is non-empty, but *has not* met the validation (e.g. '3 char minimum') to execute a query.
        /// </summary>
        /// <remarks>
        /// - NOT REACHABLE in QueryFilterConfig.Filter.
        /// </remarks>
        QueryENB,

        /// <summary>
        /// IME *has* met the validation (e.g. '3 char minimum') to execute a query and is awaiting Commit.
        /// </summary>
        /// <remarks>
        /// - NOT REACHABLE in QueryFilterConfig.Filter.
        /// </remarks>
        QueryEN,

        #region Q U E R Y
        /// <summary>
        /// The Commit action yielded 0 results for the current IME state.
        /// </summary>
        /// <remarks>
        /// Mental Models for 'QueryCompleteNoResults' and 'QueryCompleteWithResults':
        /// In QueryFilterConfig.Query mode:
        /// - Always represents the result of the *external* user-defined query.
        /// - State *cannot be advanced*.
        /// - No mechanism exists to refine this query or filter the results.
        /// In QueryFilterConfig.QueryAndFilter mode:
        /// - If FilteringState == FilteringState.Ineligible
        ///   Then this represents the result of the *external* user-defined query. 
        /// - If FilteringState > FilteringState.Ineligible (is filtering a current recordset)
        ///   Then this represents the result *internal* query on the UnfilteredItems memory database.
        /// In QueryFilterConfig.Filter mode:
        /// - Always represents the result *internal* query on the UnfilteredItems memory database.
        /// - A state of 'QueryCompleteNoResults' when UnfilteredItems is non-empty is a special case
        ///   that transfers authority to the AdaptiveShowAll state machine.
        /// </remarks>
        QueryCompleteNoResults,

        /// <summary>
        /// The Commit action yielded >= 1 result for the current IME state.
        /// </summary>
        /// <remarks>
        /// Mental Model (QUERY + FILTER MODE): "The icon and placeholder are clear: Enter more characters to filter the visible items."
        /// - IsFiltering is true ONLY in this state.
        /// </remarks>
        QueryCompleteWithResults,
        #endregion Q U E R Y
    }

    public enum Win32Message
    {
        WM_MOUSEMOVE = 0x0200,
        WM_MOUSELEAVE = 0x02A3,
        WM_NCMOUSEMOVE = 0x00A0,
        WM_MOUSEHOVER = 0x02A1,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_CONTEXTMENU = 0x007B,
    }

    internal enum StdMarkdownElement
    {
        model,
        xitem,
    }

    internal enum StdMarkdownAttribute
    {
        /// <summary>
        /// Path segment name. 
        /// </summary>
        /// <remarks>
        /// - This is typically both a guid and also the PK.
        /// - Not to be confused with Description.
        /// </remarks>
        text,

        /// <summary>
        /// A truncated reference version of Description.
        /// </summary>
        /// <remarks>
        /// Not to be confused with text which is generally a guid.
        /// </remarks>
        preview,

        /// <summary>
        /// The XBoundAttribute that holds this data model.
        /// </summary>
        /// <remarks>
        /// Not to be confused with the XElement Model property.
        /// </remarks>
        model,

        /// <summary>
        /// This record matches all active predicates.
        /// </summary>
        [DefaultValue("True")]
        ismatch,

        /// <summary>
        /// Canonical record order.
        /// </summary>
        /// <remarks>
        /// Serialized form of <c>Priority</c>.
        /// Mental Model: "The canonical sequential order of the records."
        /// </remarks>
        order,

        /// <summary>
        /// Local sort override.
        /// </summary>
        /// <remarks>
        /// Serialized form of <c>PriorityOverride</c>.
        /// Mental Model: "The user clicked the column header and this is the temporary sort."
        /// </remarks>
        sort,

        /// <summary>
        /// The total running count.
        /// </summary>
        [DefaultValue(0)]
        count,

        /// <summary>
        /// The total running count, based on XElement.Changed.
        /// </summary>
        [DefaultValue(0)]
        autocount,

        /// <summary>
        /// In filter mode, the number of items matching all the predicates.
        /// </summary>
        [DefaultValue(0)]
        matches,

        #region P R E D I C A T E S
        /// <summary>
        /// XBoundObject that holds a sorting predicate when present.
        /// </summary>
        comparer,

        /// <summary>
        /// XBoundObject that holds a filter predicates when present.
        /// </summary>
        predicates,
        #endregion P R E D I C A T E S
    }

    /// <summary>
    /// States authority inside a NotifyCollectionChanged event handler.
    /// </summary>
    /// <remarks>
    /// Defines which side is authoritative when propagating changes between
    /// the canonical unfiltered collection and its filtered projection.
    /// 
    /// Terminology:
    /// - NetProjection (typically a UI surface) refers to the items allowed by any active queries or filters. 
    /// - Canonical refers to the backend collection captured when state enters IsFiltering.
    /// - PredicateMatchSubset refers to the backend collection maintained by the Model.
    /// 
    /// Authority may shift during refinement epochs to suppress upstream propagation
    /// and prevent circular collection change events.
    /// </remarks>
    public enum CollectionChangeAuthority
    {
        /// <summary>
        /// Explicit "no authority" grant.
        /// </summary>
        /// <remarks>
        /// Distinct from 0, which is the idle state of authority grants.
        /// - In practical terms, collection changed events are suppressed
        ///   in a manner that prevents internal collections from interacting.
        /// - Can be combined with a reset authority, which raises a Reset
        ///   collection changed event when the churn has settled out.
        /// </remarks>
        None = 1,

        /// <summary>
        /// The user has effected a (presumably UI-related) change to a filtered collection.
        /// </summary>
        /// <remarks>
        /// Mental Model (typical): "When I add a new item, the current filter must not be allowed to 'immediately' hide it."
        /// User-facing {add, edit, remove} operations that occur against a filtered projection are *exempt* from the filter.
        /// </remarks>
        NetProjection = None + 1,

        /// <summary>
        /// The markdown context is notifying a change to the net
        /// projection, the (presumably visible) collection.
        /// </summary>
        MarkdownContext = NetProjection + 1,
    }

    /// <summary>
    /// Set of all supported states with canonical indexing.
    /// </summary>
    [Canonical("FSM enums should declare members relative to numeric indexes in this set.")]
    internal enum StdFSMState
    {
        DetectFastTrack = 1,

        ClearProjection,

        /// <summary>
        /// Clear or Create Table for ContractType.
        /// </summary>
        /// <remarks>
        /// The decision to repopulate with a canonical recordset
        /// is switched on the enum type.
        /// </remarks>
        ResetOrCanonizeFQBDForEpoch,

        /// <summary>
        /// Empty the contents of the contract table.
        /// </summary>
        ResetOrCanonizeModelForEpoch,

        /// <summary>
        /// Set SearchEntryState and FilteringState contextually.
        /// </summary>
        UpdateStatesForEpoch,

        /// <summary>
        /// User interactive Add.
        /// </summary>
        AddItemToModel,

        /// <summary>
        /// User interactive Remove.
        /// </summary>
        RemoveItemFromModel,

        /// <summary>
        /// Using a DHostSuppress token, populate the NetProjection. Reset will happen on token release.
        /// </summary>
        NetProjectWithSuppress,
    }

    /// <summary>
    /// Executes on rising edge of IsFiltering.
    /// </summary>
    internal enum LoadIsFilteringEpochFSM
    {
        DetectFastTrack = StdFSMState.DetectFastTrack,

        ResetOrCanonizeFQBDForEpoch = StdFSMState.ResetOrCanonizeFQBDForEpoch,

        ResetOrCanonizeModelForEpoch = StdFSMState.ResetOrCanonizeModelForEpoch,

        UpdateStatesForEpoch = StdFSMState.UpdateStatesForEpoch,
    }

    /// <summary>
    /// Executes on rising edge of IsFiltering.
    /// </summary>
    internal enum TrackUserAddItem
    {
        AddItemToModel = StdFSMState.AddItemToModel,
    }

    /// <summary>
    /// Executes on rising edge of IsFiltering.
    /// </summary>
    internal enum TrackUserRemoveItem
    {
        RemoveItemFromModel = StdFSMState.RemoveItemFromModel,
    }

    /// <summary>
    /// Executes on falling edge of IsFiltering.
    /// </summary>
    internal enum ResetFilterEpochFSM
    {
        ResetOrCanonizeFQBDForEpoch = StdFSMState.ResetOrCanonizeFQBDForEpoch,

        ResetModelForEpoch = StdFSMState.ResetOrCanonizeModelForEpoch,

        UpdateStatesForEpoch = StdFSMState.UpdateStatesForEpoch,
    }

#if false
    /// <summary>
    /// Executes on parameterless Clear invocation.
    /// </summary>
    [CollectionChangeAuthority(CollectionChangeAuthority.NetProjection)]
    internal enum NativeClearFSM
    {
        DetectFastTrack = StdFSMState.DetectFastTrack,

        ResetFQBDForEpoch = StdFSMState.ResetFQBDForEpoch,

        ResetModelForEpoch = StdFSMState.ResetModelForEpoch,
    }
#endif

    /// <summary>
    /// Empties the Model and the FiterQueryDatabase without eventing.
    /// </summary>
    [CollectionChangeAuthority(CollectionChangeAuthority.None)]
    internal enum ClearModelFSM
    {
        DetectFastTrack = StdFSMState.DetectFastTrack,

        ResetOrCanonizeFQBDForEpoch = StdFSMState.ResetOrCanonizeFQBDForEpoch,

        ResetModelForEpoch = StdFSMState.ResetOrCanonizeModelForEpoch,
    }

    /// <summary>
    /// Unconditional full clear suppresses event churn and raises final Reset.
    /// </summary>
    [ResetEpoch]
    [CollectionChangeAuthority(CollectionChangeAuthority.None)]
    internal enum NativeClearFSM
    {
        DetectFastTrack = StdFSMState.DetectFastTrack,

        ResetOrCanonizeFQBDForEpoch = StdFSMState.ResetOrCanonizeFQBDForEpoch,

        ResetOrCanonizeModelForEpoch = StdFSMState.ResetOrCanonizeModelForEpoch,

        UpdateStatesForEpoch = StdFSMState.UpdateStatesForEpoch,
    }

    public enum ProjectionTopology
    {
        /// <summary>
        /// No ObservableNetProjection has been assigned.
        /// </summary>
        None,

        /// <summary>
        /// *NOT* an ObservableNetProjection - Instead it inherits MarkdownContext and routes the enumerator.
        /// </summary>
        /// IsFiltering => 
        /// 1. DHostSuppress.GetToken to suppress INCC.
        /// 2. Copy 'this' to a canonical backing store and DB.
        /// 3. Query DB for term and populate _filteredItems.
        ///*4. Route the enumerator to _filteredItens.
        /// 5. Relinquish DHostSuppress to raise Reset.
        /// IsFiltering <=
        /// 1. DHostSuppress.GetToken to suppress INCC.
        ///*2. Route the enumerator to canonical.
        /// 3. Relinquish DHostSuppress to raise Reset.
        /// </remarks>
        Inheritance,

        /// <summary>
        /// The ObservableNetProjection inherits INotifyCollectionChanged - filtering employs copying not routing.
        /// </summary>
        /// <remarks>
        /// IsFiltering => 
        /// 1. DHostSuppress.GetToken to suppress INCC.
        /// 2. Copy 'this' to a canonical backing store and DB.
        /// 3. Query DB for term and populate _filteredItems.
        /// 4. Copy _filteredItems to 'this'.
        /// 5. Relinquish DHostSuppress to raise Reset.
        /// IsFiltering <=
        /// 1. DHostSuppress.GetToken to suppress INCC.
        /// 2. Copy canonical backing store to 'this'
        /// 3. Relinquish DHostSuppress to raise Reset.
        /// </remarks>
        Composition,
    }

    [NotFlags]
    public enum NetProjectionOption
    {
        /// <summary>
        /// Observe the projection for reconciliation but do not mutate it.
        /// </summary>
        /// <remarks>
        /// MDC subscribes to INCC events in order to maintain the canonical model,
        /// but treats the projection as externally owned by the UI.
        /// </remarks>
        ObservableOnly,

        /// <summary>
        /// Allow MDC to directly modify the projection collection.
        /// </summary>
        /// <remarks>
        /// OPT-IN: Enables MDC to puppeteer the projection when maintaining canonical
        /// state. This is a powerful opt-in that assumes MDC has safe write
        /// authority over the observable collection.
        /// </remarks>
        AllowDirectChanges,
    }

    [Flags]
    public enum ReplaceItemsOption
    {
        /// <summary>
        /// Replace items using standard INCC Remove and Add notifications.
        /// </summary>
        /// <remarks>
        /// Replacement is expressed through normal collection change events rather
        /// than a Reset. Each event may represent multiple items.
        ///
        /// Depending on the composition of the existing and replacement sets,
        /// the operation may produce:
        /// - a single Add event (when the original collection was empty), or
        /// - a Remove event followed by an Add event.
        ///
        /// MentalModel: "Observers reconcile the replacement through batched Remove/Add notifications."
        ///
        /// NOTES:
        /// Regardless of option flags:
        /// - A replacement that leaves the collection empty redirects to Reset semantics.
        /// - A replacement that begins with an empty collection redirects to Add semantics. 
        /// </remarks>
        DiscreteRemoveAndAddEvents = 0x1,

        /// <summary>
        /// Replace items by emitting a single Reset notification.
        /// </summary>
        /// <remarks>
        /// Observers are instructed to re-enumerate the collection rather than tracking structural changes.
        ///
        /// MentalModel: "The collection changed; refresh everything."
        ///
        /// NOTE:
        /// Regardless of option flags:
        /// - A replacement that leaves the collection empty redirects to Reset semantics.
        /// </remarks>
        ConsolidatedResetEvent = 0x2,
    }
}
