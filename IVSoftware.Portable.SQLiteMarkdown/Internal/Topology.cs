using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.StateMachine;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    [JsonObject]
    public partial class Topology<T> : MarkdownContext<T> where T : new()
    {
        public Topology()
        {
            // Self-detect the topology.
            var type = GetType();

            if (typeof(INotifyCollectionChanged).IsAssignableFrom(type)
                && type != typeof(Topology<T>))
            {
                var clearMethod = type.GetMethod(
                    nameof(Clear),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);

                if (clearMethod is not null)
                {   /* B C S - N O O P */
                    // Located a declared parameterless Clear method.
                }
                else
                {
                    nameof(MarkdownContext) // Avoid leaking the object itself as the awaited sender.
                        .Advisory(
                        $"Inherited MarkdownContext detected, but no parameterless Clear() was found. " +
                        "Clear(bool all = false) participates in the MDC filtering state machine and may not immediately empty the collection. " +
                        "If your callers expect IList-style behavior, consider implementing Clear() => Clear(true) to provide a deterministic terminal clear. " +
                        "You may also expose Clear(bool all) without a default parameter to make the stateful semantics explicit."
                    );
                }
            }
            CanonicalSupersetInternal = new();
            CanonicalSuperset = new ReadOnlyCollection<T>(CanonicalSupersetInternal);
            PredicateMatchSubsetInternal = new();
            PredicateMatchSubset = new ReadOnlyCollection<T>(PredicateMatchSubsetInternal);
            CanonicalSupersetInternal.CollectionChanging += (sender, e) =>OnCanonicalSupersetChanging(e);
            CanonicalSupersetInternal.CollectionChanged += (sender, e) =>OnCanonicalSupersetChanged(e);
        }
        public Topology(ObservableCollection<T> onp, NetProjectionOption option) : this()
        {
            SetObservableNetProjection(onp, option);
        }

        protected override void OnFilteringStateChanged()
        {
            base.OnFilteringStateChanged();
            switch (FilteringState)
            {
                case FilteringState.Ineligible:
                    break;
                case FilteringState.Armed:
                    break;
                case FilteringState.Active:
                    break;
                default:
                    break;
            }
        }
#if false
        

#if false
                    if(FilteringState == FilteringState.Ineligible)
                    {
                        // Apply hysteresis to SearchEntryState; obtain direction from prev state.
                        switch (FilteringStatePrev)
                        {
                            case FilteringState.Ineligible:
                                break;
                            case FilteringState.Armed:
                            case FilteringState.Active:
                                Debug.Assert(InputText.Length == 0, "Otherwise, we've got a problem.");

                                // IME downgrade FSM.
                                switch (SearchEntryState)
                                {
                                    case SearchEntryState.QueryCompleteWithResults:
                                        // Leave the projected items for now, while we enable a new Search.
                                        SearchEntryState = SearchEntryState.QueryEmpty;
                                        break;
                                    case SearchEntryState.QueryCompleteNoResults:
                                        // There aren't any projected items; No intermediate step is needed.
                                        SearchEntryState = SearchEntryState.Cleared;
                                        break;
                                    case SearchEntryState.QueryEmpty:
                                        SearchEntryState = SearchEntryState.Cleared;
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
#endif
#endif

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
            get => _observableNetProjection;
            protected set
            {
                if (!Equals(_observableNetProjection, value))
                {
                    // Unsubscribe INCC
                    if (_observableNetProjection is not null)
                    {
                        _observableNetProjection.CollectionChanged -= OnNetProjectionCollectionChanged;
                    }

                    _observableNetProjection = value;

                    // Run the handler then subscribe to any subsequent changes.
                    OnNetProjectionHandleChanged();

                    // Subscribe INCC
                    if (_observableNetProjection is not null)
                    {
                        _observableNetProjection.CollectionChanged += OnNetProjectionCollectionChanged;
                    }
                }
            }
        }
        ObservableCollection<T>? _observableNetProjection = null;

        protected virtual void OnNetProjectionHandleChanged() { }

        public void SetObservableNetProjection(
            ObservableCollection<T>? onp, 
            NetProjectionOption option = NetProjectionOption.AllowDirectChanges)
        {
#if false
            if (ProjectionTopology == ProjectionTopology.Inheritance)
            {
                ThrowHard<InvalidOperationException>(@"
Cannot assign ObservableNetProjection when ProjectionTopology is Inheritance.
Inherited contexts manage their projection internally.".TrimStart());
            }
#endif

            if (!ReferenceEquals(ObservableNetProjection, onp))
            {
                // Unsubscribe INCC
                if (ObservableNetProjection is not null)
                {
                    ObservableNetProjection.CollectionChanged -= OnNetProjectionCollectionChanged;
                }

                ObservableNetProjection = onp;

                // Run the handler then subscribe to any subsequent changes.
                OnNetProjectionHandleChanged();

                // Subscribe INCC
                if (ObservableNetProjection is not null)
                {
                    ObservableNetProjection.CollectionChanged += OnNetProjectionCollectionChanged;
                }
            }
        }

        protected virtual void OnNetProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (BeginCollectionChangeAuthority(ModeledCollectionChangeAuthority.Projection))
            {
                if (Equals(Authority, ModeledCollectionChangeAuthority.Projection))
                {
                    // Checking out the token gave the requested authority.
                    CanonicalSupersetInternal.Apply(e);
                }
                else
                {
                    Debug.Assert(
                        AuthorityEpoch.ReferenceCount > 1, 
                        "Suppression expected in this case; a different authority is already primary.");
                }
            }
        }

        protected virtual void OnCanonicalSupersetChanging(EventArgs eInternal)
        {
            // Use internal class.
            var e = (NotifyCollectionChangingEventArgs)eInternal;
            switch (Authority)
            {
                case ModeledCollectionChangeAuthority.Reset:
                    e.Cancel = true;    // Will raise reset when granular clearing is all finished.
                    break;
                case ModeledCollectionChangeAuthority.Commit:
                    e.Cancel = true;    // Will raise digest replace when model building is finished.
                    break;

                case FsmReserved.NoAuthority: // No authority claimed.
                case CollectionChangeAuthority.Projection:
                    Model.Apply(e);
                    if(IsFiltering)
                    {
                        PredicateMatchSubsetInternal.Apply(e);
                    }
                    break;
                case CollectionChangeAuthority.SupressAllEvents:
                    e.Cancel = true;
                    break;
                case CollectionChangeAuthority.Model:
                    e.Cancel = true;
                    break;
                case CollectionChangeAuthority.Canon:
                    Model.Apply(e);
                    PredicateMatchSubsetInternal.Apply(e);
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Authority.ToFullKey()} case is not supported.");
                    break;
            }
        }

        protected virtual void OnCanonicalSupersetChanged(NotifyCollectionChangedEventArgs e)
        {
            // The changing event was not canceled.
            OnModelChanged(CanonicalSuperset, e);
        }
        protected virtual void OnModelChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(CanonicalSuperset.Count == CanonicalSupersetInternal.Count);
            ModelChanged?.Invoke(sender, e);
            if(Equals(NetProjectionOption.AllowDirectChanges, ProjectionOption))
            {
                // Guaranteed not null by ProjectionOption getter.
                ObservableNetProjection!.Apply(e);
            }
            else
            {   /* G T K - N O O P */
            }
        }

        public event NotifyCollectionChangedEventHandler? ModelChanged;
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
        public static bool TryNormalizeTargets(
            this EventArgs eUnk,
            out NotifyCollectionChangeAction action,
            out NotifyCollectionChangeReason reason,
            out IList? newItems,
            out int newStartingIndex,
            out IList? oldItems,
            out int oldStartingIndex
            )
        {
            action = default!;
            reason = NotifyCollectionChangeReason.None;
            newItems = null;
            oldItems = null;
            newStartingIndex = -1;
            oldStartingIndex = -1;
            switch (eUnk)
            {
                case NotifyCollectionChangedEventArgs e:
                    action = (NotifyCollectionChangeAction)e.Action;
                    newItems = e.NewItems;
                    oldItems = e.OldItems;
                    newStartingIndex = e.NewStartingIndex;
                    oldStartingIndex = e.OldStartingIndex;
                    return true;

                case NotifyCollectionChangingEventArgs e:
                    action = e.Action;
                    reason = e.Reason;
                    newItems = e.NewItems;
                    oldItems = e.OldItems;
                    newStartingIndex = e.NewStartingIndex;
                    oldStartingIndex = e.OldStartingIndex;
                    return true;

                case MutableNotifyCollectionChangingEventArgs e:
                    action = e.Action;
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
            if (!eUnk.TryNormalizeTargets(
                out var action,
                out var reason,
                out var newItems,
                out var newStartingIndex,
                out var oldItems,
                out var oldStartingIndex))
            {
                eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name} case is not supported.");
                return;
            }

            int
                indexForAdd = model.GetAttributeValue<int>(StdMarkdownAttribute.autocount),
                count = model.GetAttributeValue<int>(StdMarkdownAttribute.count, 0),
                matches = model.GetAttributeValue<int>(StdMarkdownAttribute.matches);

            switch (action)
            {
                case NotifyCollectionChangeAction.Add: localAddToModel(); break;
                case NotifyCollectionChangeAction.Remove: localRemoveFromModel(); break;
                case NotifyCollectionChangeAction.Replace: localReplaceInModel(); break;
                case NotifyCollectionChangeAction.Move: localMoveInModel(); break;
                case NotifyCollectionChangeAction.Reset: localResetModel(); break;
                default:
                    eUnk.ThrowFramework<NotSupportedException>(
                            $"The {action.ToFullKey()} case is not supported.");
                    break;
            }
            switch (action)
            {
                case NotifyCollectionChangeAction.Add: localAddToModel(); break;
                case NotifyCollectionChangeAction.Remove: localRemoveFromModel(); break;
                case NotifyCollectionChangeAction.Replace: localReplaceInModel(); break;
                case NotifyCollectionChangeAction.Move: localMoveInModel(); break;
                case NotifyCollectionChangeAction.Reset: localResetModel(); break;
                default:
                    eUnk.ThrowFramework<NotSupportedException>(
                            $"The {action.ToFullKey()} case is not supported.");
                    break;
            }
            model.SetAttributeValue(nameof(StdMarkdownAttribute.count), count);
            model.SetAttributeValue(nameof(StdMarkdownAttribute.matches), matches);

            #region L o c a l F x
            void localAddToModel()
            {
                if (newItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in newItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    break;
                                case PlacerResult.Created:
                                    xel.Name = nameof(StdMarkdownElement.xitem);
                                    xel.SetBoundAttributeValue(
                                        tag: item,
                                        name: nameof(StdMarkdownAttribute.model));

                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), indexForAdd++);
                                    count++;
                                    matches++;
                                    break;
                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localRemoveFromModel()
            {
                Debug.Fail($@"ADVISORY - First Time.");
                if (oldItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in oldItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel, PlacerMode.FindOrPartial);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    if (xel.Parent is not null)
                                    {
                                        xel.Remove();
                                        count = Math.Max(0, count - 1);
                                        matches = Math.Max(0, matches - 1);
                                    }
                                    break;
                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected option is {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localReplaceInModel()
            {
                Debug.Fail($@"ADVISORY - First Time.");
                if (newItems is null || oldItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // REMOVE PHASE
                    foreach (var item in oldItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel, PlacerMode.FindOrPartial);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    if (xel.Parent is not null)
                                    {
                                        xel.Remove();
                                        count = Math.Max(0, count - 1);
                                        matches = Math.Max(0, matches - 1);
                                    }
                                    break;
                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected option is {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }

                    // ADD PHASE
                    foreach (var item in newItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    break;

                                case PlacerResult.Created:
                                    xel.Name = nameof(StdMarkdownElement.xitem);
                                    xel.SetBoundAttributeValue(
                                        tag: item,
                                        name: nameof(StdMarkdownAttribute.model));

                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), indexForAdd++);
                                    count++;
                                    matches++;
                                    break;

                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localMoveInModel()
            {
                Debug.Fail($@"ADVISORY - First Time.");
                if (oldItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    int targetIndex = newStartingIndex;

                    foreach (var item in oldItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel, PlacerMode.FindOrPartial);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), targetIndex++);
                                    break;

                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected option is {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localResetModel()
            {
                if(reason == NotifyCollectionChangeReason.None)
                {
                    model.RemoveNodes();
                }
                else
                {
                    Debug.Fail($@"ADVISORY - TODO distinguish ReplaceItemsEventingOption.");
                    model.RemoveNodes();
                }
                count = 0;
                matches = 0;
            }            
            #endregion L o c a l F x
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
            if (!eUnk.TryNormalizeTargets(
                out var action,
                out var reason,
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
                case NotifyCollectionChangeAction.Add: localAddToList(); break;
                case NotifyCollectionChangeAction.Remove: localRemoveFromList(); break;
                case NotifyCollectionChangeAction.Replace: localReplaceInList(); break;
                case NotifyCollectionChangeAction.Move: localMoveInList(); break;
                case NotifyCollectionChangeAction.Reset: localResetList(); break;
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
