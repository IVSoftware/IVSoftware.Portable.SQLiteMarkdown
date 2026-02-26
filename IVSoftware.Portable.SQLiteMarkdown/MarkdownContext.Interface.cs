using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Threading;
using SQLite;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext : IMarkdownContext
    {
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
            protected get => _recordset;
            set
            {
                if (value is null)
                {
                    _recordset.Clear();
                }
                else
                {
                    var recordset = value.Cast<object>().ToArray();
                    var invalidItems = recordset
                        .Where(_ => !ContractType.IsAssignableFrom(_.GetType()))
                        .Select(_ => _.GetType().FullName)
                        .Distinct()
                        .ToArray();

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

#if false
                    int success = 0;
//                    FilterQueryDatabase.RunInTransaction(() =>
//                    {
//                          FilterQueryDatabase.DeleteAll(ContractTypeTableMapping);
////                        if (_recordset.Count != 0)
////                        {
////                            int success = FilterQueryDatabase.InsertAll(_recordset);
////                            if (success != _recordset.Count)
////                            {
////                                this.ThrowHard<SQLiteException>($@"
////{success} of {_recordset.Count} succeeded in {nameof(FilterQueryDatabase.InsertAll)} ");
////                            }
////                        }
//                    });




                    foreach (var item in value)
                    {
                        if (ContractType?.IsAssignableFrom(item.GetType()) == true)
                        {
                            candidate.Add(item);
                        }
                        else
                        {
                            this.ThrowHard<InvalidCastException>($@"
Recordset rejected (rolled back): 
Item of type '{item.GetType().FullName}'
is not assignable to the current ContractType '{ContractType?.FullName ?? "Null Type"}'."
    .TrimStart());
                            return;
                        }
                    }
                    //var removedCount = rawCount - candidate.Count;
                    //if(removedCount > 0)
                    //{
                    //    this.Advisory($@"{removedCount} duplicate items have been removed.");
                    //}
                    _recordset = candidate.ToList();

#endif
                }
                OnRecordsetChanged();
            }
        }
        List<object> _recordset = new();
        protected virtual void OnRecordsetChanged()
        {
            FilterQueryDatabase.RunInTransaction(() =>
            {
                FilterQueryDatabase.DeleteAll(ContractTypeTableMapping);
                if (_recordset.Count != 0)
                {
                    int success = FilterQueryDatabase.InsertAll(_recordset);
                    if (success != _recordset.Count)
                    {
                        this.ThrowHard<SQLiteException>($@"
{success} of {_recordset.Count} succeeded in {nameof(FilterQueryDatabase.InsertAll)} ");
                    }
                }
            });

            switch (_recordset.Count)
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

        protected IList? Projection { get; }

        protected virtual void OnObservableProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public int UnfilteredCount => _recordset.Count;

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
