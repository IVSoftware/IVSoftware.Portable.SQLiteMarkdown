using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Threading;
using Newtonsoft.Json.Serialization;
using SQLite;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
                if (_filterQueryDatabase is null)
                {
                    _filterQueryDatabase = new SQLiteConnection(":memory:");
                    _ = TryCreateTableForContractType();
                }
                return _filterQueryDatabase;
            }
            set
            {
                if (!Equals(_filterQueryDatabase, value))
                {
                    _filterQueryDatabase = value;
                    if(_filterQueryDatabase is not null )
                    {
                        _ = TryCreateTableForContractType();
                    }
                    OnPropertyChanged();
                    this.OnAwaited();
                }
            }
        }

        SQLiteConnection? _filterQueryDatabase = default;

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

        public IEnumerable Recordset
        {
            set
            {
                int success = 0;
                if (value is not null)
                {
                    var recordset = value.Cast<object>().ToArray();
                    var invalidItems = recordset
                        .Where(_ => !ContractType.IsAssignableFrom(_.GetType()))
                        .Select(_ => _.GetType().FullName)
                        .Distinct()
                        .ToArray();

                    if (typeof(IAffinityItem).IsAssignableFrom(ContractType))
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
                            FilterQueryDatabase.DeleteAll(ContractTypeTableMapping);
                            foreach (var item in recordset)
                            {
                                success += FilterQueryDatabase.InsertOrReplace(item);
                            }
                            var nDuplicates = recordset.Length - UnfilteredCount;
                            if(nDuplicates != 0)
                            {
                                Debug.Fail($@"IFD ADVISORY - First Time.");
                                this.Advisory($"{nDuplicates} were identified and removed in recordset.");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        this.RethrowHard(ex, "The SQLite transaction resulted in a rollback.");
                    }
                }
                UnfilteredCount = success;
            }
        }

        /// <summary>
        /// Gets or sets the observable projection representing the effective
        /// (net visible) collection after markdown and predicate filtering.
        /// </summary>
        /// <remarks>
        /// The observable projection is the post-filter view derived from the canonical
        /// recordset and serves as the authoritative source of change notifications.
        /// When assigned, this context subscribes to CollectionChanged to track
        /// structural mutations originating from the projection layer.
        /// Replacing this property detaches the previous projection and attaches the new one.
        /// This property is infrastructure wiring and is not intended for data binding.
        /// </remarks>
        public INotifyCollectionChanged? ObservableProjection
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
            throw new NotImplementedException();
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


        /// <summary>
        /// UI changes for tracking must be wrapped in using block in order to be tracked.
        /// </summary>
        /// <remarks>
        /// Even though we do not act on this collection directly, this circularity guard
        /// prevents this object from reacting to its own pushed filter states.
        /// </remarks>
        public IDisposable BeginUIAction() => _dhostUIAction.GetToken();
        private readonly DisposableHost _dhostUIAction = new();
    }
}
