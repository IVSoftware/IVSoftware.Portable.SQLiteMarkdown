using System.Collections.Generic;
using System.Collections.ObjectModel;

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




        public IReadOnlyList<T> CanonicalSuperset
        {
            get
            {
                if (_canonicalSuperset is null)
                {
                    _canonicalSuperset = new ReadOnlyCollection<T>(CanonicalSupersetProtected);
                }
                return _canonicalSuperset;
            }
        }
        IReadOnlyList<T>? _canonicalSuperset = null;

        /// <summary>
        /// Factory-backed canonical superset used by the back-end event pipeline 
        /// even when the visible ObservableNetProjection is filtered or divergent.
        /// </summary>
        /// <remarks>
        /// This collection represents the authoritative recordset for the current epoch.
        /// The ObservableNetProjection may expose a filtered or reordered view for UI
        /// interaction, but all structural reconciliation ultimately resolves against
        /// this canonical superset.
        /// </remarks>
        public ObservableCollection<T> CanonicalSupersetProtected
        {
            get
            {
                if (_canonicalSupersetProtected is null)
                {
                    _canonicalSupersetProtected = new ObservableCollection<T>();
                    _canonicalSupersetProtected.CollectionChanged += OnCanonicalSupersetChanged;
                }
                return _canonicalSupersetProtected;
            }
        }
        protected ObservableCollection<T>? _canonicalSupersetProtected = null;
    }
}
