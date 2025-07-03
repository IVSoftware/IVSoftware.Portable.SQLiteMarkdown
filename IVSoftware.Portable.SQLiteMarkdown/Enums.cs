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
    /// <summary>
    /// Specifies the indexing behavior for a property within OnePage's "query-then-filter" model,
    /// defining how the property will participate in different types of search operations during
    /// both the initial database query and the subsequent in-memory filtering phases.
    ///
    /// These options allow for efficient data retrieval and filtering, enhancing responsiveness
    /// within OnePage applications as users interact with search fields.
    /// </summary>
    /// 
    [Flags]
    public enum IndexingMode
    {
        /// <summary>
        /// Participates in the JSON blob for Query mode SQL where: LIKE '%value"%'.
        /// </summary>
        QueryLikeTerm = 0x01,

        /// <summary>
        /// Participates in the JSON blob for Query mode SQL where: LIKE '%value"%'.
        /// </summary>
        FilterLikeTerm = 0x02,

        /// <summary>
        /// Participates in the JSON blob for explicit tag queries SQL where values are 
        /// surrounded by square brackets and searched: LIKE '%[value]"%'.
        /// </summary>
        TagMatchTerm = 0x04,

        /// <summary>
        /// Combines both query phase and filter phase matching, enabling the property to
        /// support both partial retrieval from the database and broad in-memory search functionality.
        /// </summary>
        QueryOrFilter = 0x03,

        All = 0x7,
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
}
