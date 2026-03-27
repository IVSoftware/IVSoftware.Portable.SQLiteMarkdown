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
        /// MarkdownContext
        /// </summary>
        mdc,

        /// <summary>
        /// The XBoundAttribute that holds this data model.
        /// </summary>
        /// <remarks>
        /// Not to be confused with the XElement Model property.
        /// </remarks>
        model,

        /// <summary>
        /// Records the net effect of qmatch and pmatch
        /// </summary>
        [DefaultValue("True")]
        match,

        /// <summary>
        /// This record matches all active predicates.
        /// </summary>
        [DefaultValue("True")]
        qmatch,

        /// <summary>
        /// This record matches all active predicates.
        /// </summary>
        [DefaultValue("True")]
        pmatch,

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
        triage,
        #endregion P R E D I C A T E S
    }

    /// <summary>
    /// Laws of Gravity for the Markdown Context Domain
    /// </summary>
    /// <remarks>
    /// ASYNCHONOUS
    /// - States should be Async and non-concurrent.
    /// - Lower requested states should have the authority to cancel Higher states.
    /// - Higher requested states should await the completion of Lower states.
    /// WORK PRODUCT - An event (even an 'Empty' one) should always be produced.
    /// - Empty: When IME is cleared.
    /// - Empty: When state changes Filter->Query.
    /// - BCL Reset: When Reset epoch completes.
    /// - Replace with Reason: When Commit() epoch completes.
    /// - Replace with Reason: When Remodel(bool) epoch completes.
    /// ReplaceItemsEventingOption
    /// - Replace with Reason is another way of saying Clear then Add.
    /// - Depending on this setting, replace actions produce 'any or all' of
    ///   1. BCL Reset event on the clear phase before the replace.
    ///   2. Structural 'Replace with Reason' event, which is a digest of the transaction.
    ///   3. BCL Add event on the repopulate phase after the replace.
    /// </remarks>
    [NotFlags, Description("Authority"), Probationary("260320")]
    public enum CollectionChangeAuthority
    {
        /// <summary>
        /// Programmatic calls on IList produce corresponding INCC events.
        /// </summary>
        /// <remarks>
        /// Mental Model: "No Surprises."
        /// - Routes to CanonicalSuperset(CSS) where all CSS events raise INCC on ObservableNetProjection(ONP).
        /// - The FSM is not allowed to change state in response.
        /// </remarks>
        None = 0,

        /// <summary>
        /// Epoch that returns to first cause and produces a single Base Class Library (BCL) event.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Clear everything while suppressing intermediate events. Raise a BCL Reset INCC when done."
        /// Association: <c>Clear(true)</c>
        /// </remarks>
        Reset,

        /// <summary>
        /// Epoch that begins with a programmatic Commit() command and produces an INotifyPpropertyChanged (INCC) event.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Ask user to query their primary data source. Load this recordset as canon."
        /// BEHAVIOR
        /// This is a direct action on CSS. 
        /// - Then ONP if it exists will be cleared and repopulated with the CSS items.
        /// - Epistemically, the ONP that is 'out there' might handle the changes one by 
        ///   one, or it can opt in to Replace with Reason events in addition or instead of.
        /// - Another good option for ONP is to check authority for suppression, then do
        ///   a settled reset when changes stop occurring.
        /// </remarks>
        Commit,

        /// <summary>
        /// Epoch that begins with a direct change to the ONP and ends with an INCC.
        /// </summary>
        /// <remarks>
        /// Mental Model: "The UI user performs an Add, Remove, or Move action on a Full or Filtered list."
        /// - Routes to CSS where all CSS events raise INCC on ONP.
        /// - The FSM is not allowed to change state in response.
        /// - The model must grant IsMatch status immediately to any 
        ///   new item for the current epoch, ensuring they they 
        ///   don't mysteriously disappear the moment they commit.         ///   
        /// Association: <c>Remodel(true)</c>
        /// </remarks>
        Projection,

        /// <summary>
        /// Epoch that begins when Input Method Entry (IME) changes to the InputText property have settled.
        /// </summary>
        /// <remarks> 
        /// Mental Model: "User is refining a recordset by modifying text in the IME."
        /// </remarks>
        Settle,

        /// <summary>
        /// Epoch that begins when active filter predicates have immediate consequences (e.g., Radio or CheckBoxes).
        /// </summary>
        /// <remarks>
        /// Mental Model: "UI Radio selection, e.g., [ShowAll, ShowUnchecked, ShowChecked]."
        /// - The FSM is not allowed to change state in response.
        /// - Routes to PMS where all PMS events raise INCC on ONP.
        /// Association: <c>Remodel(false)</c>
        /// </remarks>
        [Description("Verb: PRED-ih-kate")]
        Predicate,
    }

    /// <summary>
    /// Set of all supported states with canonical indexing.
    /// </summary>
    [Canonical("FSM enums should declare members relative to numeric indexes in this set.")]
    internal enum StdFSMState
    {
        DetectFastTrack = 1,

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

        /// <summary>
        /// Request that an external host raise a collection change notification.
        /// </summary>
        ModelSettled,
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
    /// Unconditional full clear suppresses event churn and raises final Reset.
    /// </summary>
    [CollectionChangeAuthority(CollectionChangeAuthority.Settle)]
    internal enum NativeClearFSM
    {
        DetectFastTrack = StdFSMState.DetectFastTrack,

        ResetOrCanonizeFQBDForEpoch = StdFSMState.ResetOrCanonizeFQBDForEpoch,

        ResetOrCanonizeModelForEpoch = StdFSMState.ResetOrCanonizeModelForEpoch,

        UpdateStatesForEpoch = StdFSMState.UpdateStatesForEpoch,

        RaiseModelSettledEvent = StdFSMState.ModelSettled,
    }

    public enum ProjectionTopology
    {
        /// <summary>
        /// Topology does not match any known pattern.
        /// </summary>
        Unknown,

        /// <summary>
        /// Not inherited and not associated by composition. Direct actions are not allowed.
        /// </summary>
        Self,

        /// <summary>
        /// Indicates a direct subclass that implements canonical IList (write) and routed ICollection (read).
        /// </summary>
        /// <remarks>
        /// Synchronization per se is not required because the read enumerator can switch between a canonical and a predicated collection.
        /// </remarks>
        Inheritance,

        /// <summary>
        /// Indicates that a non-canonical ObservableCollection{T} has been submitted as the visible surface.
        /// </summary>
        /// <remarks>
        /// IsFiltering edges are tracked, and either capture or revert the non-canonical projected surface with the internal canonical superset.
        /// </remarks>
        Composition,
    }

    /// <summary>
    /// Specifies whether 
    /// </summary>
    [NotFlags]
    public enum NetProjectionOption
    {
        /// <summary>
        /// Direct inheritance rules out
        /// </summary>
        None,

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
    public enum ReplaceItemsEventingOption
    {
        /// <summary>
        /// Emit structural collection change events that reflect the specific mutation.
        /// </summary>
        /// <remarks>
        /// The emitted INCC event corresponds to the structural transition classified
        /// by <see cref="ReplaceItemsEventingTriage"/>:
        /// - EmptyBefore  -> Add
        /// - EmptyAfter   -> Remove
        /// - NeverEmpty   -> Replace
        ///
        /// Each event may represent multiple items.
        ///
        /// MentalModel: "Observers reconcile the mutation structurally."
        /// </remarks>
        StructuralReplaceEvent = 0x1,

        /// <summary>
        /// Collapse any structural mutation into a single Reset event.
        /// </summary>
        /// <remarks>
        /// Observers are instructed to discard their current view and
        /// re-enumerate the collection regardless of the underlying mutation.
        ///
        /// MentalModel: "Something changed; refresh everything."
        /// </remarks>
        ResetOnAnyChange = 0x2,

        /// <summary>
        /// Emit both the structural mutation event and a Reset notification.
        /// </summary>
        /// <remarks>
        /// The structural INCC event (Add, Remove, or Replace) is raised first
        /// according to <see cref="ReplaceItemsEventingTriage"/>, followed by a
        /// Reset event.
        ///
        /// This mode preserves structural information for observers that track
        /// incremental mutations while also forcing views that rely on Reset
        /// semantics to refresh.
        ///
        /// MentalModel: "Tell observers exactly what changed, then force a refresh."
        /// </remarks>
        All = ResetOnAnyChange | StructuralReplaceEvent,
    }

    [NotFlags]
    internal enum ReplaceItemsEventingTriage
    {
        /// <summary>
        /// Describes a noop "wash" invocation.
        /// </summary>
        /// <remarks>
        /// Unconditionally suppress event invocation.
        /// </remarks>
        AlwaysEmpty,

        /// <summary>
        /// Describes a virtual replacement with Add semantics.
        /// </summary>
        /// <remarks>
        /// StructuralReplaceEvent - generates a Add action for N new items.
        /// ResetOnAnyChange - generates an Reset event.
        /// </remarks>
        EmptyBefore,

        /// <summary>
        /// Describes a virtual replacement with Clear semantics.
        /// </summary>
        /// <remarks>
        /// StructuralReplaceEvent - generates a Remove action for N old items.
        /// ResetOnAnyChange - generates an Reset event.
        /// </remarks>
        EmptyAfter,

        /// <summary>
        /// Describes a virtual replacement with Replace semantics.
        /// </summary>
        /// <remarks>
        /// StructuralReplaceEvent - generates a Replace action for N old and N' new items.
        /// ResetOnAnyChange - generates an Reset event.
        /// </remarks>
        NeverEmpty,
    }
}
