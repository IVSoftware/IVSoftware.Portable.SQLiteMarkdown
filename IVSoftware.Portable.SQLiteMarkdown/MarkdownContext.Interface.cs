using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{

    partial class MarkdownContext : IMarkdownContext
    {
        /// <summary>
        /// Returns the singleton, non-replaceable root XElement, created on demand.
        /// </summary>
        /// <remarks>
        /// This represents the canonical ledger.
        /// </remarks>
        public XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new XElement(nameof(StdMarkdownElement.model));
                    _model.Changing += (sender, e) =>
                    {
                        if(sender is XElement xel && e.ObjectChange == XObjectChange.Remove)
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
                                    if(!_parentsOfRemoved.TryGetValue(xel, out pxel))
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
        XElement? _model = null;

        Dictionary<XElement, XElement> _parentsOfRemoved = new();

        protected virtual void OnXAttributeChanged (XAttribute xattr, XObjectChangeEventArgs e) 
        {
            if(xattr is XBoundAttribute xbo && xbo.Tag.GetType() == ContractType)
            {
                OnBoundItemObjectChange(xbo, e.ObjectChange);
            }
        }

        protected virtual void OnXElementChanged (XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
            switch (e.ObjectChange)
            {
                case XObjectChange.Add:
                case XObjectChange.Remove:
                    var xbo =
                        xel
                        .Attributes()
                        .OfType<XBoundAttribute>()
                        .FirstOrDefault(_ => _.Tag?.GetType() == ContractType);
                    if(xbo is not null)
                    {
                        OnBoundItemObjectChange(xbo, e.ObjectChange);
                    }
                    break;
            }
        }

#if DEBUG
        const bool SQLITE_STRICT = true;
#else
        const bool SQLITE_STRICT = false;
#endif
        protected virtual void OnBoundItemObjectChange(XBoundAttribute xbo, XObjectChange action)
        {
            var item = xbo.Tag;
            if (SQLITE_STRICT)
            {
                switch (action)
                {
                    case XObjectChange.Add:
                        if(xbo.Tag is IAffinityModel modeled)
                        {
                            if(xbo.Parent is null)
                            {
                                this.ThrowFramework<NullReferenceException>(
                                    "UNEXPECTED: An attribute that is added should have a parent. What was it added *to*?");
                            }
                            else
                            {
                                modeled.Model = xbo.Parent;
                            }
                        }
                        if (1 != FilterQueryDatabase.Insert(item))
                        {
                            Debug.Fail($@"ADVISORY - Expecting operation to succeed.");
                        }
                        break;
                    case XObjectChange.Remove:
                        FilterQueryDatabase.Delete(item);
                        break;
                }
            }
            else
            {
                switch (action)
                {
                    case XObjectChange.Add:
                        if (1 != FilterQueryDatabase.InsertOrReplace(item))
                        {

                            Debug.Fail($@"ADVISORY - Expecting operation to succeed.");
                        }
                        break;
                    case XObjectChange.Remove:
                        FilterQueryDatabase.Delete(item);
                        break;
                }
            }
        }

        protected async Task<Enum> RunFSMAsync<TFsm>(object? context = null) where TFsm : struct, Enum
        {
            Enum result = ReservedAffinityState.None;
            foreach (Enum state in GetDeclaredValues<TFsm>())
            {
                result = await ExecStateAsync(state, context);
            }
            return result;
        }

        protected virtual async Task<Enum> ExecStateAsync(Enum state, object? context = null)
        {
            return ReservedAffinityState.Canceled;
        }

        /// <summary>
        /// Enumerates the values of the enum type <typeparamref name="TFsm"/> in their
        /// declaration order rather than their underlying numeric order. This is used
        /// by the FSM runner to evaluate states exactly in the sequence authored in
        /// the enum definition.
        /// </summary>
        protected IEnumerable<TFsm> GetDeclaredValues<TFsm>() where TFsm : Enum
        {
            return typeof(TFsm)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Select(field => (TFsm)field.GetValue(null)!);
        }

        /// <summary>
        /// Executes each state in the enum <typeparamref name="TFsm"/> in declaration
        /// order, invoking <c>ExecState</c> for each. Normal progression continues while
        /// <see cref="ReservedAffinityState.Next"/> is returned. Any other result
        /// transitions the runner into an out-of-band loop for the current state until
        /// <see cref="ReservedAffinityState.None"/> is returned, after which execution
        /// terminates and the last result is returned.
        /// </summary>
        protected Enum RunFSM<TFsm>(object? context = null) where TFsm : struct, Enum
        {
            Enum result = ReservedAffinityState.None;
            foreach (Enum state in GetDeclaredValues<TFsm>())
            {
                // Expecting 'Next' for linear flow.
                result = ExecState(state, context);

                if(!Equals(result, ReservedAffinityState.Next))
                {
                    Enum outOfBand;
                    while (!Equals(ReservedAffinityState.None, (outOfBand = ExecState(state, context)))) { }
                    break;
                }
            }
            return result;
        }

        protected Enum ExecState(Enum state, object? context = null)
        {
            switch ((StdFSMState)state)
            {
                case StdFSMState.InitFQBDForEpoch when context is IEnumerable canonical:
                    localInitFQDBEpoch(canonical);
                    break;
                case StdFSMState.InitModelForEpoch when context is IEnumerable canonical:
                    localInitModelForEpoch(canonical);
                    break;
                case StdFSMState.InitStatesForEpoch:
                    localInitStatesForEpoch();
                    break;
                case StdFSMState.ResetFQBDForEpoch:
                    localResetFQBDForEpoch();
                    break;
                case StdFSMState.ResetModelForEpoch:
                    localResetModelForEpoch();
                    break;
            }
            return ReservedAffinityState.Next;

            #region L o c a l F x
            Enum localInitFQDBEpoch(IEnumerable canonical)
            {
                try
                {
                    FilterQueryDatabase.RunInTransaction(() =>
                    {
                        FilterQueryDatabase.DeleteAll(ContractType.GetMapping());
                        FilterQueryDatabase.CreateTable(ContractType);
                    });
                }
                catch (Exception ex)
                {
                    this.RethrowHard(ex);
                    return ReservedAffinityState.Canceled;
                }
                return ReservedAffinityState.Next;
            }

            Enum localInitModelForEpoch(IEnumerable canonical)
            {
                Model.SetAttributeValue(StdMarkdownAttribute.count, null);
                Model.SetAttributeValue(StdMarkdownAttribute.matchCount, null);

                PropertyInfo? pk = ContractType.GetMapping().PK?.PropertyInfo;
#if DEBUG
                #region L o c a l F x
                void localOnXObjectChanged(object? sender, XObjectChangeEventArgs e)
                {
                    Debug.Fail($@"ADVISORY - Yes, this DOES raise events.");
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
                    Model.RemoveNodes();
                }
#else
                Model.RemoveNodes();
#endif
                int
                    countDistinct = 0,
                    countDuplicate = 0;

                if (pk is null)
                {
                    throw new NotSupportedException($"Type '{ContractType.Name}' has no PK and such types are not (yet) supported.");
                }
                foreach (var item in canonical)
                {
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
                Model.SetAttributeValue(StdMarkdownAttribute.ismatch, null); ;
                return ReservedAffinityState.Next;
            }

            string localGetFullPath(PropertyInfo pk, object unk)
            {
                if (pk.GetValue(unk)?.ToString() is { } id && !string.IsNullOrWhiteSpace(id))
                {
                    return id;
                }
                else
                {
                    this.ThrowHard<NullReferenceException>(
                        $"Expecting a non-empty value for PrimaryKey '{pk.Name}'.");
                    return null!;
                }
            }

            void localInitStatesForEpoch()
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

            void localResetFQBDForEpoch()
            {
                if (FilterQueryDatabase.DeleteAll(ContractType.GetMapping()) == 0)
                {   /* G T K */
                }
                else
                {   /* G T K */
                }
            }

            void localResetModelForEpoch()
            {
                Model.RemoveNodes();
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// The ephemeral backing store for this collection's contract filtering.
        /// </summary>
        /// <remarks>
        /// - Like any other SQLite database this can be configured with N tables.
        ///   However, the semantic constraints on contract parsing (where ContractType 
        ///   is assumed to be the item type of the collection that subclasses it) will
        ///   provide an advisory stream should this be called upon to service more
        ///   that the implicit single table for the collection.
        /// </remarks>
        protected SQLiteConnection FilterQueryDatabase
        {
            get
            {
                // HYBRID - factory getter.
                if (_filterQueryDatabase is null)
                {
                    _filterQueryDatabase = new SQLiteConnection(":memory:");
                    // ContractType is set at construction and cannot be null.
                    _filterQueryDatabase.CreateTable(ContractType);
                }
                return _filterQueryDatabase;
            }
            set
            {
                if (!Equals(_filterQueryDatabase, value))
                {
                    _filterQueryDatabase = value;
                    if(_filterQueryDatabase is not null)
                    {
                        _filterQueryDatabase.CreateTable(ContractType);
                    }
                    OnPropertyChanged();
                    this.OnAwaited();
                }
            }
        }

        SQLiteConnection? _filterQueryDatabase = default;

#if false
        private bool TryCreateTableForContractType()
        {
            if( _filterQueryDatabase is not null 
                && ContractType?.GetConstructor(Type.EmptyTypes) is not null)
            {
                ContractTypeTableMapping = _filterQueryDatabase.GetMapping(ContractType);
                _filterQueryDatabase.CreateTable(ContractType);
                return true;
            }
            else
            {
                return false;
            }
        }
        public TableMapping ContractTypeTableMapping
        {
            get => _contractTypeTableMapping;
            set
            {
                if (!Equals(_contractTypeTableMapping, value))
                {
                    _contractTypeTableMapping = value;
                    OnPropertyChanged();
                }
            }
        }
        TableMapping _contractTypeTableMapping = default;
#endif

        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        public virtual async Task LoadCanonAsync(IEnumerable? recordset)
            => await RunFSMAsync<InitFilterEpochFSM>(recordset);

        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        public virtual void LoadCanon(IEnumerable? recordset)
        {
            recordset ??= Array.Empty<object>();
            using (DHostBusy.GetToken())
            {
                RunFSM<InitFilterEpochFSM>(recordset);
            }
        }

        /// <summary>
        /// Determines whether MDC is allowed to puppeteer the projection directly.
        /// </summary>
        internal NetProjectionOption ProjectionOptions { get; set; } = NetProjectionOption.AllowDirectChanges;

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
        public INotifyCollectionChanged? ObservableNetProjection
        {
            get => _observableProjection;
            set
            {
                if (!Equals(_observableProjection, value))
                {
                    // Unsubscribe INCC
                    if (_observableProjection is not null)
                    {
                        _observableProjection.CollectionChanged -= OnObservableProjectionCollectionChanged;
                    }

                    _observableProjection = value;

                    // [Careful] This is safest when MDC is first in line.
                    ProjectionTopology = _observableProjection switch
                    {
                        MarkdownContext _ => ProjectionTopology.Inheritance,
                        INotifyCollectionChanged _ and not MarkdownContext
                            => ProjectionTopology.Composition,
                        _ => ProjectionTopology.None,
                    };

                    // Run the handler then subscribe to any subsequent changes.
                    OnObservableProjectionChanged();

                    // Subscribe INCC
                    if (_observableProjection is not null)
                    {
                        _observableProjection.CollectionChanged += OnObservableProjectionCollectionChanged;
                    }
                }
            }
        }
        INotifyCollectionChanged? _observableProjection = null;


        /// <summary>
        /// Raised when the handle to the ObservableNetCollection changes.
        /// </summary>
        /// <remarks>
        /// SYNCHRONOUS - Do *not* mess around. This is information we need *now* and will have to wait for.
        /// MentalMode (Query          config): "Do not track changes on this INCC."
        /// MentalMode (QueryAndFilter config): "The system must be reset to root cause in order to be stable."
        /// MentalMode (Filter         config): "The contents of the new projection must be regarded as a new canon."
        /// </remarks>
        protected virtual void OnObservableProjectionChanged()
        {
            LoadCanon(ObservableNetProjection as IEnumerable);
        }

        public ProjectionTopology ProjectionTopology { get; protected set; }

        /// <summary>
        /// Raised when the collection - that is the ObservableNetProjection - is modified in some way.
        /// </summary>
        protected virtual void OnObservableProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    LoadCanon(sender as IEnumerable);
                    break;
                default:
                    break;
            }
        }

        [Obsolete("Version 2.0+ uses clearer semantics: CanonicalCount and PredicateMatchCount.")]
        [PublishedSignature("1.0")] // Required for backward compatibility. Do not remove this property.
        public int UnfilteredCount 
        {
            get => CanonicalCount;
            protected set => Model.SetAttributeValue(StdMarkdownAttribute.count, value);
        }

        public int CanonicalCount => Model.GetAttributeValue<int>(StdMarkdownAttribute.count, @default: 0);

        public int PredicateMatchCount => Model.GetAttributeValue<int>(StdMarkdownAttribute.ismatch, @default: 0);

        protected override async Task OnEpochFinalizingAsync(EpochFinalizingAsyncEventArgs e)
        {
            using (BeginAuthorityClaim())
            {
                await base.OnEpochFinalizingAsync(e);
                if (!e.Cancel)
                {
                    await OnInputTextSettled(new CancelEventArgs());
                }
            }
        }
    }
}
