using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.StateRunner.Preview;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.Collections;
using IVSoftware.Portable.Xml.Linq.Collections.Internal;
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

            if (typeof(INotifyCollectionChanged).IsAssignableFrom(GetType()))
            {
                ProjectionTopology = NetProjectionTopology.Routed;
                var type = GetType();
                bool hasParameterlessClear = type.GetMethod(nameof(Clear), Type.EmptyTypes) is not null;
                bool hasBooleanClear = type.GetMethod(nameof(Clear), [typeof(bool)]) is not null;
                
                if(!(hasParameterlessClear && hasBooleanClear))
                {
                    this.ThrowPolicyException(MarkdownContextPolicyViolation.ExplicitClearAdvisory);
                }
            }
        }

        protected override void OnXElementChanged(XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
            // Update histogram first.
            base.OnXElementChanged(xel, pxel, e);

            // Now: IFTTT on the stable histogram population.
            switch (e.ObjectChange)
            {
                case XObjectChange.Add:
                    // 260230 AFAIK
                    // - An XElement coming online with attributes already
                    //   populated is a test-only phenomenon.
                    // - It needs to be robust regardless.
                    // - Ordinarily, however, the IFTTT happens when attributes come
                    //   and go on an XElement that's already wired for the events.
                    foreach (var xattr in xel.Attributes())
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdModelAttribute std)
                            && std.GetCustomAttribute<IFTTTAttribute>() is not null)
                        {
                            switch (std)
                            {
                                case StdModelAttribute.model when xattr is XBoundAttribute xba && xba.Tag is T itemT:
                                    if (PredicateMatchSubsetProtected.Contains(itemT))
                                    {   /* G T K - N O O P */
                                    }
                                    else
                                    {
                                        Debug.Fail($@"ADVISORY - First Time UNEXPECTED just confirm.");
                                        PredicateMatchSubsetProtected.Add(itemT);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case XObjectChange.Remove:
                    // [Remember] The node has been removed so no XObject changes. We need to call the actions manually.
                    foreach (var xattr in xel.Attributes())
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdModelAttribute std)
                            && std.GetCustomAttribute<IFTTTAttribute>() is not null)
                        {
                            switch (std)
                            {
                                case StdModelAttribute.model when xattr is XBoundAttribute xba && xba.Tag is T itemT:
                                    OnXBoundAttributeChanged(xba: xba, e.ObjectChange);
                                    break;
                            }
                        }
                    }
                    break;
                case XObjectChange.Value:
                    break;
            }
        }

        /// <summary>
        /// Central model authority for IFTTT.
        /// </summary>
        /// <remarks>
        /// - The itemT field is allowed to be null, especially in bare metal testing.
        /// - Its absence is considered normal, not even advisory.
        /// </remarks>
        protected override void OnXAttributeChanged(XAttribute xattr, XElement pxel, XObjectChangeEventArgs e)
        {
            T? itemT = pxel.To<T?>();
            bool? value;
            base.OnXAttributeChanged(xattr, pxel, e);
            if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdModelAttribute std))
            {
                switch (std)
                {
                    case StdModelAttribute.match:
                        value = bool.TryParse(xattr.Value, out var valid) ? valid : null;
                        switch (e.ObjectChange)
                        {
                            case XObjectChange.Add:
                                if (value == true && itemT is T)
                                {
                                    PredicateMatchSubsetProtected.Add(itemT);
                                }
                                break;
                            case XObjectChange.Remove:
                                PredicateMatchSubsetProtected.Remove(itemT);
                                break;
                            case XObjectChange.Value:
                                switch (value)
                                {
                                    case null:
                                        // The value isn't null, but isn't parseable to bool either.
                                        Debug.Fail($@"ADVISORY 260330 - Proposed validation attribute for Histo should make this unreachable.");
                                        /* G T K - N O O P */
                                        break;
                                    case true:
                                        PredicateMatchSubsetProtected.Add(itemT);
                                        break;
                                    case false:
                                        PredicateMatchSubsetProtected.Remove(itemT);
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
        }
#if DEBUG
        const bool SQLITE_STRICT = true;
#else
        const bool SQLITE_STRICT = false;
#endif
        [Canonical("The globally unique authority for binding items and their INPC events.")]
        protected override void OnXBoundAttributeChanged(XBoundAttribute xba, XObjectChange action)
        {
            var itemT = (T)xba.Tag;
            switch (action)
            {
                case XObjectChange.Add:
                    localSetModelContainer();
                    localAddEvents();
                    if(localTryAddToDatabase() == true)
                    {
                        // Do we need to add to PMSS manually here?
                    }
                    if(Authority == CollectionChangeAuthority.Projection)
                    {
                        if( QueryFilterConfig.HasFlag(QueryFilterConfig.Filter) 
                            && FilteringState == FilteringState.Active)
                        {
                            xba.Parent.SetStdModelAttributeValue(StdModelAttribute.live, bool.TrueString);
                        }
                    }
                    break;
                case XObjectChange.Remove:
                    if(localTryRemoveFromDatabase() == true)
                    {
                        PredicateMatchSubsetProtected.Remove(itemT);
                    }
                    localRemoveEvents();
                    break;
            }
            #region L o c a l F x

            // Associate the xml Model governing this ddx.
            void localSetModelContainer()
            {
                if (xba.Tag is IAffinityModel modeled)
                {
                    if (xba.Parent is null)
                    {
                        this.ThrowFramework<NullReferenceException>(
                            "UNEXPECTED: An attribute that is added should have a parent. What was it added *to*?");
                    }
                    else
                    {
                        modeled.Model = xba.Parent;
                    }
                }
            }

            void localAddEvents()
            {
                if (itemT is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged += OnItemPropertyChanged;
                }
            }

            void localRemoveEvents()
            {
                if (itemT is INotifyPropertyChanged inpc)
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
                        isSuccess = 1 == FilterQueryDatabase.Insert(itemT);
                    }
                    else
                    {
                        isSuccess = 1 == FilterQueryDatabase.InsertOrReplace(itemT);
                    }
                }
                else
                {   /* G T K - N O O P */
                    // There is no filter database to maintain.
                    isSuccess = null;
                }
                if (isSuccess == false)
                {
                    this.ThrowPolicyException(MarkdownContextPolicyViolation.SQLiteOperationFailed);
                }
                return isSuccess;
            }

            bool? localTryRemoveFromDatabase()
            {
                bool? isSuccess = null;
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    isSuccess = 1 == FilterQueryDatabase.Delete(itemT);
                }
                else
                {
                    isSuccess = null;
                }
                return isSuccess;
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
                switch (FilteringState)
                {
                    case FilteringState.Ineligible:
                    case FilteringState.Armed:
                        return true;
                    case FilteringState.Active:
                        if (0 == Histo[StdModelAttribute.match])
                        {
                            // The collection is eligible for filtering (has at least two items).
                            // All items have been filtered out.
                            // ∴ The list will be ambiguously empty UNLESS
                            // -  UseAdaptiveShowAll will display *all* items instead of *no* items.
                            return Equals(Settings[StdMarkdownContextSetting.UseAdaptiveShowAll], true);
                        }
                        else return false;
                    default:
                        this.ThrowFramework<NotSupportedException>($"The {FilteringState.ToFullKey()} case is not supported.");
                        return true;
                }
            }
        }

        bool _routeToFullRecordset = true;

        SemaphoreSlim _sslimAF = new SemaphoreSlim(1, 1);
        protected override async Task ApplyFilter()
        {
            using (DHostBusy.GetToken())
            {
                await _sslimAF.WaitAsync();
                await base.ApplyFilter();
                try
                {
                    using (RequestModelEpochAuthority(ModelDataExchangeAuthority.ModelDeferred, Read))
                    {
                        string sql;
                        IList matches = Array.Empty<object>();
                        string[] matchPaths;

                        await Task.Run(async () =>
                        {
                            PredicateMatchSubsetProtected.Clear();
                            Model.RemoveDescendantAttributes(
                                [
                                    StdModelAttribute.match,
                                StdModelAttribute.pmatch,
                                StdModelAttribute.qmatch,
                                ]);

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

                            if (matches.Count == 0 && Equals(Settings[StdMarkdownContextSetting.AllowPluralize], true))
                            {
                                sql = sql.ToFuzzyQuery();
                                matches = FilterQueryDatabase.Query(ProxyType.GetSQLiteMapping(), sql);
                            }
                            #endregion F I L T E R    Q U E R Y

                            matchPaths = localGetPaths();

                            foreach (var path in matchPaths)
                            {
                                switch (Model.Place(path, out var xaf, PlacerMode.FindOrPartial))
                                {
                                    case PlacerResult.Exists:
                                        // IFTTT - the XObject.Change will add this to PMSS.
                                        xaf.SetAttributeValue(nameof(StdModelAttribute.qmatch), bool.TrueString);
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
        protected override void OnInputTextChanged()
        {
            switch (Authority)
            {
                case CollectionChangeAuthority.Reset:
                    /* G T K - N O O P */
                    return;
                default:
                    break;
            }
            base.OnInputTextChanged();
            switch (FilteringState)
            {
                case FilteringState.Armed:
                    // One of:
                    // - IME [X] clear
                    // - A backspace in Filter mode results in an empty entry text field.
                    // Stay in filtering mode but the UI visuals might change e.g. icon glyph and/or color.
                    if (FilteringStatePrev == FilteringState.Active)
                    {
                        NotifyCollectionChangedEventArgs? ePost = null;
                        if (ReplaceItemsEventingOptions.HasFlag(ReplaceItemsEventingOption.StructuralReplaceEvent))
                        {
                            ePost = 
                                ((IList)PredicateMatchSubset)
                                .Diff((IList)CanonicalSuperset,
                                reason: NotifyCollectionChangeReason.RemoveFilter);
                            OnModelSettled(ePost);
                        }
                        if (ReplaceItemsEventingOptions.HasFlag(ReplaceItemsEventingOption.ResetOnAnyChange))
                        {
                            if (ePost?.Action != NotifyCollectionChangedAction.Reset)
                            {
                                ePost = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset);
                            }
                            OnModelSettled(ePost);
                        }
                    }
                    break;
            }
        }

        protected override void OnFilteringStateChanged()
        {
            base.OnFilteringStateChanged();
            if(FilteringState == FilteringState.Ineligible)
            {
                PredicateMatchSubsetProtected.Clear();
            }
        }

        #region C H A N G E    O N P    H A N D L E    O R    I T E M S
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
        #endregion C H A N G E    O N P    H A N D L E    O R    I T E M S

        /// <summary>
        /// Execute asynchronous work in the settlement phase of the BC WDT epoch.
        /// </summary>
        /// <remarks>
        /// Any subclass override should check out its own Settle authority
        /// token and perform all work under its auspices.
        /// </remarks>
        protected override async Task OnEpochFinalizingAsync(EpochFinalizingAsyncEventArgs e)
        {            
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Settle))
            {
                await base.OnEpochFinalizingAsync(e);
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
        protected virtual void OnCanonicalSupersetChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            switch (Authority)
            {
                // Mental Model: "When does the Model require an update?"
                case CollectionChangeAuthority.None:        // When the IList interface of an MMDC is invoked programmatically.
                case CollectionChangeAuthority.Reset:       // When an unconditional global clear is taking place.
                case CollectionChangeAuthority.Commit:      // When the model is being fully displaced by a new canonical recordset.
                case CollectionChangeAuthority.Projection:  // When [+] or [🗑] actions (buttons) operate on the visible surface directly.
                    // Call the Apply extension on Model.
                    Model.Apply(eBCL);

                    // Check the model authority here, not inside
                    // the model settled virtual method.
                    switch (DHostModelEpoch.Authority)
                    {
                        case ModelDataExchangeAuthority.Collection:
                        case ModelDataExchangeAuthority.Model:
                            OnModelSettled(eBCL);
                            break;
                        case ModelDataExchangeAuthority.CollectionDeferred:
                        case ModelDataExchangeAuthority.ModelDeferred:
                            if(DHostModelEpoch.IsDisposing)
                            {
                                OnModelSettled(eBCL);
                            }
                            break;
                        default:
                            this.ThrowFramework<NotSupportedException>($"The {DHostModelEpoch.Authority.ToFullKey()} case is not supported.");
                            break;
                    }
#if false
                    switch (Authority)
                    {
                        // Mental Model: "When does the Model require an update?"
                        case CollectionChangeAuthority.None:        // When the IList interface of an MMDC is invoked programmatically.
                        case CollectionChangeAuthority.Reset:       // When an unconditional global clear is taking place.
                        case CollectionChangeAuthority.Commit:      // When the model is being fully displaced by a new canonical recordset.
                        case CollectionChangeAuthority.Projection:  // When [+] or [🗑] actions (buttons) operate on the visible surface directly.
                            break;
                        // Mental Model: "When does the Model *not* require an update?"
                        case CollectionChangeAuthority.Settle:      // The IME text has settled and deferred relitigation of
                                                                    // 'qmatch' and 'match' attributes is proceeding.
                        case CollectionChangeAuthority.Predicate:   // A filter has been toggled and immediate relitigation of
                                                                    // 'pmatch' and 'match' attributes is proceeding.
                            switch (ProjectionTopology)
                            {
                                case NetProjectionTopology.None:
                                    // N O O P
                                    // There is no projection to update.
                                    break;
                                case NetProjectionTopology.ObservableOnly:    // Maintain internal canon but do not push internal changes out to projection.
                                    break;
                                case NetProjectionTopology.AllowDirectChanges:
                                    if (ObservableNetProjection is null)
                                    {
                                        this.ThrowHard<NullReferenceException>($"Expecting not null is baked into {ProjectionTopology.ToFullKey()}");
                                    }
                                    else
                                    {
                                        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                        // - Subclass has OPTED-IN to direct changes from this model.
                                        // - Subclass is listening for changes, and not pushing them.
                                        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                        using (BeginAuthority(ModelDataExchangeAuthority.ModelDeferred))
                                        {
                                            OnModelSettled(e);
                                        }
                                    }
                                    break;
                                case NetProjectionTopology.Routed:
                                    OnModelSettled(e);
                                    break;
                                default:
                                    ThrowFramework<NotSupportedException>($"The {ProjectionTopology.ToFullKey()} case is not supported.");
                                    break;
                            }
                            break;
                        default:
                            this.ThrowHard<NotSupportedException>($"The {Authority.ToFullKey()} case is not supported.");
                            break;
                    }
#endif
                    break;
                default:

                    this.ThrowHard<NotSupportedException>($"The {Authority.ToFullKey()} case is not supported.");
                    break;
            }
        }

        public IDisposable RequestModelEpochAuthority(ModelDataExchangeAuthority authority, IList source)
            => DHostModelEpoch.GetToken(authority, source);
        ModelDataExchangeAuthorityProvider<T> DHostModelEpoch
        {
            get
            {
                if (_DHostModelEpoch is null)
                {
                    _DHostModelEpoch = new ModelDataExchangeAuthorityProvider<T>();
                    _DHostModelEpoch.FinalDispose += (sender, e) =>
                    {
                        if (e is ModelDataExchangeFinalDisposeEventArgs eFD)
                        {
                            OnModelSettled(eFD.Digest);
                        }
                    };
                }
                return _DHostModelEpoch;
            }
        }
        ModelDataExchangeAuthorityProvider<T>? _DHostModelEpoch = null;

        protected virtual void UpdateModelWithAuthority(object sender, NotifyCollectionChangedEventArgs e)
        {
            #region A U T H O R I T Y    G U A R D
            switch (Authority)
            {
                case CollectionChangeAuthority.Settle:
                case CollectionChangeAuthority.Projection:
                    // The players.
                    break;
                case CollectionChangeAuthority.Reset:
                    Debug.Fail($@"ADVISORY - Explicit no authority. Is this what we really want here?.");
                    return;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {ProjectionTopology.ToFullKey()} case is not supported.");
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

        bool _reentry = false;

        /// <summary>
        /// Signals that the markdown model has reached a stable state following an input-driven reconciliation.
        /// </summary>
        /// <remarks>
        /// <see cref="MarkdownContext"/> does not implement <see cref="INotifyCollectionChanged"/> but
        /// is designed to support collections that do (either through inheritance or composition). 
        ///
        /// The supplied <see cref="NotifyCollectionChangedEventArgs"/> may be downcast to <c>ModelSettledEventArgs</c>. 
        /// When cast in this way, the reason for the model iteration is provided.
        /// </remarks>
        protected virtual void OnModelSettled(EventArgs eUnk)
        {
            if(_reentry)
            {
                // This isn't being relied on, but possibly could work standalone!
                Debug.Fail($@"ADVISORY - First Time.");
                return;
            }
            else
            {
                try
                {
                    _reentry = true;
                    if (Authority != CollectionChangeAuthority.Projection)
                    {
                        ObservableNetProjection?.Apply(eUnk);
                    }
                    ModelSettled?.Invoke(this, eUnk);
                }
                finally
                {
                    _reentry = false;
                }
            }
        }
        public event EventHandler? ModelSettled;

        /// <summary>
        /// Determines whether MDC is allowed to puppeteer the projection directly.
        /// </summary>
        /// <remarks>
        /// The CTor checks for:
        /// - If runtime subclass implements INotifyCollectionChanged.
        /// ∴ Is inherently read-write.
        /// ∴ Cannot simultaneously host an ObservableNetProjection.
        /// ∴ Employs routing, not copying, for the IsFiltering epoch.
        /// - Then the NetProjectionTopology.Routed assigned, and is immutable once set.
        /// Manual Assignment:
        /// - If NetProjectionTopology.Routed is *not* assigned in ctor
        ///   then it cam be set using the SetObservableNetProjection method.
        /// </remarks>
        public NetProjectionTopology ProjectionTopology { get; protected set; } = NetProjectionTopology.None;

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
            if (all)
            {
                using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Reset))
                {
                    switch (Authority)
                    {
                        default:
                        case CollectionChangeAuthority.Reset:
                            // - Model does not answer to authority 
                            // ∴ Database will update.
                            Model.RemoveNodes();
                            // Call the base *with* reset authority.
                            base.OnClear(all);
                            CanonicalSupersetProtected.Clear();
                            break;
                        case CollectionChangeAuthority.Commit:
                            // Moved this management to LoadCanon because
                            // we were just suppressing the events anyway.
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
            else
            {
                // Call the base *without* reset authority.
                base.OnClear(all);
            }
        }

        /// <summary>
        /// Performs a terminal clear on the model before adding recordset if non-empty.
        /// </summary>
        /// <remarks>
        /// Mental Model: "Establish a baseline for filtering, prioritization, and temporal projections."
        /// ChangedEvents: 1. 'Reset' always 2. 'Add' if recordset is non-empty.
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
                    // This method *does* have the authority to raise TWO events.
                    // First event: Reset
                    CanonicalSupersetProtected.Clear();
                    // SecondEvent: Add (digest) on Final batch dispose.
                    if (newItems.Count > 0)
                    {
                        using (RequestModelEpochAuthority(ModelDataExchangeAuthority.ModelDeferred, Read))
                        {
                            foreach (var newItem in newItems)
                            {
                                CanonicalSupersetProtected.Add(newItem);
                            }
                        }
                    }

                    // 260404 - Critical for ModelDataExchange paradigms.
                    // This needs to be staged now, because DHostModelEpoch takes out
                    // a token at the start of ApplyFilter and uses Read to
                    // make a snapshot of the source, which now points to PMSS.
                    PredicateMatchSubsetProtected.Clear();
                    foreach (var item in CanonicalSuperset)
                    {
                        PredicateMatchSubsetProtected.Add(item);
                    }
                    UpdateStatesForEpoch();
                }
                else
                {
                    nameof(LoadCanon).ThrowHard<InvalidOperationException>("Failed authority claim.");
                }
            }
        }

        public virtual async Task LoadCanonAsync(IEnumerable? recordset)
        {
            IList oldItems = null!, newItems = null!;
            await Task.Run(() =>
            {
                // Make a tmp copy of existing population.
                oldItems = CanonicalSupersetProtected.ToArray();
                // Copy recordset, including any null values.
                newItems = recordset.Cast<T>().ToList();

                NotifyCollectionChangingEventArgs ePre = oldItems.Diff(newItems);
            });

            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Commit))
            {
                if (Equals(Authority, CollectionChangeAuthority.Commit))
                {
                    // This method *does* have the authority to raise TWO events.
                    // First event: Reset
                    CanonicalSupersetProtected.Clear();

                    // SecondEvent: Add (digest) on Final batch dispose.
                    if (newItems.Count > 0)
                    {
                        using (RequestModelEpochAuthority(ModelDataExchangeAuthority.ModelDeferred, Read))
                        {
                            await Task.Run(() =>
                            {
                                foreach (var newItem in newItems)
                                {
                                    CanonicalSupersetProtected.Add((T)newItem);
                                }
                            });
                        }
                    }
                    UpdateStatesForEpoch();
                }
                else
                {
                    nameof(LoadCanon).ThrowHard<InvalidOperationException>("Failed authority claim.");
                }
            }
        }

        void UpdateStatesForEpoch()
        {
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

        void AddItemToModel(object? item)
        {            
            if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
            {
                int indexForAdd = Histo[StdModelAttribute.model];

                var placerResult = Model.Place(full, out var xel);
                switch (placerResult)
                {
                    case PlacerResult.Exists:
                        break;
                    case PlacerResult.Created:
                        xel.Name = nameof(StdModelElement.item);
                        xel.SetBoundAttributeValue(
                            tag: item,
                            name: nameof(StdModelAttribute.model));
                        xel.SetAttributeValue(nameof(StdModelAttribute.order), indexForAdd);
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
                IList? canon;
                localQuery(false);

                if (canon is { Count: 0 })
                {
                    if (Equals(Settings[StdMarkdownContextSetting.AllowPluralize], true))
                    {
                        localQuery(true);
                    }
                    else
                    {   /* G T K - N O O P */
                    }
                }

                // Unconditional: Might be null, None or Some.
                LoadCanon(canon);

#if DEBUG
                int loopbackCount = FilterQueryDatabase.Table<T>().Count();
                Debug.Assert(canon?.Count == loopbackCount);

                var count = this.Count;
                var enumerator = ((IList)this).GetEnumerator();
                var cssCount = CanonicalSuperset.Count;
#endif
                #region L o c a l F x
                void localQuery(bool fuzzy)
                {
                    var sql = fuzzy ? e.SQL.ToFuzzyQuery() : e.SQL;
                    if (e.CanonicalSuperset is null && MemoryDatabase is not null)
                    {
                        canon = MemoryDatabase.Query(ContractType.GetSQLiteMapping(), sql);
                    }
                    else
                    {
                        canon = e.CanonicalSuperset;
                    }
                }
                #endregion L o c a l F x
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
            NetProjectionTopology? topology = null)
        {
            if(topology == NetProjectionTopology.Routed)
            {
                this.ThrowHard<ArgumentException>(
                    $"{NetProjectionTopology.Routed.ToFullKey()} is runtime-inferred and cannot be assigned explicitly.");
                topology = null;
            }
            ObservableNetProjection = onp;
            if (onp is null)
            {
                topology ??= NetProjectionTopology.None;
                var type = GetType();

                switch (topology)
                {
                    case NetProjectionTopology.None:
                        ProjectionTopology = (NetProjectionTopology)topology;
                        break;
                    case NetProjectionTopology.ObservableOnly:
                    case NetProjectionTopology.AllowDirectChanges:
                        this.ThrowHard<ArgumentException>(
                            $"The value {topology.ToFullKey()} is invalid when {nameof(topology)} is null.");
                        break;
                    default:
                        this.ThrowHard<NotSupportedException>(
                            $"The {((NetProjectionTopology)topology).ToFullKey()} case is not supported.");
                        break;
                }
            }
            else
            {
                switch (ProjectionTopology)
                {
                    case NetProjectionTopology.Routed:
                        this.ThrowHard<NotSupportedException>(
    $"Cannot assign an observable projection when {nameof(ProjectionTopology)} is {NetProjectionTopology.Routed}");
                        break;
                    case NetProjectionTopology.None:
                    case NetProjectionTopology.ObservableOnly:
                    case NetProjectionTopology.AllowDirectChanges:
                        ProjectionTopology = topology ??= NetProjectionTopology.AllowDirectChanges;
                        break;
                    default:
                        this.ThrowHard<NotSupportedException>($"The {ProjectionTopology.ToFullKey()} case is not supported.");
                        break;
                }
            }
        }

        #region A U T H O R I T Y
        public IDisposable BeginCollectionChangeAuthority(CollectionChangeAuthority authority)
        {
            switch (authority)
            {
                case CollectionChangeAuthority.Settle:
                case CollectionChangeAuthority.Predicate:
                    ICollection snapshot;
                    if(ObservableNetProjection is null)
                    {
                        // Diff compares against the iteration prior to changes.
                        snapshot = Read.Cast<T>().ToArray();
                    }
                    else
                    {
                        // Diff compares against the ONP.
                        snapshot = ObservableNetProjection.Cast<T>().ToArray();
                    }            
                    return CollectionChangeAuthorityProvider.BeginAuthority(authority, snapshot);
                default:
                    return CollectionChangeAuthorityProvider.BeginAuthority(authority);
            }
        }

        public CollectionChangeAuthority Authority => CollectionChangeAuthorityProvider.Authority;

        AuthorityEpochProvider<CollectionChangeAuthority> CollectionChangeAuthorityProvider { get; } = new();
        #endregion A U T H O R I T Y

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

        public XElement Model => CanonicalSupersetProtected.Model;

        public ObservableModeledCollection<T> CanonicalSupersetProtected
        {
            get => _canonicalSupersetProtected;
            set
            {
                if (value is null)
                {
                    this.ThrowHard<InvalidOperationException>(
                        $"{nameof(CanonicalSupersetProtected)} cannot be set to null.");
                }
                else
                {
                    if (!Equals(_canonicalSupersetProtected, value))
                    {
                        if(_canonicalSupersetProtected is not null)
                        {
                            _canonicalSupersetProtected.CollectionChanged -= (sender, e) =>
                            {
                                OnCanonicalSupersetChanged(e);
                            };
                        }
                        _canonicalSupersetProtected = value;
                        _canonicalSupersetProtected.CollectionChanged += (sender, e) =>
                        {
                            OnCanonicalSupersetChanged(e);
                        };
                        OnPropertyChanged();
                    }
                }
            }
        }
        ObservableModeledCollection<T> _canonicalSupersetProtected = null!;    // Initialized in CTor.

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
        {
            get
            {
                if(_predicateMatchSubset is null)
                {
                    if(PredicateMatchSubsetProtected is IList<T> listT)
                    {
                        _predicateMatchSubset = new ReadOnlyCollection<T>(listT);
                    }
                    else
                    {
                        this.ThrowFramework<InvalidOperationException>(
                            $"Expecting {nameof(PredicateMatchSubsetProtected)} is initialized using {nameof(ObservableCollection<T>)}");
                        _predicateMatchSubset = new List<T>();
                    }
                }
                return _predicateMatchSubset;
            }
        }
        private IReadOnlyList<T> _predicateMatchSubset = null!;
        IList ITopology.PredicateMatchSubset => (IList)PredicateMatchSubset;

        public IList PredicateMatchSubsetProtected
        {
            get
            {
                if (_predicateMatchSubsetProtected is null)
                {
                    // 260329 - Observable for debug convenience only at this time.
                    var opc = new ObservablePreviewCollection<T>(eventScope: NotifyCollectionChangeScope.CancelOnly);
                    opc.CollectionChanging += (sender, e) =>
                    {
                        Debug.WriteLine($"260330.A {nameof(PredicateMatchSubsetProtected)}.{e.Action} Count={PredicateMatchSubsetProtected.Count}");
                        switch (e.Action)
                        {
                            case NotifyCollectionChangeAction.Add:
                                // First, cancel if Filter mode is not enabled.
                                if (!(e.Cancel = !QueryFilterConfig.HasFlag(QueryFilterConfig.Filter)))
                                {
                                    // If not canceled then check this edge case.
                                    if (e.NewItems?[0] is null)
                                    {   /* G T K */
                                        // This is a valid state (apparently) that arises in test when 
                                        // the 'match' attribute goes true on a node without an 'model'
                                        e.Cancel = true;
                                    }
                                }

                                break;
                            case NotifyCollectionChangeAction.Remove:
                                break;
                            case NotifyCollectionChangeAction.Reset:
                                break;
                        }
                    };
                    opc.CollectionChanged += (sender, e) =>
                    { };
                    _predicateMatchSubsetProtected = opc;
                }
                return _predicateMatchSubsetProtected;
            }
        }
        IList? _predicateMatchSubsetProtected = null;

        ObservableCollection<T>? IModeledMarkdownContext<T>.ObservableNetProjection =>
            (ObservableCollection<T>?)ObservableNetProjection;
    }
}
