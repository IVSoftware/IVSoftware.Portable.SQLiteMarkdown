using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
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
        : MarkdownContext<T>
        , IModeledMarkdownContext<T>
        where T : new()
    {
        public ModeledMarkdownContext()
        {
            CanonicalSupersetProtected = new();
            CanonicalSupersetProtected.CollectionChanged += (sender, e) => OnCanonicalSupersetChanged(e);
        }

        /// <summary>
        /// Returns the singleton, non-replaceable root XElement, created on demand.
        /// </summary>
        /// <remarks>
        /// This represents the canonical ledger.
        /// </remarks>
        public override XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = 
                        new XElement(
                            nameof(StdMarkdownElement.model),
                            new XBoundAttribute(nameof(StdMarkdownAttribute.mdc), this, $"[MMDC]"),
                            new XAttribute(nameof(StdMarkdownAttribute.autocount), 0),
                            new XAttribute(nameof(StdMarkdownAttribute.count), 0),
                            new XAttribute(nameof(StdMarkdownAttribute.matches), 0));
                    _model.Changing += (sender, e) =>
                    {
                        if (sender is XElement xel && e.ObjectChange == XObjectChange.Remove)
                        {
                            _parentsOfRemoved[xel] = xel.Parent;
                        }
                    };
                    _model.Changed += (sender, e) =>
                    {
                        switch (sender)
                        {
                            case XElement xel:
                                XElement pxel;
                                if (e.ObjectChange == XObjectChange.Remove)
                                {
                                    if (!_parentsOfRemoved.TryGetValue(xel, out pxel))
                                    {
                                        _parentsOfRemoved.ThrowSoft<NullReferenceException>(
                                            $"Expecting parent for removed XElement was cached prior." +
                                            $"Unless this throw is escalated, flow will continue with null parent.");
                                    }
                                    _parentsOfRemoved.Remove(xel);
                                }
                                else
                                {
                                    pxel = xel.Parent;
                                }
                                OnXElementChanged(xel, pxel, e);
                                break;
                            case XAttribute xattr:
                                OnXAttributeChanged(xattr, e);
                                break;
                        }
                    };
                }
                return _model;
            }
        }

#if DEBUG
        const bool SQLITE_STRICT = true;
#else
        const bool SQLITE_STRICT = false;
