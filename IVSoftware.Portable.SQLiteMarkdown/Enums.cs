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
    public enum SearchEntryState
    {
        Cleared,

        QueryEmpty,

        QueryENB,

        QueryEN,

        #region Q U E R Y
        QueryCompleteNoResults,

        QueryCompleteWithResults,
        #endregion Q U E R Y
    }

    /// <summary>
    /// Flags-based enum controlling the allowable states of the FSM
    /// </summary>
    [Flags]
    public enum QueryFilterConfig
    {
        /// <summary>
        /// A configuration that provides Query behavior only.
        /// </summary>
        Query = 0x00040000,

        /// <summary>
        /// A configuration that provides Filter behavior only.
        /// </summary>
        Filter = 0x00100000,

        /// <summary>
        /// A configuration that provides both Query and Filter state-based behaviors.
        /// </summary> 
        QueryAndFilter = Query | Filter,
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
