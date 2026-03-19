using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    [JsonObject]
    public partial class Topology<T> : MarkdownContext<T>
    {
        public Topology()
        {
            // Self-detect the topology.
            // - RTTI claims INotifyCollectionChange implementation.
            // - This class, however, doesn't do that on its own.
            // - Inheritance is the only possibility.
            _isInherited = typeof(INotifyCollectionChanged).IsAssignableFrom(GetType());
            if (_isInherited)
            {
                if (this.GetType().GetMethod(nameof(Clear), Type.EmptyTypes) is { } clearMethod)
                {   /* B C S - N O O P */
                }
                else
                {
                    // Avoid leaking the object itself as the awaited sender.
                    nameof(MarkdownContext).Advisory(
                        $"Inherited MarkdownContext detected, but no parameterless Clear() was found. " +
                        "Clear(bool all = false) participates in the MDC filtering state machine and may not immediately empty the collection. " +
                        "If your callers expect IList-style behavior, consider implementing Clear() => Clear(true) to provide a deterministic terminal clear. " +
                        "You may also expose Clear(bool all) without a default parameter to make the stateful semantics explicit."
                    );
                }
            }
            CanonicalSupersetInternal = new ();
            CanonicalSuperset = new ReadOnlyCollection<T>(CanonicalSupersetInternal);
            PredicateMatchSubsetInternal = new();
            PredicateMatchSubset = new ReadOnlyCollection<T>(PredicateMatchSubsetInternal);
            CanonicalSupersetInternal.CollectionChanging += OnCanonicalSupersetChanging;
            CanonicalSupersetInternal.CollectionChanged += OnCanonicalSupersetChanged;
        }

        protected override void OnFilteringStateChanged()
        {
            base.OnFilteringStateChanged();

            if (IsFiltering)
            {
                if (_authorityToken is null)
                {
                    _authorityToken = BeginCollectionChangeAuthority(CollectionChangeAuthority.Model);
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

        private void OnCanonicalSupersetChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            switch (Authority)
            {
                case 0:
                    // Explicitly allowed.
                    e.Cancel = false;
                    break;
                case CollectionChangeAuthority.None:
                    e.Cancel = true;
                    break;
                case CollectionChangeAuthority.Model:
                    e.Cancel = true;
                    break;
                case CollectionChangeAuthority.Projection:
                    e.Cancel = true;
                    Model.Apply(e);
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Authority.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected virtual void OnCanonicalSupersetChanged(object sender, NotifyCollectionChangedEventArgs e) 
        {
            Debug.Assert(CanonicalSuperset.Count == CanonicalSupersetInternal.Count);
        }

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
        internal ObservablePreviewCollection<T> CanonicalSupersetInternal { get; }

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
    static class ChangeEventExtensions
    {
        /// <summary>
        /// Normalizes supported collection change EventArgs into a unified action and payload.
        /// </summary>
        /// <remarks>
        /// - Supports NotifyCollectionChangedEventArgs and internal changing variants.
        /// - Outputs canonical action, items, and indices for downstream application.
        /// - Returns false only after throwing for unsupported inputs.
        /// </remarks>
        public static bool TryNormalizeTargets<T>(
            this EventArgs eUnk,
            out T action,
            out IList? newItems,
            out int newStartingIndex,
            out IList? oldItems,
            out int oldStartingIndex
            )
            where T : Enum
        {
            action = default!;
            newItems = null;
            oldItems = null;
            newStartingIndex = -1;
            oldStartingIndex = -1;
            var type = typeof(T);
            if (type != typeof(NotifyCollectionChangedAction) &&
                type != typeof(NotifyCollectionChangingAction))
            {
                nameof(ChangeEventExtensions)
                    .ThrowFramework<NotSupportedException>(
                        $"The enum type '{type.Name}' is not supported. " +
                        $"Expected one of: {nameof(NotifyCollectionChangedAction)}, {nameof(NotifyCollectionChangingAction)}.");
                return false;
            }
            switch (eUnk)
            {
                case NotifyCollectionChangedEventArgs e:
                    action = (T)(object)e.Action;
                    newItems = e.NewItems;
                    oldItems = e.OldItems;
                    newStartingIndex = e.NewStartingIndex;
                    oldStartingIndex = e.OldStartingIndex;
                    return true;

                case NotifyCollectionChangingEventArgs e:
                    action = (T)(object)e.Action;
                    newItems = e.NewItems;
                    oldItems = e.OldItems;
                    newStartingIndex = e.NewStartingIndex;
                    oldStartingIndex = e.OldStartingIndex;
                    return true;

                case MutableNotifyCollectionChangingEventArgs e:
                    action = (T)(object)e.Action;
                    newItems = e.NewItems;
                    oldItems = e.OldItems;
                    newStartingIndex = e.NewStartingIndex;
                    oldStartingIndex = e.OldStartingIndex;
                    return true;
                default:
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>($"The {eUnk.GetType().Name} case is not supported.");
                    return false;
            }
        }

        /// <summary>
        /// Applies a normalized collection change to the XML model.
        /// </summary>
        /// <remarks>
        /// - Delegates structural updates to local handlers based on action.
        /// - Consumes normalized payload (items, indices) without reinterpreting EventArgs.
        /// - Intended as the authoritative model mutation entry point.
        /// </remarks>
        public static void Apply(this XElement model, EventArgs eUnk)
        {
            if (!eUnk.TryNormalizeTargets<NotifyCollectionChangingAction>(
                out var action,
                out var newItems,
                out var newStartingIndex,
                out var oldItems,
                out var oldStartingIndex))
            {
                nameof(ChangeEventExtensions)
                    .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name} case is not supported.");
                return;
            }

            switch (action)
            {
                case NotifyCollectionChangingAction.Add: localAddToModel(); break;
                case NotifyCollectionChangingAction.Remove: localRemoveFromModel(); break;
                case NotifyCollectionChangingAction.Replace: localReplaceInModel(); break;
                case NotifyCollectionChangingAction.Move: localMoveInModel(); break;
                case NotifyCollectionChangingAction.Reset: localResetModel(); break;
                default:
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                            $"The {action.ToFullKey()} case is not supported.");
                    break;
            }

            void localAddToModel()
            {
                if (newItems is null)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in newItems)
                    {
                    }
                }
            }

            void localRemoveFromModel()
            {
                if (oldItems is null)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in oldItems)
                    {
                    }
                }
            }

            void localReplaceInModel()
            {
                if (newItems is null || oldItems is null)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in oldItems)
                    {
                    }
                    foreach (var item in newItems)
                    {
                    }
                }
            }

            void localMoveInModel()
            {
                if (oldItems is null)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in oldItems)
                    {
                    }
                }
            }

            void localResetModel()
            {
            }
        }

        /// <summary>
        /// Applies a normalized collection change to a list target.
        /// </summary>
        /// <remarks>
        /// - Mirrors model application semantics for IList-backed projections.
        /// - Uses normalized action and payload for consistent mutation behavior.
        /// - Serves as the projection-side execution counterpart to model updates.
        /// </remarks>
        public static void Apply(this IList list, EventArgs eUnk)
        {
            if (!eUnk.TryNormalizeTargets<NotifyCollectionChangingAction>(
                out var action,
                out var newItems,
                out var newStartingIndex,
                out var oldItems,
                out var oldStartingIndex))
            {
                nameof(ChangeEventExtensions)
                    .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name} case is not supported.");
                return;
            }

            switch (action)
            {
                case NotifyCollectionChangingAction.Add: localAddToList(); break;
                case NotifyCollectionChangingAction.Remove: localRemoveFromList(); break;
                case NotifyCollectionChangingAction.Replace: localReplaceInList(); break;
                case NotifyCollectionChangingAction.Move: localMoveInList(); break;
                case NotifyCollectionChangingAction.Reset: localResetList(); break;
                default:
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name} case is not supported.");
                    break;
            }
            void localAddToList()
            {
                if (newItems is null || newStartingIndex < 0)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    var index = newStartingIndex;
                    foreach (var item in newItems)
                    {
                        list.Insert(index++, item);
                    }
                }
            }

            void localRemoveFromList()
            {
                if (oldItems is null || oldStartingIndex < 0)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // Remove at index repeatedly (items shift left)
                    for (int i = 0; i < oldItems.Count; i++)
                    {
                        list.RemoveAt(oldStartingIndex);
                    }
                }
            }

            void localMoveInList()
            {
                if (oldItems is null || oldStartingIndex < 0 || newStartingIndex < 0)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // Preserve order of moved block
                    var buffer = new object[oldItems.Count];
                    for (int i = 0; i < oldItems.Count; i++)
                    {
                        buffer[i] = list[oldStartingIndex];
                        list.RemoveAt(oldStartingIndex);
                    }

                    var insertIndex = newStartingIndex;
                    foreach (var item in buffer)
                    {
                        list.Insert(insertIndex++, item);
                    }
                }
            }

            void localReplaceInList()
            {
                if (newItems is null || oldItems is null || newStartingIndex < 0)
                {
                    nameof(ChangeEventExtensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // Replace in place
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        list[newStartingIndex + i] = newItems[i];
                    }
                }
            }

            void localResetList()
            {
                list.Clear();
            }
        }
    }
}
