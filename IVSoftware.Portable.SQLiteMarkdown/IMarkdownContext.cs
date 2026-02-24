using IVSoftware.Portable.Common.Attributes;
using SQLite;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// [assembly:InternalsVisibleTo("IVSoftware.Portable.SQLiteMarkdown.MSTest")] ;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    /// <summary>
    /// Defines the behavioral contract for a markdown-driven query/filter state controller.
    /// </summary>
    /// <remarks>
    /// Coordinates IME settlement, query execution, and filtering state
    /// transitions without owning the visible collection. Implementations
    /// are awaitable; awaiting represents completion of the current refinement epoch.
    /// </remarks>
    [Probationary("Internal during development")]
    internal interface IMarkdownContext
    {
        /// <summary>
        /// Configures query versus filter semantics and determines reachable FSM states.
        /// </summary>
        QueryFilterConfig QueryFilterConfig { get; set; }

        /// <summary>
        /// Exposes the current IME-driven search state.
        /// </summary>
        SearchEntryState SearchEntryState { get; }

        /// <summary>
        /// Exposes the current filtering eligibility and activation state.
        /// </summary>
        FilteringState FilteringState { get; }

        /// <summary>
        /// Sets the authoritative population for the current epoch.
        /// </summary>
        /// <remarks>
        /// Does not imply ownership. Cardinality and contents may influence
        /// filtering eligibility and subsequent state transitions.
        /// </remarks>
        IList Recordset { set; }

        /// <summary>
        /// Optional in-memory SQLite store used for query-mode evaluation.
        /// </summary>
        /// <remarks>
        /// Typically, filter mode queries will run in a protected FilterQueryDatabase.
        /// </remarks>
        SQLiteConnection? MemoryDatabase { get; set; }

        /// <summary>
        /// Returns an awaiter that completes when the current refinement epoch settles.
        /// </summary>
        TaskAwaiter<TaskStatus> GetAwaiter();
    }
}