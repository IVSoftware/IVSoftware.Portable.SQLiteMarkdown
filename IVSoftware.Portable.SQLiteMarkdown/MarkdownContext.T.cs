using System.Collections.Generic;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class MarkdownContext<T> : MarkdownContext
    {
        /// <summary>
        /// Creates a typed context whose base infrastructure is initialized
        /// with the element type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// The base <see cref="MarkdownContext"/> constructs its internal
        /// collections using the supplied element type. As a result, the
        /// predicate-match subset is guaranteed to be backed by a
        /// <c>List&lt;T&gt;</c> (exposed as <see cref="IList"/> in the base
        /// contract). The generic context therefore only projects a typed
        /// read-only view and does not need to enforce the type at runtime.
        /// </remarks>
        public MarkdownContext() : base(typeof(T)) { }

        /// <summary>
        /// Provides a typed, read-only view of the predicate-match subset.
        /// </summary>
        /// <remarks>
        /// The underlying collection is created by the base context using
        /// the element type supplied at construction. This property simply
        /// re-exposes that collection as <see cref="IReadOnlyList{T}"/>.
        /// Structural changes performed by the infrastructure remain visible
        /// through this view.
        /// </remarks>
        public new IReadOnlyList<T> PredicateMatchSubset
            => (IReadOnlyList<T>)base.PredicateMatchSubset;
    }
}
