using IVSoftware.Portable.Common.Attributes;
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

    /// <summary>
    /// Specifies which collection currently has synchronization authority.
    /// </summary>
    /// <remarks>
    /// Defines which side is authoritative when propagating changes between
    /// the canonical unfiltered collection and its filtered projection.
    /// 
    /// Terminology:
    /// - Canonical refers to the unfiltered source collection.
    /// - Projection refers to the filtered collection derived from the canonical source.
    /// - Upstream propagation means synchronization from the projection back to the canonical source.
    /// 
    /// Authority may shift during refinement epochs to suppress upstream propagation
    /// and prevent circular collection change events.
    /// </remarks>
    public enum CollectionSyncAuthority
    {
        /// <summary>
        /// The filtered projection is authoritative.
        /// Changes originating in the filtered collection may propagate
        /// to the canonical unfiltered backing collection.
        /// </summary>
        /// <remarks>
        /// Mental Model (typical):
        /// - The filtered collection represents the current visible projection.
        /// - User-facing {add, edit, remove} operations occur against this projection.
        /// - These operations are treated as authoritative and synchronized upstream.
        /// </remarks>
        Filtered,

        /// <summary>
        /// The canonical unfiltered collection is authoritative.
        /// Changes in the filtered projection are not propagated upstream.
        /// </summary>
        /// <remarks>
        /// Mental Model (typical):
        /// - The unfiltered collection represents the canonical source for a settled state.
        /// - During a refinement epoch, the filtered projection is modified programmatically.
        /// - Upstream propagation is suppressed to prevent circular collection change events.
        /// </remarks>
        Unfiltered,
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
        text,

        /// <summary>
        /// This record matches all active predicates.
        /// </summary>
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
        /// Mental Model: "Temporary UI-driven ordering (e.g., column header sorts)."
        /// </remarks>
        sort,
        preview,
    }

    /// <summary>
    /// States authority inside a NotifyCollectionChanged event handler.
    /// </summary>
    public enum NotifyCollectionChangedEventAuthority
    {
        /// <summary>
        /// The user has effected a (presumably UI-related) change to a filtered collection.
        /// </summary>
        /// <remarks>
        /// During filtering, the MarkdownContext tracks a 'canonical' unfiltered
        /// list which now (counterinuitively) must be updated to remain canonical.
        /// </remarks>
        NetProjection,

        /// <summary>
        /// The markdown context is notifying a change to the net
        /// projection, the (presumably visible) collection.
        /// </summary>
        MarkdownContext,
    }

    /// <summary>
    /// Set of all supported states with canonical indexing.
    /// </summary>
    [Canonical("FSM enums should declare members relative to numeric indexes in this set.")]
    internal enum StdFSMState
    {
        [Obsolete]
        InitializeUnfilteredItemsCollection = 1,

        /// <summary>
        /// Clear or Create Table for ContractType.
        /// </summary>
        InitFQBDForEpoch,

        /// <summary>
        /// Build the XML model of the canonical recordset.
        /// </summary>
        InitModelForEpoch,

        /// <summary>
        /// Set SearchEntryState and FilteringState contextually.
        /// </summary>
        InitStatesForEpoch,

        /// <summary>
        /// Using a DHostSuppress token, populate the NetProjection. Reset will happen on token release.
        /// </summary>
        NetProjectWithSuppress,

        /// <summary>
        /// Empty the contents of the contract table.
        /// </summary>
        ResetFQBDForEpoch,

        /// <summary>
        /// Empty the contents of the contract table.
        /// </summary>
        ResetModelForEpoch,

        /// <summary>
        /// Update current values for CanonicalCount and PredicateMatchCount by iterating model.
        /// </summary>
        UpdateCounts,
    }

    /// <summary>
    /// 260228 inprog
    /// </summary>
    internal enum InitFilterEpochFSM
    {
        InitFQBDForEpoch = StdFSMState.InitFQBDForEpoch,

        InitModelForEpoch = StdFSMState.InitModelForEpoch,

        UpdateCounts = StdFSMState.UpdateCounts,

        InitStatesForEpoch = StdFSMState.InitStatesForEpoch,
    }

    internal enum ResetFilterEpochFSM
    {
        ResetFQBDForEpoch = StdFSMState.ResetFQBDForEpoch,

        ResetModelForEpoch = StdFSMState.ResetModelForEpoch,

        UpdateCounts = StdFSMState.UpdateCounts,
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
        /// The ObservableNetProjection inherits INotifyCollectionChanged - flitering employs copying not routing.
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

    [Flags, Probationary]
    internal enum NetProjectionOption
    {
        /// <summary>
        /// Default permissive.
        /// </summary>
        /// <remarks>
        /// MentalModel: "The explicit assignment of ObservableNetProjection *is" the OPT-IN."
        /// </remarks>
        AllowDirectChanges = 0x0,

        /// <summary>
        /// Track INCC events but don't attempt to cast IList or make changes on the handle.
        /// </summary>
        ObservableOnly     = 0x1,
    }
}
