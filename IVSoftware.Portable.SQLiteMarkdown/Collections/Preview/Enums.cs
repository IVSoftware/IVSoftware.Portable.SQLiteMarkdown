using IVSoftware.Portable.SQLiteMarkdown.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.Collections.Preview
{

    /// <summary>
    /// Specifies how CollectionChanging (preview) events are emitted.
    /// </summary>
    /// <remarks>
    /// - Controls whether preview events are raised per mutation 
    ///   or whether they honor <see cref="ModelDataExchangeAuthority"/> deferrals.
    /// - Does not affect CollectionChanged, which may still be deferred.
    /// </remarks>
    [NotFlags]
    public enum CollectionChangingEventingOption
    {
        /// <summary>
        /// Raises CollectionChanging immediately for each mutation.
        /// </summary>
        /// <remarks>
        /// Provides fine-grained intent signals for validation and policy.
        /// </remarks>
        Discrete,

        /// <summary>
        /// Honor <see cref="ModelDataExchangeAuthority"/> for deferrals.
        /// </summary>
        Deferred,
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
}