#endif
        [Canonical("The globally unique authority for binding items and their INPC events.")]
        protected override void OnBoundItemObjectChange(XBoundAttribute xbo, XObjectChange action)
        {
            var item = (T)xbo.Tag;
            switch (action)
            {
                case XObjectChange.Add:
                    localSetModelContainer();
                    localAddEvents();
                    localEvaluateMatch();
                    _ = localTryAddToDatabase();
                    break;
                case XObjectChange.Remove:
                    _ = localTryRemoveFromDatabase();
                    localRemoveEvents();
                    localRemoveMatch();
                    break;
            }
            #region L o c a l F x

            // Associate the xml Model governing this ddx.
            void localSetModelContainer()
            {
                if (xbo.Tag is IAffinityModel modeled)
                {
                    if (xbo.Parent is null)
                    {
                        this.ThrowFramework<NullReferenceException>(
                            "UNEXPECTED: An attribute that is added should have a parent. What was it added *to*?");
                    }
                    else
                    {
                        modeled.Model = xbo.Parent;
                    }
                }
            }

            void localAddEvents()
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged += OnItemPropertyChanged;
                }
            }

            void localRemoveEvents()
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged -= OnItemPropertyChanged;
                }
            }

            bool? localTryAddToDatabase()
            {
                bool? isSuccess = null;
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    if (SQLITE_STRICT)
                    {
                        isSuccess = 1 == FilterQueryDatabase.Insert(item);
                    }
                    else
                    {
                        isSuccess = 1 == FilterQueryDatabase.InsertOrReplace(item);
                    }
                }
                else
                {   /* G T K - N O O P */
                    // There is no filter database to maintain.
                    isSuccess = null;
                }
                if (isSuccess == false)
                {
                    this.ThrowPolicyException(SQLiteMarkdownPolicyViolation.SQLiteOperationFailed);
                }
                return isSuccess;
            }

            bool? localTryRemoveFromDatabase()
            {
                bool? isSuccess = null;
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    isSuccess = 1 == FilterQueryDatabase.Delete(item);
                }
                else
                {
                    isSuccess = null;
                }
                return isSuccess;
            }
            void localEvaluateMatch()
            {
                if(xbo.Parent is { } xel)
                {
                    var qmatch = xel.GetAttributeValue<bool>(StdMarkdownAttribute.qmatch);
                    var pmatch = xel.GetAttributeValue<bool>(StdMarkdownAttribute.pmatch);
                    if(qmatch && pmatch)
                    {
                        PredicateMatchSubsetPrivate.Add(item);
                    }
                    else
                    {
                        xel.SetAttributeValue(StdMarkdownAttribute.match, bool.FalseString);
                    }
                }
            }
            void localRemoveMatch()
            {
                PredicateMatchSubsetPrivate.Remove(item);
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// True when InputText is empty regardless of IsFiltering.
        /// </summary>
        /// <remarks>
        /// Mental Model:
        /// "If the input text is empty, just swap the handle instead of recalculating."
        /// Functional Behavior:
        /// - External predicate filters must still run even if IME doesn't contribute.
        /// - This is the purview of the subclass. Override for full control.
        /// </remarks>
        public virtual bool RouteToFullRecordset
        {
            get
            {
                if (InputText.Trim().Length == 0)
                {
                    return true;
                }
                int
                    autocount = Model.GetAttributeValue<int>(StdMarkdownAttribute.autocount, 0),
                    matches = Model.GetAttributeValue<int>(StdMarkdownAttribute.matches, 0);
                return autocount == matches;
            }
        }

        bool _routeToFullRecordset = true;

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
                        PredicateMatchSubsetPrivate.Clear();
                        Model.RemoveDescendantAttributes(StdMarkdownAttribute.match);

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
                                    xaf.SetAttributeValue(nameof(StdMarkdownAttribute.match), bool.TrueString);
                                    if (xaf.Attribute(StdMarkdownAttribute.model) is XBoundAttribute xbaModel
                                        && xbaModel.Tag is T model)
                                    {
                                        PredicateMatchSubsetPrivate.Add(model);
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

                    if (eventContext.Structural is NotifyCollectionChangedEventArgs eStructural)
                    {
                        OnModelChanged(ModelSettledEventArgs.FromNotifyCollectionChangedEventArgs(
                            reason: NotifyCollectionChangeReason.ApplyFilter,
                            e: eStructural));
                    }
                    if (eventContext.Reset is NotifyCollectionChangedEventArgs eReset)
                    {
                        OnModelChanged(eReset);
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
        /// Links or reassigns a non-canonical (presumably UI) items source to the markdown context.
        /// </summary>
        /// <remarks>
        /// Mental Model: "ItemsSource for a CollectionView with both initial query and subsequent filter refinement.
        /// - OBSERVABLE: This is an INCC object that can be tracked.
        /// - NET       : The items in this collection depend on the net result of the recordset and any state-dependent filters.
        /// - PROJECTION: Conveys that this 'filtering' produces a PCL collection, albeit one that is likely to be visible.
        ///
        /// When assigned, this context subscribes to CollectionChanged as a reconciliation sink. During
        /// refinement epochs, structural changes made against the filtered projection are absorbed into
        /// the canonical backing store so that the canon remains complete and relevant.
        ///
        /// The projection is an interaction surface, not a storage authority.
        /// Its mutations are normalized and merged into the canonical collection
        /// according to the active authority contract.
        ///
        /// Replacing this property detaches the previous projection and attaches the new one.
        /// </remarks>
        public IList? ObservableNetProjection
        {
            get => _observableProjection;
            protected set
            {
                if (!Equals(_observableProjection, value))
                {
                    // Unsubscribe INCC
                    if (_observableProjection is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_observableProjection).CollectionChanged -= OnNetProjectionCollectionChanged;
                    }

                    _observableProjection = value;

                    // Run the handler then subscribe to any subsequent changes.
                    OnNetProjectionHandleChanged();

                    // Subscribe INCC
                    if (_observableProjection is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)_observableProjection).CollectionChanged += OnNetProjectionCollectionChanged;
                    }
                }
            }
        }
        IList? _observableProjection = null;


        /// <summary>
        /// Raised when the *handle* to the ObservableNetCollection changes.
        /// </summary>
        /// <remarks>
        /// SYNCHRONOUS - Do *not* mess around. This is information we need *now* and will have to wait for.
        /// MentalMode (Query          config): "Do not track changes on this INCC."
        /// MentalMode (QueryAndFilter config): "The system must be reset to root cause in order to be stable."
        /// MentalMode (Filter         config): "The contents of the new projection must be regarded as a new canon."
        /// </remarks>
        protected virtual void OnNetProjectionHandleChanged()
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

        public CollectionChangeAuthority Authority =>
            (CollectionChangeAuthority)StateRunner.AuthorityProvider.Authority;

        protected override async Task OnEpochFinalizingAsync(EpochFinalizingAsyncEventArgs e)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Settle))
            {
                await base.OnEpochFinalizingAsync(e);
            }
        }

        /// <summary>
        /// Receives projection change notifications required to maintain the canonical ledger.
        /// </summary>
        protected virtual void OnNetProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Projection))
            {
                if(Equals(Authority, CollectionChangeAuthority.Projection))
                {
                    CanonicalSupersetProtected.Apply(e);
                }
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
        /// </remarks>
        protected virtual void OnCanonicalSupersetChanged(NotifyCollectionChangedEventArgs e)
        {
            Model.Apply(e);
            switch (Authority)
            {
                case CollectionChangeAuthority.Reset:
                case CollectionChangeAuthority.Commit:
                case CollectionChangeAuthority.Settle:
                case CollectionChangeAuthority.Predicate:
                    if(DHostBatch.TryApply(e))
                    { 
                        /* G T K - N O O P */
                        // Deferred
                    }
                    else
                    {
                        OnModelChanged(e);
                    }
                    break;
            }
        }

        public IDisposable BeginBatch() => DHostBatch.GetToken(this);
        DHostBatchCollectionChange DHostBatch
        {
            get
            {
                if (_dhostBatch is null)
                {
                    _dhostBatch = new DHostBatchCollectionChange();
                    _dhostBatch.FinalDispose += (sender, e) =>
                    {
                        if (e is BatchFinalDisposeEventArgs eFD)
                        {
                            OnModelChanged(eFD.Digest);
                        }
                    };
                }
                return _dhostBatch;
            }
        }
        DHostBatchCollectionChange? _dhostBatch = null;

        protected virtual void UpdateModelWithAuthority(object sender, NotifyCollectionChangedEventArgs e)
        {
            #region A U T H O R I T Y    G U A R D
            switch (Authority)
            {
                case CollectionChangeAuthority.Settle:
                case CollectionChangeAuthority.Projection:
                    // The players.
                    break;
                case 0:
                    this.ThrowFramework<InvalidOperationException>($"{nameof(CollectionChangeAuthority)} is required.");
                    return;
                case CollectionChangeAuthority.Reset:
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
            if (Authority == CollectionChangeAuthority.Projection)
            {
                CanonicalSupersetProtected.Clear();
                foreach (var item in Model.Descendants().Select(_ => _.To<T>()).OfType<T>())
                {
                    CanonicalSupersetProtected.Add(item);
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
        protected virtual void OnModelChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            switch (ProjectionOption)
            {
                case NetProjectionTopology.None: 
                    // N O O P
                    // There is no projection to update.
                    break;
                case NetProjectionTopology.ObservableOnly:    // Maintain internal canon but do not push internal changes.
                    ModelChanged?.Invoke(this, eBCL);
                    break;
                case NetProjectionTopology.AllowDirectChanges:
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
                            Equals(AuthorityEpochProvider.Authority, CollectionChangeAuthority.Settle),
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
                                    $"In {nameof(OnModelChanged)} Multi item moves are not supported. Override this method for full control.");
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
        }

        public event NotifyCollectionChangedEventHandler? ModelChanged;

        /// <summary>
        /// Determines whether MDC is allowed to puppeteer the projection directly.
        /// </summary>
        public NetProjectionTopology ProjectionOption { get; protected set; } = NetProjectionTopology.None;

        public ReplaceItemsEventingOption ReplaceItemsEventingOptions { get; set; } = ReplaceItemsEventingOption.StructuralReplaceEvent;

        /// <summary>
        /// Indicates that the runtime type is a subclass of MMDC.
        /// </summary>
        public bool IsInherited
        {
            get
            {
                if (_isInherited is null)
                {
                    _isInherited = GetType() != typeof(ModeledMarkdownContext<T>);
                }
                return (bool)_isInherited;
            }
        }
        bool? _isInherited = null;



        #endregion P R O J E C T I O N

        /// <summary>
        /// Overrides the BC.Clear so that the model and database can be tracked.
        /// </summary>
        protected override void OnClear(bool all)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Reset))
            {
                switch (Authority)
                {
                    default:
                    case CollectionChangeAuthority.Reset:
                        base.OnClear(all);
                        if (all)
                        {
                            CanonicalSupersetProtected.Clear();
                        }
                        break;
                    case CollectionChangeAuthority.Commit:
                        // [Careful] Don't clear InputText in this case.
                        CanonicalSupersetProtected.Clear();
                        FilteringState = FilteringState.Ineligible;
                        SearchEntryState = SearchEntryState.Cleared;
                        break;
                }
#if DEBUG
                if (all && QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    Debug.Assert(FilterQueryDatabase.Table<T>().Count() == 0);
                }
#endif
            }
        }

        /// <summary>
        /// Performs a terminal clear on the model before adding recordset if non-empty.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Establish a baseline for filtering, prioritization, and temporal projections."
        /// Changing Event:
        /// - Reset where OldItems contains the canonical list before loading.
        /// ChangedEvents:
        /// - Reset always,
        /// - Add if recordset is non-empty.
        /// </remarks>
        public virtual void LoadCanon(IEnumerable? recordset)
        {
            // Make a tmp copy of existing population.
            var oldItems = CanonicalSupersetProtected.ToArray();
            // Copy recordset, including any null values.
            var newItems = recordset.Cast<T>().ToList();

            NotifyCollectionChangingEventArgs ePre = oldItems.Diff(newItems);

            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Commit))
            {
                if (Equals(Authority, CollectionChangeAuthority.Commit))
                {
                    // This method *does* have the authority to event.
                    Clear(all: true);

                    if (newItems.Count > 0)
                    {
                        using (BeginBatch())
                        {
                            foreach (var newItem in newItems)
                            {
                                CanonicalSupersetProtected.Add(newItem);
                            }
                        }
                    }
                }
                else
                {
                    nameof(LoadCanon).ThrowHard<InvalidOperationException>("Failed authority claim.");
                }
                ExecState(StdFSMState.UpdateStatesForEpoch, recordset);
            }
