using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using Newtonsoft.Json.Serialization;
using SQLite;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    [Flags]
    internal enum NetProjectionOption
    {
        ObservableOnly,
        AllowDirectChanges = 0x1,
    }

    partial class MarkdownContext : IMarkdownContext
    {
        /// <summary>
        /// Immutable once set.
        /// </summary>
        public XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new XElement(nameof(StdMarkdownElement.model));
                    _model.Changed += (sender, e) =>
                    {
                        switch (sender)
                        {
                            case XElement:
                                switch (e.ObjectChange)
                                {
                                    case XObjectChange.Add:
                                        break;
                                    case XObjectChange.Remove:
                                        break;
                                }
                                break;
                        }
                    };
                }
                return _model;
            }
        }
        XElement? _model = null;

        protected async Task RunFSM(Type fsm)
        {
            foreach (Enum state in fsm.GetEnumValues())
            {
                await ExecStateAsync(state);
            }
        }
        protected async Task<Enum> ExecStateAsync(Enum state) 
        {
            return ReservedAffinityState.None;
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
        /// Model the canonical recordset as hierarchal xml.
        /// </summary>
        public IEnumerable Recordset
        {
            set
            {
                Type listType = value?.GetType() ?? typeof(object);
                if (listType.IsGenericType && listType.GetGenericArguments().Single() is { } itemType)
                {
                    FilterQueryDatabase.CreateTable(itemType);

                    Model.RemoveNodes();
                    object[] recordset = value?.Cast<object>().ToArray() ?? [];

                    int
                        countDistinct = 0,
                        countDuplicate = 0;
                    if (itemType.GetMapping() is { } mapping)
                    {
                        PropertyInfo? pk = mapping.PK?.PropertyInfo;
                        if (pk is null)
                        {
                            throw new NotSupportedException($"Type '{itemType.Name}' has no PK and such types are not (yet) supported.");
                        }
                        foreach (EnterFilterFSM state in Enum.GetValues(typeof(EnterFilterFSM)))
                        {
                            switch (state)
                            {
                                case EnterFilterFSM.CaptureUnfilteredItemsArray:
                                    break;
                                case EnterFilterFSM.InitializeUnfilteredItemsCollection:
                                    break;
                                case EnterFilterFSM.InitializeModel:
                                    foreach (var item in recordset)
                                    {
                                        var placerResult = Model.Place(path: localGetFullPath(pk, item), out var xel);
                                        switch (placerResult)
                                        {
                                            case PlacerResult.Exists:
                                                countDuplicate++;
                                                break;
                                            case PlacerResult.Created:
                                                xel.SetBoundAttributeValue(
                                                    tag: item,
                                                    name: nameof(StdMarkdownElement.xitem));
                                                countDistinct++;
                                                break;
                                            default:
                                                this.ThrowFramework<NotSupportedException>(
                                                    $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                                break;
                                        }
                                    }
                                    break;
                                case EnterFilterFSM.InitializeFilterQueryDatabase:
                                    break;
                                case EnterFilterFSM.SuppressedReplace:
                                    break;
                                case EnterFilterFSM.RaiseResetEvent:
                                    break;
                                default:
                                    this.ThrowHard<NotSupportedException>($"The {state.ToFullKey()} case is not supported.");
                                    break;
                            }
                        }
                        if (countDuplicate != 0)
                        {
                            Debug.Fail($@"IFD ADVISORY - First Time.");
                            this.Advisory($"{countDuplicate} were identified and removed in recordset.");
                        }
                        UnfilteredCount = countDistinct;
                        
                        RouteToFullRecordset = true;
#if false
                        if (unfilteredItems is ITemporalAffinity temporal)
                        {
                            foreach (ExitFilterFSM state in Enum.GetValues(typeof(ExitFilterFSM)))
                            {

                            }
                            throw new NotImplementedException("ToDo");
                        }
                        else if (unfilteredItems is IPrioritizedAffinity prioritized)
                        {
                            foreach (ExitFilterFSM item in Enum.GetValues(typeof(ExitFilterFSM)))
                            {

                            }
                            throw new NotImplementedException("ToDo");
                        }
                        else if (unfilteredItems is IList collection)
                        {
                            PropertyInfo? pk = mapping.PK?.PropertyInfo;
                            if (pk is null)
                            {
                                throw new NotSupportedException($"Type '{itemType.Name}' has no PK and such types are not (yet) supported.");
                            }
                            foreach (ExitFilterFSM state in Enum.GetValues(typeof(ExitFilterFSM)))
                            {
                                switch (state)
                                {
                                    case ExitFilterFSM.CaptureUnfilteredItemsArray:
                                        break;
                                    case ExitFilterFSM.InitializeUnfilteredItemsCollection:
                                        break;
                                    case ExitFilterFSM.InitializeModel:
                                        foreach (var item in collection)
                                        {
                                            Model.Add(localMakeXel(pk, item));
                                        }
                                        break;
                                    case ExitFilterFSM.InitializeFilterQueryDatabase:
                                        break;
                                    case ExitFilterFSM.SuppressedReplace:
                                        break;
                                    case ExitFilterFSM.RaiseResetEvent:
                                        break;
                                    default:
                                        this.ThrowHard<NotSupportedException>($"The {state.ToFullKey()} case is not supported.");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
#endif
                    }
                    #region L o c a l F x
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


#if false
                    XElement localMakeXel(PropertyInfo pk, object item)
                    {
                        if (pk.GetValue(item)?.ToString() is { } id && !string.IsNullOrWhiteSpace(id))
                        {
                            var xel = new XElement(
                                nameof(StdMarkdownElement.xitem),
                                new XAttribute(nameof(StdMarkdownAttribute.text), id));

                            xel.SetBoundAttributeValue(
                                tag: item,
                                name: nameof(StdMarkdownElement.xitem));

                            return xel;
                        }
                        this.ThrowHard<NullReferenceException>();
                        return null!;
                    }
#endif
                }
                #endregion L o c a l F x
                return;

                int success = 0;
                if (value is not null)
                {
                    object[] recordset = value?.Cast<object>().ToArray() ?? [];
                    var invalidItems = recordset
                        .Where(_ => !ContractType.IsAssignableFrom(_.GetType()))
                        .Select(_ => _.GetType().FullName)
                        .Distinct()
                        .ToArray();

                    if (typeof(ITemporalAffinity).IsAssignableFrom(ContractType))
                    {   /* G T K */
                    }

                    if (invalidItems.Length != 0)
                    {
                        this.ThrowHard<InvalidCastException>($@"
Recordset rejected (rolled back):
{invalidItems.Length} item(s) are not assignable to the current ContractType '{ContractType.FullName}'.

Invalid type(s):
- {string.Join("\n- ", invalidItems)}

Recordset assignment is atomic; no changes were applied."
                        .TrimStart());
                        return;
                    }
                    try
                    {
                        FilterQueryDatabase.RunInTransaction(() =>
                        {
                            FilterQueryDatabase.DeleteAll(ContractType.GetMapping());
                            foreach (var item in recordset)
                            {
                                success += FilterQueryDatabase.InsertOrReplace(item);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        this.RethrowHard(ex, "The SQLite transaction resulted in a rollback.");
                    }
                    foreach (var record in recordset)
                    {
                        // Model.Place()
                    }
                    var nDuplicates = recordset.Length - success;
                    if (nDuplicates != 0)
                    {
                        Debug.Fail($@"IFD ADVISORY - First Time.");
                        this.Advisory($"{nDuplicates} were identified and removed in recordset.");
                    }
                }
                UnfilteredCount = success;
            }
        }

        /// <summary>
        /// Determines whether MDC is allowed to pupetteer the projection directly.
        /// </summary>
        internal NetProjectionOption ProjectionOptions { get; set; }

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
        /// 
        /// MentalMode (Query          config): "Do not track changes on this INCC."
        /// MentalMode (QueryAndFilter config): "The system must be reset to root cause in order to be stable."
        /// MentalMode (Filter         config): "The contents of the new projection must be regarded as a new canon."
        protected virtual void OnObservableProjectionChanged() 
        {
            switch (QueryFilterConfig)
            {
                case QueryFilterConfig.Filter:
                    if (ObservableNetProjection is IEnumerable recordset)
                    {
                        Recordset = recordset;
                    }
                    else
                    {
                        Recordset = recordset = Array.Empty<object>();
                    }
                    break;
                case QueryFilterConfig.Query:
                case QueryFilterConfig.QueryAndFilter:
                    Clear(all: true);
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {QueryFilterConfig.ToFullKey()} case is not supported.");
                    break;
            }
        }

        /// <summary>
        /// Raised when the collection - that is the ObservableNetProjection - is modified in some way.
        /// </summary>
        protected virtual void OnObservableProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { } 


        public int UnfilteredCount
        {
            get => _unfilteredCount;
            protected set
            {
                if (!Equals(_unfilteredCount, value))
                {
                    _unfilteredCount = value; 
                    switch (_unfilteredCount)
                    {
                        case 0:
                            SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                            FilteringState = FilteringState.Ineligible;
                            if (QueryFilterConfig == QueryFilterConfig.QueryAndFilter)
                            {
                                IsFiltering = false;
                            }
                            break;
                        case 1:
                            SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                            FilteringState = FilteringState.Ineligible;
                            if (QueryFilterConfig == QueryFilterConfig.QueryAndFilter)
                            {
                                IsFiltering = false;
                            }
                            break;
                        default:
                            SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                            FilteringState = FilteringState.Armed;
                            if (QueryFilterConfig == QueryFilterConfig.QueryAndFilter)
                            {
                                IsFiltering = true;
                            }
                            break;
                    }
                    OnPropertyChanged();
                }
            }
        }
        int _unfilteredCount = default;

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

        public IDisposable BeginAuthorityClaim() => DHostClaimAuthority.GetToken();

        DisposableHost DHostClaimAuthority
        {
            get
            {
                if (_dhostClaimAuthority is null)
                {
                    _dhostClaimAuthority = new DisposableHost();
                    _dhostClaimAuthority.BeginUsing += (sender, e)
                        => CollectionChangeAuthority = NotifyCollectionChangedEventAuthority.MarkdownContext;
                    _dhostClaimAuthority.FinalDispose += (sender, e)
                        => CollectionChangeAuthority = NotifyCollectionChangedEventAuthority.NetProjection;
                }
                return _dhostClaimAuthority;
            }
        }
        DisposableHost? _dhostClaimAuthority = null;

        public NotifyCollectionChangedEventAuthority CollectionChangeAuthority { get; private set; }
    }
}
