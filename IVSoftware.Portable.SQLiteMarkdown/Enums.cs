using System;
using System.Collections.Generic;
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
    public enum FilteringState
    {
        /// <summary>
        /// One of:
        /// - Filtering is either globally disabled.
        /// - The minimum item count of 2 UNFILTERED items is not present.
        /// </summary>
        Ineligible,

        /// <summary>
        /// - The list meets the minumum requirement.
        /// - Now, when the text changes, a query will execute and filtering 
        ///   will be considered active regardless of the filtered result count.
        /// </summary>
        /// <remarks>
        /// - Pushing Armed will clear the filtered items buffer.
        /// </remarks>
        Armed,

        /// <summary>
        /// The visible list items represent a filtered
        /// subset of records that match the predicate.
        /// </summary>
        Active,
    }

    /// <summary>
    /// IME flat state machine. Takes on directionality in conjunction with FilteringState.
    /// </summary>
    public enum SearchEntryState
    {
        /// <summary>
        /// The 'absolute zero' of the state machine and is the first of two states representing an empty IME.
        /// </summary>
        /// <remarks>
        /// Based on reachable states that are dependent on config, the gravity
        /// of regressive state machine 'eventually' reaches this state.
        /// Mental Model:
        /// When IME is non-empty:
        /// - The first call to Clear empties the IME. Then:
        /// - If FilteringState.Active then -> FilteringState.Armed + FilteredItems 
        ///   will contain All items. When Query mode is reachable, this will be 'all 
        ///   items in the current recordset' whereas in Filter-only mode this will
        ///   be all items in the canonical unfiltered collection.        ///   
        /// When IME is already empty:
        /// - When Query state is reachable:
        ///   case FilteringState.Armed then -> FilteringState.Ineligible + FilteredItems will contain None.
        /// </remarks>
        Cleared,

        /// <summary>
        /// This is the second of two states representing an empty IME.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        QueryEmpty,

        QueryENB,

        QueryEN,

        #region Q U E R Y
        QueryCompleteNoResults,

        QueryCompleteWithResults,
        #endregion Q U E R Y
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
}