#if false
            if (Equals(Authority, CollectionChangeAuthority.Commit))
            {
                using (var eventHost = Model.SetSelfRemovingXBoundAttribute(
                    StdMarkdownAttribute.triage,
                    Model.GetReplacementTriageEvents(NotifyCollectionChangeReason.QueryResult, recordset, ReplaceItemsEventingOptions)))
                {
#if false && CHECK_FAST_TRACK
                    if(Equals(FsmReservedState.FastTrack, ExecState(StdFSMState.DetectFastTrack, recordset)))
                    {
                        Debug.Fail($@"ADVISORY - First Time.");
                    }
#endif
                    ExecState(StdFSMState.ResetOrCanonizeFQBDForEpoch, (IList)recordset);
                    var diff = CanonicalSupersetProtected.Diff(recordset.Cast<T>().ToList());
                    CanonicalSupersetProtected.Apply(diff);
                    // ExecState(StdFSMState.ResetOrCanonizeModelForEpoch, recordset);

                    ExecState(StdFSMState.UpdateStatesForEpoch, recordset);


                    if (eventHost.Tag is ReplaceItemsEventingContext context)
                    {
                        if (context.Structural is NotifyCollectionChangedEventArgs eStructural)
                        {
                            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Settle))
                            {
                                OnModelChanged(eStructural);
                            }
                        }
                        if (context.Reset is NotifyCollectionChangedEventArgs eReset)
                        {
                            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Settle))
                            {
                                OnModelChanged(eReset);
                            }
                        }
                    }
                    else
                    {
                        this.ThrowFramework<NullReferenceException>($"Expecting {nameof(ReplaceItemsEventingContext)}");
                    }
                }
            }
            else
            {
                Debug.Fail($@"ADVISORY - First Time UNEXPECTED failed to gain authority.");
            }
