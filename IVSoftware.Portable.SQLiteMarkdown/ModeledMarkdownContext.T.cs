using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class ModeledMarkdownContext<T> : MarkdownContextBase<T>, IModeledMarkdownContext
    {
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

        public new IReadOnlyList<T> CanonicalSuperset
            => (IReadOnlyList<T>)base.CanonicalSuperset;

        protected new ObservableCollection<T> CanonicalSupersetProtected
            => (ObservableCollection<T>)base.CanonicalSupersetProtected;
    }
}
