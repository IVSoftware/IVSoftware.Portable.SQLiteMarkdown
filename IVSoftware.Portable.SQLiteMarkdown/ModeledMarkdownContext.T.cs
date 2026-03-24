using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.StateMachine;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static IVSoftware.Portable.SQLiteMarkdown.Internal.Extensions;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class ModeledMarkdownContext<T>
        : Topology<T>
        , IModeledMarkdownContext<T>
        where T : new()
    {
        public ModeledMarkdownContext()
        {
            Model.SetBoundAttributeValue(
                this,
                name: nameof(StdMarkdownAttribute.mmdc),
                text: $"[MMDC]");
        }
        public ModeledMarkdownContext(ObservableCollection<T> onp, NetProjectionOption option)
            : base(onp, option) { }

        SemaphoreSlim _sslimAF = new SemaphoreSlim(1, 1);
        protected override async Task ApplyFilter()
        {
            await base.ApplyFilter();

            using (DHostBusy.GetToken())
            {
                await _sslimAF.WaitAsync();
                try
                {
                    string sql;
                    IList matches = Array.Empty<object>();
                    string[] matchPaths;

                    await Task.Run(async () =>
                    {
                        PredicateMatchSubsetInternal.Clear();
                        Model.RemoveDescendantAttributes(StdMarkdownAttribute.ismatch);

                        #region F I L T E R    Q U E R Y
                        sql = ParseSqlMarkdown();
#if DEBUG
                        if (InputText == "b")
                        {
                            Debug.Assert(sql == @"
SELECT * FROM items WHERE
(FilterTerm LIKE '%b%')".TrimStart(),
                            "PROBABLY *NOT* BUGIRL - SCREENING FOR A SPURIOUS FAIL");
                        }
#endif
                        // Execute the filter query against the proxy table. The returned rows are
                        // lightweight proxy records used only to discover which canonical models
                        // satisfy the predicate. These proxy instances are not inserted into the
                        // projection; instead their paths are resolved back to the original model
                        // objects bound in the AST.
                        matches = FilterQueryDatabase.Query(ProxyType.GetSQLiteMapping(), sql);
                        #endregion F I L T E R    Q U E R Y

                        Model.SetAttributeValue(StdMarkdownAttribute.matches, (matchPaths = localGetPaths()).Length);

                        foreach (var path in matchPaths)
                        {
                            switch (Model.Place(path, out var xaf, PlacerMode.FindOrPartial))
                            {
                                case PlacerResult.Exists:
                                    xaf.SetAttributeValue(nameof(StdMarkdownAttribute.ismatch), bool.TrueString);
                                    if (xaf.Attribute(StdMarkdownAttribute.model) is XBoundAttribute xbaModel
                                        && xbaModel.Tag is T model)
                                    {
                                        PredicateMatchSubsetInternal.Add(model);
                                    }
                                    break;
                                case PlacerResult.Created:
                                    this.ThrowFramework<InvalidOperationException>($"Unexpected result for {PlacerMode.FindOrPartial.ToFullKey()}");
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (typeof(IPrioritizedAffinity).IsAssignableFrom(ProxyType))
                        {
                            await ApplyAffinities(matches);
                        }
                    });

                    var eventContext = Model.GetReplacementTriageEvents(NotifyCollectionChangeReason.ApplyFilter, matches, ReplaceItemsEventingOptions);

                    // The WDT epoch controls this!
                    Debug.Assert(Equals(Authority, CollectionChangeAuthority.Model));

                    using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Model))
                    {
                        if (eventContext.Structural is NotifyCollectionChangedEventArgs eStructural)
                        {
                            OnModelSettled(ModelChangedEventArgs.FromNotifyCollectionChangedEventArgs(
                                reason: NotifyCollectionChangeReason.ApplyFilter,
                                e: eStructural));
                        }
                        if (eventContext.Reset is NotifyCollectionChangedEventArgs eReset)
                        {
                            OnModelSettled(eReset);
                        }
                    }

#if ABSTRACT
            // EXAMPLE<model autocount="3" count="3" matches="1">
              <xitem text="312d1c21-0000-0000-0000-000000000001" model="[SelectableQFModelLTOQO]" sort="0" />
              <xitem text="312d1c21-0000-0000-0000-00000000002c" model="[SelectableQFModelLTOQO]" sort="1" />
              <xitem text="312d1c21-0000-0000-0000-00000000002e" model="[SelectableQFModelLTOQO]" sort="2" ismatch="True" />
            </model>
#endif


                    #region L o c a l F x

                    /// <summary>
                    /// Resolves the path identifiers for the matched recordset. When the proxy
                    /// implements <c>IPrioritizedAffinity</c>, paths are taken directly from
                    /// <c>FullPath</c>; otherwise the value of the mapped SQLite primary key is
                    /// used. A missing primary key mapping is treated as a framework error.
                    /// </summary>
                    string[] localGetPaths()
                    {
                        if (typeof(IPrioritizedAffinity).IsAssignableFrom(ProxyType))
                        {
                            return matches.Cast<IPrioritizedAffinity>().Select(_ => _.FullPath).ToArray();
                        }
                        else
                        {
                            if (ProxyType.GetSQLiteMapping().PK?.PropertyInfo is PropertyInfo pi)
                            {
                                return matches.Cast<object>().Select(_ => (string)pi.GetValue(_)).ToArray();
                            }
                            // Error fall-through.
                            this.ThrowHard<InvalidOperationException>();
                            return [];
                        }
                    }
                    #endregion L o c a l F x
                }
                catch (Exception ex)
                {
                    this.RethrowHard(ex);
                }
                finally
                {
                    _sslimAF.Release();
                }
            }
        }


        #region P R O J E C T I O N

        /// <summary>
        /// Raised when the *handle* to the ObservableNetCollection changes.
        /// </summary>
        /// <remarks>
        /// SYNCHRONOUS - Do *not* mess around. This is information we need *now* and will have to wait for.
        /// MentalMode (Query          config): "Do not track changes on this INCC."
        /// MentalMode (QueryAndFilter config): "The system must be reset to root cause in order to be stable."
        /// MentalMode (Filter         config): "The contents of the new projection must be regarded as a new canon."
        /// </remarks>
        protected override void OnNetProjectionHandleChanged()
        {
            if (ObservableNetProjection is IEnumerable collection && collection.Cast<object>().Any())
            {
                // Treat any non-empty projection as a new canonical recordset.
                LoadCanon(collection);
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Applies canonical reconciliation logic when the authoritative superset changes.
        /// </summary>
        /// <remarks>
        /// This handler represents the back-end mutation sink for the context. All structural
        /// changes to the canonical superset pass through here so the routed projection,
        /// filtering state, and collection notifications remain consistent with the
        /// authoritative dataset for the current epoch.
        ///
        /// Mental Model: "The canonical ledger changed. Reconcile projections and notify observers."
        /// </remarks>
        protected override void OnCanonicalSupersetChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCanonicalSupersetChanged(e);
            Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 24).Date, "Don't forget decommissioning");
            //using var authority = BeginCollectionChangeAuthority(CollectionChangeAuthority.Model);
            //if (Authority == CollectionChangeAuthority.Model)
            //{
            //    UpdateModelWithAuthority(sender, e);
            //}
            //else
            //{
            //    Debug.Fail($@"ADVISORY - First Time.");
            //}
        }

        protected virtual void UpdateModelWithAuthority(object sender, NotifyCollectionChangedEventArgs e)
        {
            #region A U T H O R I T Y    G U A R D
            switch (Authority)
            {
                case FsmReserved.NoAuthority:
                    this.ThrowFramework<InvalidOperationException>($"{nameof(CollectionChangeAuthority)} is required.");
                    return;
                case CollectionChangeAuthority.Model:
                case CollectionChangeAuthority.Projection:
                    // The players.
                    break;
                case CollectionChangeAuthority.SupressAllEvents:
                    Debug.Fail($@"ADVISORY - Explicit no authority. Is this what we really want here?.");
                    return;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {ProjectionOption.ToFullKey()} case is not supported.");
                    return;
            }
            #endregion A U T H O R I T Y    G U A R D

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems is null)
                    {
                        e.ThrowHard<InvalidOperationException>($"{nameof(e.NewItems)} cannot be null.");
                    }
                    else
                    {
                        foreach (var item in e.NewItems)
                        {
                            AddItemToModel(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug.Fail($@"ADVISORY - First Time.");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is null)
                    {
                        e.ThrowHard<InvalidOperationException>($"{nameof(e.OldItems)} cannot be null.");
                    }
                    else
                    {
                        foreach (var item in e.OldItems)
                        {
                            AddItemToModel(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    LoadCanon(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (sender is IList list && list.Count == 0)
                    {
                        // #{A665C02F-B1DE-45AE-8DAD-67775114E725}
                        if (Model.HasElements)
                        {
                            Model.RemoveAll();
                        }
                        if (SearchEntryState != SearchEntryState.Cleared)
                        {
                            SearchEntryState = SearchEntryState.Cleared;
                        }
                        if (FilteringState != FilteringState.Ineligible)
                        {
                            FilteringState = FilteringState.Ineligible;
                        }
                    }
                    else
                    {
                        LoadCanon(sender as IEnumerable);
                    }
                    break;
                default:
                    ThrowHard<NotSupportedException>($"The {e.Action.ToFullKey()} case is not supported.");
                    break;
            }

            // [Remember]
            // The projection is not authoritative
            // The model is.
            if (Equals(Authority, CollectionChangeAuthority.Projection))
            {
                CanonicalSupersetInternal.Clear();
                foreach (var item in Model.Descendants().Select(_ => _.To<T>()).OfType<T>())
                {
                    CanonicalSupersetInternal.Add(item);
                }
            }
        }

        /// <summary>
        /// Signals that the markdown model has reached a stable state following an input-driven reconciliation.
        /// </summary>
        /// <remarks>
        /// <see cref="MarkdownContext"/> does not implement <see cref="INotifyCollectionChanged"/> but
        /// is designed to support collections that do (either through inheritance or composition). 
        ///
        /// The supplied <see cref="NotifyCollectionChangedEventArgs"/> may be downcast to <c>ModelSettledEventArgs</c>. 
        /// When cast in this way, the reason for the model iteration is provided.
        ///
        /// Mental Model: "Input text has settled; the model has reconciled."
        /// </remarks>
        [Obsolete]
        protected virtual void OnModelSettled(NotifyCollectionChangedEventArgs eBCL)
        {
            throw new NotImplementedException("[Obsolete]");
#if false
            switch (ProjectionOption)
            {
                case NetProjectionOption.Inherited:         // Subclass should apply policy first, then call base.
                case NetProjectionOption.ObservableOnly:    // Maintain internal canon but do not push internal changes.
                    ModelChanged?.Invoke(this, eBCL);
                    break;
                case NetProjectionOption.AllowDirectChanges:
                    localApplyDirectChanges();
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {ProjectionOption.ToFullKey()} case is not supported.");
                    break;
            }
            void localApplyDirectChanges()
            {
                if (eBCL is not ModelSettledEventArgs eModel)
                {
                    this.ThrowFramework<InvalidOperationException>(
                        $"Insisting on {nameof(ModelSettledEventArgs)} - The pattern match just gets the cast.");
                }
                else
                {
                    if (ObservableNetProjection is not IList projection)
                    {
                        this.ThrowFramework<InvalidOperationException>(
                            $"Expecting {nameof(ObservableNetProjection)} is determined to be non-null in the ProjectionOption property getter.");
                    }
                    else
                    {
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                        // Subclass has OPTED-IN to direct changes.
                        //
                        // Every change made here will 'attempt to' raise events on that
                        // object, but we expect that collection object to apply its own
                        // suppression and instead raise eBCL when the churn has finished
                        // in response to the ModelUpdated that is about to be raised.
                        //
                        // TO THAT END this operation is wrapped in an authority whereby
                        // the ONP can tell this is taking place from the back end.
                        //
                        // [Careful]
                        // Inspecting the sender of those events is *not* an effective
                        // way in in which to determine authority because *that* collection
                        // raises *those* events, i.e., is the sender of them.
                        Debug.Assert(
                            Equals(AuthorityEpoch.Authority, CollectionChangeAuthority.Model),
                            "Expecting this operation takes place under Model authority."
                        );
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                        switch (eBCL.Action)
                        {
                            case NotifyCollectionChangedAction.Add: localAdd(); break;
                            case NotifyCollectionChangedAction.Move: localMove(); break;
                            case NotifyCollectionChangedAction.Remove: localRemove(); break;
                            case NotifyCollectionChangedAction.Replace: localReplace(); break;
                            case NotifyCollectionChangedAction.Reset: localReset(); break;
                            default:
                                this.ThrowFramework<NotSupportedException>($"The {eBCL.Action.ToFullKey()} case is not supported.");
                                break;
                        }
                    }
                    ModelChanged?.Invoke(this, eBCL);

                    #region L o c a l F x

                    void localAdd()
                    {
                        if (eBCL.NewItems is null)
                        {
                            ThrowHard<NullReferenceException>($"{nameof(eBCL.NewItems)} cannot be null.");
                        }
                        else
                        {
                            var index =
                                eBCL.NewStartingIndex == -1
                                ? projection.Count
                                : eBCL.NewStartingIndex;
                            foreach (var item in eBCL.NewItems)
                            {
                                projection.Insert(index++, item);
                            }
                        }
                    }

                    void localMove()
                    {
                        if (eBCL.NewItems is null)
                        {
                            ThrowHard<NullReferenceException>($"{nameof(eBCL.NewItems)} cannot be null.");
                        }
                        else
                        {
                            if (eBCL.NewItems.Count != 1)
                            {
                                ThrowHard<NotSupportedException>(
                                    $"In {nameof(OnModelSettled)} Multi item moves are not supported. Override this method for full control.");
                                return;
                            }
                            int oldIndex = eBCL.OldStartingIndex;
                            int newIndex = eBCL.NewStartingIndex;

                            if (oldIndex < 0 || newIndex < 0)
                            {
                                this.ThrowFramework<InvalidOperationException>(
                                    $"Expecting valid indices for {NotifyCollectionChangedAction.Move.ToFullKey()}.");
                            }

                            // Capture items first to preserve ordering for multi-item moves
                            var moved = new List<object?>();
                            foreach (var _ in eBCL.NewItems)
                            {
                                moved.Add(projection[oldIndex]);
                                projection.RemoveAt(oldIndex);
                            }

                            int insertIndex = newIndex;
                            foreach (var item in moved)
                            {
                                projection.Insert(insertIndex++, item);
                            }
                        }
                    }

                    void localRemove()
                    {
                        if (eBCL.OldItems is null)
                        {
                            ThrowHard<NullReferenceException>($"{nameof(eBCL.OldItems)} cannot be null.");
                        }
                        else
                        {
                            if (eBCL.OldStartingIndex >= 0)
                            {
                                int index = eBCL.OldStartingIndex;

                                foreach (var _ in eBCL.OldItems)
                                {
                                    projection.RemoveAt(index);
                                }
                            }
                            else
                            {
                                foreach (var item in eBCL.OldItems)
                                {
                                    projection.Remove(item);
                                }
                            }
                        }
                    }

                    void localReplace()
                    {
                        switch (eModel.Reason)
                        {
                            case NotifyCollectionChangeReason.QueryResult:
                            case NotifyCollectionChangeReason.ApplyFilter:
                            case NotifyCollectionChangeReason.RemoveFilter:
                                // Avoid Clear() here. Some observers treat Clear as a semantic reset
                                // (e.g., selection or virtualization state) rather than a sequence of
                                // removes. Replaying the individual Remove/Add operations preserves
                                // the original mutation semantics and avoids surprising state resets.
                                if (eBCL.OldItems is not null)
                                {
                                    foreach (var item in eBCL.OldItems)
                                    {
                                        projection.Remove(item);
                                    }
                                }
                                if (eBCL.NewItems is not null)
                                {
                                    foreach (var item in eBCL.NewItems)
                                    {
                                        projection.Add(item);
                                    }
                                }
                                break;
                            default:
                                // Normal BCL Replace
                                if (eBCL.OldItems is not null &&
                                    eBCL.NewItems is not null &&
                                    eBCL.OldStartingIndex >= 0)
                                {
                                    int index = eBCL.OldStartingIndex;

                                    foreach (var item in eBCL.NewItems)
                                    {
                                        projection[index++] = item;
                                    }
                                }
                                break;
                        }
                    }
                    void localReset()
                    {
                        projection.Clear();

                        // Typically this eBCL repesents an "emptying of the collection"
                        // but this is not a guarantee. If the event offers new items,
                        // take this opportunity to copy them.
                        if (eBCL.NewItems is not null)
                        {
                            Debug.Fail($@"IFD ADVISORY - First Time.");
                            foreach (var item in eBCL.NewItems)
                            {
                                projection.Add(item);
                            }
                        }
                    }
                    #endregion L o c a l F x
                }
            }
#endif
        }
        #endregion P R O J E C T I O N


        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        public virtual async Task LoadCanonAsync(IEnumerable? recordset)
            => await RunFSMAsync<LoadIsFilteringEpochFSM>(recordset);

        /// <summary>
        /// Established a new canonical model for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        public override void LoadCanon(IEnumerable? recordset)
        {
            base.LoadCanon(recordset);

#if false && SAVE
            if (DHostAuthorityEpoch.Authority == CollectionChangeAuthority.Model)
            {   /* G T K - N O O P */
            }
            else
            {
                using (var eventHost = Model.SetSelfRemovingXBoundAttribute(
                    StdMarkdownAttribute.triage,
                    Model.GetReplacementTriageEvents(NotifyCollectionChangedReason.QueryResult, recordset, ReplaceItemsEventingOptions)))
                {
                    RunFSM<LoadIsFilteringEpochFSM>(recordset);
                    if (eventHost.Tag is ReplaceItemsEventingContext context)
                    {
                        if (context.Structural is NotifyCollectionChangedEventArgs eStructural)
                        {
                            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Model))
                            {
                                OnModelSettled(eStructural);
                            }
                        }
                        if (context.Reset is NotifyCollectionChangedEventArgs eReset)
                        {
                            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Model))
                            {
                                OnModelSettled(eReset);
                            }
                        }
                    }
                    else
                    {
                        this.ThrowFramework<NullReferenceException>($"Expecting {nameof(ReplaceItemsEventingContext)}");
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Executes a declared FSM sequentially while temporarily asserting any collection-change authority required by the FSM type.
        /// </summary>
        /// <remarks>
        /// If the FSM enum <typeparamref name="TFsm"/> is decorated with
        /// <c>CollectionChangeAuthorityAttribute</c>, an authority token is claimed for the
        /// duration of the FSM execution window. This enables controlled mutation of the
        /// canonical/projection collections during state execution without leaking that
        /// authority outside the run scope.
        ///
        /// The FSM is executed deterministically by iterating the declared enum values
        /// in order and invoking <c>ExecStateAsync</c> for each state. The final state
        /// result is returned to the caller.
        ///
        /// This mechanism allows authority to behave as a dynamic capability rather than
        /// a static property of the context, constraining mutation rights to the precise
        /// interval in which the FSM is running.
        /// </remarks>
        [Probationary("This is a draft implementation that hasn't been thoroughly tested.")]
        protected async Task<Enum> RunFSMAsync<TFsm>(object? context = null) where TFsm : struct, Enum
        {
            Debug.Fail($@"ADVISORY - [Probationary].");
            IDisposable? authorityToken = null;

            if (typeof(TFsm).GetCustomAttribute<CollectionChangeAuthorityAttribute>()?.Authority is CollectionChangeAuthority authority)
            {
                authorityToken = BeginCollectionChangeAuthority(authority);
            }
            using (new TokenDisposer(authorityToken))
            {
                Enum result = FsmReservedState.None;
                // Materialize enumerable context to a stable snapshot so FSM states cannot observe multiple enumerations or deferred side effects.
                // * Reuse an incoming value that is already an object[] to avoid an unnecessary allocation.
                if (context is IEnumerable collection)
                {
                    context = collection is object[] array
                        ? array
                        : collection.Cast<object>().ToArray();
                }

                foreach (Enum state in GetDeclaredValues<TFsm>())
                {
                    // Expecting 'Next' for linear flow.
                    result = await ExecStateAsync(state, context);

                    switch (result)
                    {
                        case FsmReservedState.Canceled:
                        case FsmReservedState.FastTrack:
                        case FsmReservedState.None:
                            return result;
                        case FsmReservedState.Next:
                            break;
                        default:
                            return await localRunOOB(state, context);
                    }
                }
                return result;
            }

            #region L o c a l F x
            async Task<Enum> localRunOOB(Enum outOfBand, object? context)
            {
                Debug.Fail($@"ADVISORY - First Time.");
                int oobCurrent = 0;
                const int OOB_MAX = 100;
                while (++oobCurrent <= OOB_MAX)
                {
                    outOfBand = ExecState(outOfBand, context);

                    switch (outOfBand)
                    {
                        case FsmReservedState.Canceled:
                        case FsmReservedState.FastTrack:
                        case FsmReservedState.None:
                            return outOfBand;
                        case FsmReservedState.Next:
                            break;
                    }
                }
                return FsmReservedState.MaxOOB;
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// Executes a declared FSM sequentially while temporarily asserting any collection-change authority required by the FSM type.
        /// </summary>
        /// <remarks>
        /// If the FSM enum <typeparamref name="TFsm"/> is decorated with
        /// <c>CollectionChangeAuthorityAttribute</c>, an authority token is claimed for the
        /// duration of the FSM execution window. This enables controlled mutation of the
        /// canonical/projection collections during state execution without leaking that
        /// authority outside the run scope.
        ///
        /// The FSM is executed deterministically by iterating the declared enum values
        /// in order and invoking <c>ExecState</c> for each state. The final state
        /// result is returned to the caller.
        ///
        /// This mechanism allows authority to behave as a dynamic capability rather than
        /// a static property of the context, constraining mutation rights to the precise
        /// interval in which the FSM is running.
        /// </remarks>
        protected Enum RunFSM<TFsm>(object? context = null) where TFsm : struct, Enum
        {
            IDisposable? authorityToken = null;

            if (typeof(TFsm).GetCustomAttribute<CollectionChangeAuthorityAttribute>()?.Authority is CollectionChangeAuthority authority)
            {
                authorityToken = BeginCollectionChangeAuthority(authority);
            }
            using (new TokenDisposer(authorityToken))
            {
                Enum result = FsmReservedState.None;
                // Materialize enumerable context to a stable snapshot so FSM states cannot observe multiple enumerations or deferred side effects.
                // * Reuse an incoming value that is already an object[] to avoid an unnecessary allocation.
                if (context is IEnumerable collection)
                {
                    context = collection is object[] array
                        ? array
                        : collection.Cast<object>().ToArray();
                }

                foreach (Enum state in GetDeclaredValues<TFsm>())
                {
                    // Expecting 'Next' for linear flow.
                    result = ExecState(state, context);

                    switch (result)
                    {
                        case FsmReservedState.Canceled:
                        case FsmReservedState.FastTrack:
                        case FsmReservedState.None:
                            return result;
                        case FsmReservedState.Next:
                            break;
                        default:
                            return localRunOOB(state, context);
                    }
                }
                return result;
            }

            #region L o c a l F x
            Enum localRunOOB(Enum outOfBand, object? context)
            {
                Debug.Fail($@"ADVISORY - First Time.");
                int oobCurrent = 0;
                const int OOB_MAX = 100;
                while (++oobCurrent <= OOB_MAX)
                {
                    outOfBand = ExecState(outOfBand, context);

                    switch (outOfBand)
                    {
                        case FsmReservedState.Canceled:
                        case FsmReservedState.FastTrack:
                        case FsmReservedState.None:
                            return outOfBand;
                        case FsmReservedState.Next:
                            break;
                    }
                }
                return FsmReservedState.MaxOOB;
            }
            #endregion L o c a l F x
        }
        protected virtual async Task<Enum> ExecStateAsync(Enum state, object? context = null)
        {
            return FsmReservedState.Canceled;
        }

        protected Enum ExecState(Enum state, object? context = null)
        {
            IEnumerable<object>? canon = context as IEnumerable<object>;
            bool
                isEmptyProjection = canon?.Any() != true;
#if DEBUG
            switch (state)
            {
                case NativeClearFSM:
                    break;
                case LoadIsFilteringEpochFSM:
                    break;
            }
#endif
            switch ((StdFSMState)state)
            {
                case StdFSMState.DetectFastTrack:
                    if (Equals(localDetectFastTrack(), FsmReservedState.FastTrack))
                    {
                        return FsmReservedState.FastTrack;
                    }
                    else
                    {
                        break;
                    }
                case StdFSMState.ResetOrCanonizeFQBDForEpoch:
                    localResetOrCanonizeFQDBForEpoch();
                    break;
                case StdFSMState.ResetOrCanonizeModelForEpoch:
                    localResetOrCanonizeModelForEpoch();
                    break;
                case StdFSMState.UpdateStatesForEpoch:
                    localUpdateStatesForEpoch();
                    break;
                case StdFSMState.AddItemToModel:
                    AddItemToModel(context);
                    break;
                case StdFSMState.RemoveItemFromModel:
                    RemoveItemFromModel(context);
                    break;
                case StdFSMState.ModelSettled:
                    localRaiseModelSettled();
                    break;
                default:
                    Debug.Fail($@"ADVISORY - Unrecognized action.");
                    break;
            }
            return FsmReservedState.Next;

            #region L o c a l F x
            Enum localDetectFastTrack()
            {
                bool isEmptyProjection =
                    !(ObservableNetProjection is IEnumerable projection && projection.Cast<object>().Any());
                switch (state)
                {
                    case NativeClearFSM:
                        // If ALL are true.
                        if (SearchEntryState == SearchEntryState.Cleared
                            && !Model.HasElements
                            && isEmptyProjection)
                        {
                            return FsmReservedState.FastTrack;
                        }
                        else
                        {
                            break;
                        }
                }
                return FsmReservedState.Next;
            }

            Enum localResetOrCanonizeFQDBForEpoch()
            {
                // Check to see whether we should have a FQDB in the first place.
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    try
                    {
                        FilterQueryDatabase.RunInTransaction(() =>
                        {
                            // Ensure table exists.
                            FilterQueryDatabase.CreateTable(ContractType);
                            // Clear any entries from a pre-existing table.
                            FilterQueryDatabase.DeleteAll(ContractType.GetSQLiteMapping());
                            // [Remember]
                            // - Canonization happens via XML changes as they arrive.
                            // - N O O P
                        });
                    }
                    catch (Exception ex)
                    {
                        this.RethrowHard(ex);
                        return FsmReservedState.Canceled;
                    }
                }
                else
                {   /* G T K - N O O P */
                    // There is no FQDB to maintain in Query-Only mode.
                }
                return FsmReservedState.Next;
            }

            void localResetOrCanonizeModelForEpoch()
            {
                if (canon is not IEnumerable)
                {
                    Model.RemoveNodes(StdMarkdownAttribute.autocount, StdMarkdownAttribute.count, StdMarkdownAttribute.matches);
                    return;
                }
                else
                {
#if DEBUG
                    int nRemoved = 0;
#endif
                    Model.SetAttributeValue(StdMarkdownAttribute.count, null);
                    Model.SetAttributeValue(StdMarkdownAttribute.matches, null);

                    PropertyInfo? pk = ContractType.GetSQLiteMapping().PK?.PropertyInfo;
#if RELEASE
                Model.RemoveNodes();
#else
                    // DEBUG:
                    // Provides clarity on how the XML Changed events work on a bulk RemoveNodes.
                    #region L o c a l F x
                    void localOnXObjectChanged(object? sender, XObjectChangeEventArgs e)
                    {
                        Debug.WriteLine($@"260306.A: Removed {++nRemoved}");
                    }
                    #endregion L o c a l F x
                    using (Model.WithOnDispose(
                        onInit: (sender, e) =>
                        {
                            Model.Changed += localOnXObjectChanged;
                        },
                        onDispose: (sender, e) =>
                        {
                            Model.Changed -= localOnXObjectChanged;
                        }))
                    {
                        if (Model.HasElements)
                        {
                            Model.RemoveNodes();
                        }
                    }
#endif
                    int
                        countDistinct = 0,
                        countDuplicate = 0;

                    if (pk is null)
                    {
                        throw new NotSupportedException($"Type '{ContractType.Name}' has no PK and such types are not (yet) supported.");
                    }
                    foreach (var item in canon)
                    {
                        // ToDo: Test with item.GetFullPath() extension.
                        var placerResult = Model.Place(path: localGetFullPath(pk, item), out var xel);

                        switch (placerResult)
                        {
                            case PlacerResult.Exists:
                                countDuplicate++;
                                break;
                            case PlacerResult.Created:
                                xel.Name = nameof(StdMarkdownElement.xitem);
                                xel.SetBoundAttributeValue(
                                    tag: item,
                                    name: nameof(StdMarkdownAttribute.model));
                                xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), countDistinct);
                                countDistinct++;
                                break;
                            default:
                                this.ThrowFramework<NotSupportedException>(
                                    $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                break;
                        }
                    }

                    Model.SetAttributeValue(StdMarkdownAttribute.count, countDistinct);
                    if (Model.GetAttributeValue<IList?>(StdMarkdownAttribute.predicates) is { } predicates)
                    {
                        Debug.Fail($@"ADVISORY - First Time.");
                        Model.SetAttributeValue(StdMarkdownAttribute.matches, countDistinct); // This will change.
                    }
                    else
                    {
                        Model.SetAttributeValue(StdMarkdownAttribute.matches, countDistinct);
                    }
                    Model.SetAttributeValue(StdMarkdownAttribute.ismatch, null);
                }
            }

            string localGetFullPath(PropertyInfo pk, object unk)
            {
                if (pk.GetValue(unk)?.ToString() is { } id && !string.IsNullOrWhiteSpace(id))
                {
                    return id;
                }
                else
                {
                    ThrowHard<NullReferenceException>(
                        $"Expecting a non-empty value for PrimaryKey '{pk.Name}'.");
                    return null!;
                }
            }

            void localUpdateStatesForEpoch()
            {
                switch (state)
                {
                    case NativeClearFSM:
                        SearchEntryState = SearchEntryState.Cleared;
                        FilteringState = FilteringState.Ineligible;
                        return;
                    default:
                        break;
                }
                switch (CanonicalCount)
                {
                    case 0:
                        SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                        FilteringState = FilteringState.Ineligible;
                        break;
                    case 1:
                        SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                        FilteringState = FilteringState.Ineligible;
                        break;
                    default:
                        SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                        switch (QueryFilterConfig)
                        {
                            case QueryFilterConfig.Query:
                            case QueryFilterConfig.Filter:
                            default:
                                FilteringState = FilteringState.Ineligible;
                                break;
                            case QueryFilterConfig.QueryAndFilter:
                                FilteringState = FilteringState.Armed;
                                break;
                        }
                        break;
                }
            }

            void localRaiseModelSettled()
            {
                var e = context as ModelChangedEventArgs
                    ?? new ModelChangedEventArgs(
                        reason: NotifyCollectionChangeReason.None,
                        action: NotifyCollectionChangedAction.Reset);
                OnModelSettled(e);
            }
            #endregion L o c a l F x
        }


        void AddItemToModel(object? item)
        {
            if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
            {
                int
                    indexForAdd = Model.GetAttributeValue<int>(StdMarkdownAttribute.autocount),
                    countB4 = Model.GetAttributeValue<int>(StdMarkdownAttribute.count, 0),
                    matchesB4 = Model.GetAttributeValue<int>(StdMarkdownAttribute.matches);

                var placerResult = Model.Place(full, out var xel);
                switch (placerResult)
                {
                    case PlacerResult.Exists:
                        break;
                    case PlacerResult.Created:
                        xel.Name = nameof(StdMarkdownElement.xitem);
                        xel.SetBoundAttributeValue(
                            tag: item,
                            name: nameof(StdMarkdownAttribute.model));

                        xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), indexForAdd);
                        Model.SetAttributeValue(nameof(StdMarkdownAttribute.count), ++countB4);
                        Model.SetAttributeValue(nameof(StdMarkdownAttribute.matches), ++matchesB4);
                        break;
                    default:
                        this.ThrowFramework<NotSupportedException>(
                            $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                        break;
                }
            }
            else
            {
                ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
            }
        }
        void RemoveItemFromModel(object? item)
        {
            if (item.GetFullPath() is { } full
                && !string.IsNullOrWhiteSpace(full)
                && PlacerResult.Exists == Model.Place(full, out var xel, PlacerMode.FindOrPartial))
            {
                Debug.Fail($@"ADVISORY - First Time.");
            }
            else ThrowHard<NullReferenceException>("Expecting object exists.");
        }
        protected override void OnCommit(RecordsetRequestEventArgs e)
        {
            base.OnCommit(e);
            if (!e.Handled)
            {
                if (e.CanonicalSuperset is null)
                {
                    if (MemoryDatabase is not null)
                    {
                        var canon = MemoryDatabase.Query(ContractType.GetSQLiteMapping(), e.SQL);
                        LoadCanon(canon);

#if DEBUG
                        if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                        {
                            var loopbackCount = FilterQueryDatabase.Table<T>().Count();
                            Debug.Assert(canon.Count == loopbackCount);
                        }
#endif
                    }
                }
                else
                {
                    LoadCanon(e.CanonicalSuperset);
                }
            }
        }

        protected override void OnSearchEntryStateChanged()
        {
            base.OnSearchEntryStateChanged();

            switch (SearchEntryState)
            {
                case SearchEntryState.Cleared:
                    if (Equals(FsmReservedState.FastTrack, RunFSM<NativeClearFSM>()))
                    {   /* G T K */
                    }
                    break;
            }
        }
    }
}