#endif
        }

        public virtual async Task LoadCanonAsync(IEnumerable? recordset)
        {
            using var token = BeginCollectionChangeAuthority(CollectionChangeAuthority.Commit);
            if (Equals(Authority, CollectionChangeAuthority.Commit))
            {
                using (var eventHost = Model.SetSelfRemovingXBoundAttribute(
                    StdMarkdownAttribute.triage,
                    Model.GetReplacementTriageEvents(NotifyCollectionChangeReason.QueryResult, recordset, ReplaceItemsEventingOptions)))
                {
#if false && CHECK_FAST_TRACK
                    if (Equals(FsmReservedState.FastTrack, await ExecStateAsync(StdFSMState.DetectFastTrack, recordset)))
                    {
                        Debug.Fail($@"ADVISORY - First Time.");
                    }
#endif
                    await ExecStateAsync(StdFSMState.ResetOrCanonizeFQBDForEpoch, recordset);
                    await ExecStateAsync(StdFSMState.ResetOrCanonizeModelForEpoch, recordset);
                    await ExecStateAsync(StdFSMState.UpdateStatesForEpoch, recordset);

                    if (eventHost.Tag is ReplaceItemsEventingContext context)
                    {
                        if (context.Structural is NotifyCollectionChangedEventArgs eStructural)
                        {
                            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Settle))
                            {
                                OnModelChanged(eStructural);
                            }
                        }
                        if (context.Reset is NotifyCollectionChangedEventArgs eReset)
                        {
                            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Settle))
                            {
                                OnModelChanged(eReset);
                            }
                        }
                    }
                    else
                    {
                        this.ThrowFramework<NullReferenceException>($"Expecting {nameof(ReplaceItemsEventingContext)}");
                    }
                }
            }
            else
            {
                Debug.Fail($@"ADVISORY - First Time UNEXPECTED failed to gain authority.");
            }
        }

