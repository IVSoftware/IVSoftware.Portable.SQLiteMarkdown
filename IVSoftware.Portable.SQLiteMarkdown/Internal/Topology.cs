using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    [JsonObject]
    public partial class Topology<T> : MarkdownContext<T>
    {
        public Topology()
        {
            MMDC = this as IModeledMarkdownContext;
            CanonicalSupersetInternal = new AuthoritativeObservableCollection<T>(() => MMDC.Authority);
            CanonicalSuperset = new ReadOnlyCollection<T>(CanonicalSupersetInternal);
            PredicateMatchSubsetInternal = new();
            PredicateMatchSubset = new ReadOnlyCollection<T>(PredicateMatchSubsetInternal);
        }
        private readonly IModeledMarkdownContext? MMDC;

        protected override void OnFilteringStateChanged()
        {
            base.OnFilteringStateChanged();

            if (IsFiltering)
            {
                if (_authorityToken is null)
                {
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
                }
            }
        }
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


        #region P R O J E C T I O N

        /// <summary>
        /// Gets or sets the observable projection representing the effective
        /// (net visible) collection after markdown and predicate filtering.
        /// </summary>
        /// <remarks>
        /// Mental Model: "ItemsSource for a CollectionView with both initial query and subsequent filter refinement.
        /// - OBSERVABLE: This is an INCC object that can be tracked.
        /// - NET       : The items in this collection depend on the net result of the recordset and any state-dependent filters.
        /// - PROJECTION: Conveys that this 'filtering' produces a PCL collection, albeit one that is likely to be visible.
        ///
        /// When assigned, this context subscribes to CollectionChanged as a
        /// reconciliation sink. During refinement epochs, structural changes
        /// made against the filtered projection are absorbed into the canonical
        /// backing store so that the canon remains complete and relevant.
        ///
        /// The projection is an interaction surface, not a storage authority.
        /// Its mutations are normalized and merged into the canonical collection
        /// according to the active authority contract.
        ///
        /// Replacing this property detaches the previous projection and attaches the new one.
        ///
        /// This property is infrastructure wiring and is not intended for data binding.
        /// </remarks>
        public ObservableCollection<T>? ObservableNetProjection
        {
            get => _observableProjection;
            set
            {
                if (ProjectionTopology == ProjectionTopology.Inheritance)
                {
                    ThrowHard<InvalidOperationException>(@"
Cannot assign ObservableNetProjection when ProjectionTopology is Inheritance.
Inherited contexts manage their projection internally.".TrimStart());
                }
                else
                {
                    if (!Equals(_observableProjection, value))
                    {
                        // Unsubscribe INCC
                        if (_observableProjection is not null)
                        {
                            _observableProjection.CollectionChanged -= OnNetProjectionCollectionChanged;
                        }

                        _observableProjection = value;

                        // Run the handler then subscribe to any subsequent changes.
                        OnNetProjectionHandleChanged();

                        // Subscribe INCC
                        if (_observableProjection is not null)
                        {
                            _observableProjection.CollectionChanged += OnNetProjectionCollectionChanged;
                        }
                    }
                }
            }
        }

        protected virtual void OnNetProjectionHandleChanged() { }

        protected virtual void OnNetProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }

        ObservableCollection<T>? _observableProjection = null;
        #endregion P R O J E C T I O N
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
