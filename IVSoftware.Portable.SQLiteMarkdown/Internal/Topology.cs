using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    [JsonObject]
    partial class Topology<T> : MarkdownContext<T>
    {
        public Topology(IModeledMarkdownContext mmdc, ObservableCollection<T>? projection = null)
        {
            MMDC = mmdc;
            mmdc.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(IsFiltering):
                        IsFiltering = mmdc.IsFiltering;
                        break;
                }
            };
            CanonicalSupersetInternal = new AuthoritativeObservableCollection<T>(() => MMDC.Authority);
            CanonicalSuperset = new ReadOnlyCollection<T>(CanonicalSupersetInternal);
            PredicateMatchSubsetInternal = new();
            PredicateMatchSubset = new ReadOnlyCollection<T>(PredicateMatchSubsetInternal);
            ObservableNetProjection = projection;
        }
        private readonly IModeledMarkdownContext MMDC;
        public XElement Model => MMDC.Model;

        public bool IsFiltering
        {
            get => _isFiltering;
            set
            {
                if (!Equals(_isFiltering, value))
                {
                    if (value)
                    {
                        if (_authorityToken is null)
                        {
                            _isFiltering = true;
                            _authorityToken = MMDC.BeginCollectionChangeAuthority(CollectionChangeAuthority.Model);
                        }
                        else
                        {
                            this.ThrowHard<InvalidOperationException>(
                                "Filtering state invariant violated: token already present on enter.");
                        }
                    }
                    else
                    {
                        if (_authorityToken is null)
                        {
                            this.ThrowHard<InvalidOperationException>(
                                "Filtering state invariant violated: token missing on exit.");
                        }
                        else
                        {
                            var tmp = _authorityToken;
                            _authorityToken = null;
                            tmp.Dispose();
                            _isFiltering = false;
                        }
                    }
                }
            }
        }
        bool _isFiltering = false;
        IDisposable? _authorityToken = null;

        internal IReadOnlyList<T> Read =>
            IsFiltering
            ? PredicateMatchSubset
            : CanonicalSuperset;


        /// <summary>
        /// If present, this external IList must be kept in sync with the net filtered result.
        /// </summary>
        /// <remarks>
        /// To eliminate churn, user may inherit from <see cref="AuthoritativeObservableCollection{T}AuthoritativeObservableCollection"/>
        /// </remarks>
        public ObservableCollection<T>? ObservableNetProjection { get; }
        public IReadOnlyList<T> CanonicalSuperset { get; }

        /// <summary>
        /// Provides the authoritative mutable superset that backs all collection operations.
        /// </summary>
        /// <remarks>
        /// - All write operations are applied to this collection under enforced
        ///   <see cref="CollectionChangeAuthority"/>.
        /// - When <see cref="IsFiltering"/> is active or when a sorted projection is in effect,
        ///   index-based operations are mediated through the model to resolve positions
        ///   relative to the currently visible items.
        /// - This collection is not used as the read surface; enumeration is routed through
        ///   the active view (see <c>Read</c>), which selects between the canonical superset
        ///   and the predicate-matched subset.
        /// - <see cref="AuthoritativeObservableCollection{T}"/> suppresses redundant
        ///   <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/>
        ///   emissions when changes originate from the model and are mirrored into the
        ///   <c>ObservableNetCollection</c>, preventing feedback loops since projection
        ///   updates ultimately target this canonical superset.
        /// </remarks>
        internal AuthoritativeObservableCollection<T> CanonicalSupersetInternal { get; }

        /// <summary>
        /// Exposes the current predicate-matched subset as a stable read-only view.
        /// </summary>
        public IReadOnlyList<T> PredicateMatchSubset { get; }

        /// <summary>
        /// Stores the mutable backing list for the predicate-matched subset.
        /// </summary>
        /// <remarks>
        /// - Rebuilt by the model during reconciliation based on the current filter state.
        /// - Treated as an ephemeral projection snapshot, not a source of truth.
        /// - Not exposed for external mutation; contents are fully controlled by the model.
        /// - Updates are applied as a single settled snapshot; intermediate churn is suppressed
        ///   and observers are notified via the model’s ModelSettled event.
        /// </remarks>
        internal List<T> PredicateMatchSubsetInternal { get; }
    }
}