#if false && LEGACY_RUN_FSM
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
#endif
        protected virtual async Task<Enum> ExecStateAsync(Enum state, object? context = null)
        {
            return FsmReservedState.Canceled;
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
                && PlacerResult.Exists ==Model.Place(full, out var xel, PlacerMode.FindOrPartial))
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
                        var loopbackCount = FilterQueryDatabase.Table<T>().Count();
                        Debug.Assert(canon.Count == loopbackCount);
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
            if (SearchEntryState == SearchEntryState.Cleared)
            {
                if (Equals(FsmReservedState.FastTrack, RunFSM<NativeClearFSM>()))
                {   /* G T K */
                }
            }
        }

        /// <summary>
        /// Establishes the coupled invariant between the observable projection and its projection option.
        /// </summary>
        /// <remarks>
        /// - The projection and its option must be set together.
        /// ∴ They are not exposed via independent setters.
        /// - When ONP is null: 
        ///   Only non-observable modes are permitted; invalid
        ///   combinations are downgraded via advisory or rejected.
        /// - When ONP is not null:
        ///   Defaults to <see cref="NetProjectionTopology.AllowDirectChanges"/> 
        ///   unless explicitly specified. 
        /// </remarks>
        public void SetObservableNetProjection(
            ObservableCollection<T>? onp, 
            NetProjectionTopology? option = null)
        {
            ObservableNetProjection = onp;
            if (onp is null)
            {
                option ??= NetProjectionTopology.None;
                var type = GetType();

                switch (option)
                {
                    case NetProjectionTopology.None:
                        ProjectionOption = (NetProjectionTopology)option;
                        break;
                    case NetProjectionTopology.ObservableOnly:
                    case NetProjectionTopology.AllowDirectChanges:
                        this.ThrowSoft<ArgumentException>(
                            $"The value {option.ToFullKey()} is invalid when {nameof(option)} is null.");
                        break;
                    default:
                        this.ThrowHard<NotSupportedException>(
                            $"The {((NetProjectionTopology)option).ToFullKey()} case is not supported.");
                        break;
                }
            }
            else
            {
                ProjectionOption = option ??= NetProjectionTopology.AllowDirectChanges;
            }
        }

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
        public IReadOnlyList<T> CanonicalSuperset => CanonicalSupersetProtected;
        IList ITopology.CanonicalSuperset => (IList)CanonicalSuperset;

        protected ObservableCollection<T> CanonicalSupersetProtected { get; }

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
        public IReadOnlyList<T> PredicateMatchSubset
            => PredicateMatchSubsetPrivate;
        IList ITopology.PredicateMatchSubset => (IList)PredicateMatchSubset;

        public ObservableCollection<T> PredicateMatchSubsetPrivate
        {
            get
            {
                if (_predicateMatchSubsetPrivate is null)
                {
                    _predicateMatchSubsetPrivate = new ObservableCollection<T>();
                    _predicateMatchSubsetPrivate.CollectionChanged += (sender, e) =>
                    {
                        Model.SetAttributeValue(StdMarkdownAttribute.matches, _predicateMatchSubsetPrivate.Count);
                    };
                }
                return _predicateMatchSubsetPrivate;
            }
        }
        ObservableCollection<T>? _predicateMatchSubsetPrivate = null;


        ObservableCollection<T>? IModeledMarkdownContext<T>.ObservableNetProjection =>
            (ObservableCollection<T>?)ObservableNetProjection;
    }
}
