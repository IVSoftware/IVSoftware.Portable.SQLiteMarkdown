using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using Newtonsoft.Json.Serialization;
using SQLite;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext : IMarkdownContext
    {
        /// <summary>
        /// If set, the MDC puppeteers the visible projection directly.
        /// </summary>
        /// <remarks>
        /// WHAT IT IS: The handle to the ItemsSource that is bound to (what is presumed to be) the UI.
        /// WHAT IT IS NOT: A readable list to sync to.
        /// </remarks>
        protected IList? Projection { get; }

        /// <summary>
        /// Immutable once set.
        /// </summary>
        public XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new XElement(nameof(StdElement.model));
                }
                return _model;
            }
        }
        XElement? _model = null;


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

        public IEnumerable Recordset
        {
            set
            {
                int success = 0;
                var recordset = value?.Cast<object>().ToArray() ?? [];
                if (value is not null)
                {
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
                }
                var nDuplicates = recordset.Length - success;
                if (nDuplicates != 0)
                {
                    Debug.Fail($@"IFD ADVISORY - First Time.");
                    this.Advisory($"{nDuplicates} were identified and removed in recordset.");
                }
                UnfilteredCount = success;
            }
        }

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
                    if (_observableProjection is not null)
                    {
                        _observableProjection.CollectionChanged -= OnObservableProjectionCollectionChanged;
                    }

                    _observableProjection = value;

                    if (_observableProjection is not null)
                    {
                        _observableProjection.CollectionChanged += OnObservableProjectionCollectionChanged;
                    }
                }
            }
        }

        INotifyCollectionChanged? _observableProjection = null;

        protected virtual void OnObservableProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
        { 
        }


        public int UnfilteredCount
        {
            get => _unfilteredCount;
            set
            {
                if (!Equals(_unfilteredCount, value))
                {
                    _unfilteredCount = value; 
                    switch (_unfilteredCount)
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
                            FilteringState = FilteringState.Armed;
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
