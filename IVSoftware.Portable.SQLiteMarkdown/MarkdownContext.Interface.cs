using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.Collections;
using SQLite;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext : IMarkdownContext
    {
        protected virtual void OnItemPropertyChanged(object item, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new ItemPropertyChangedEventArgs(e.PropertyName, item));
        }

        /// <summary>
        /// The ephemeral backing store for this collection's contract filtering.
        /// </summary>
        /// <remarks>
        /// - Like any other SQLite database this can be configured with N tables.
        ///   However, the semantic constraints on contract parsing (where ContractType 
        ///   is assumed to be the item type of the collection that subclasses it) will
        ///   provide an advisory stream should this be called upon to service more
        ///   than the implicit single table for the collection.
        /// </remarks>
        protected SQLiteConnection FilterQueryDatabase
        {
            get
            {
                if (!QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    this.ThrowPolicyException(MarkdownContextPolicyViolation.FilterEngineUnavailable);
                    // NOTE:
                    // Handling the Throw creates a benign condition where a DB
                    // that might not really be necessary is instantiated regardless.
                }

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
                if (value is not null && !QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    // The user must be given the benefit of the doubt if they are explicitly
                    // injecting a connection to be used for internal filter queries. This will
                    // silently upgrade the configuration unless escalated in the Throw handler.
                    this.ThrowPolicyException(MarkdownContextPolicyViolation.ConfigurationModifiedByDatabaseAssignment);
                    QueryFilterConfig |= QueryFilterConfig.Filter;
                }

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

        [Obsolete("Version 2.0+ uses clearer semantics: CanonicalCount and PredicateMatchCount.")]
        [PublishedContract("1.0")] // Required for backward compatibility. Do not remove this property.
        public int UnfilteredCount
        {
            get => CanonicalCount;
            protected set => this.ThrowHard<InvalidOperationException>(
                @"[Obsolete(""Version 2.0+ uses clearer semantics: CanonicalCount and PredicateMatchCount."")]");
        }

        #region D U E    T O    L E G A C Y
        /// <summary>
        /// We wish these weren't here in the superclass, but they already are.
        /// That said, it would be cool if they were abstract, but they're not.
        /// </summary>
        /// <remarks>
        /// Bottom line, we'll make consumer aware and let them handle the Throw if they don't care about it.
        /// </remarks>
        public virtual int CanonicalCount 
        {
            get
            {
                this.ThrowHard<ModelException>(
    $"{nameof(CanonicalCount)} requires override in derived type.");
                return 0;
            }
        }

        public virtual int PredicateMatchCount
        {
            get
            {
                this.ThrowHard<ModelException>(
    $"{nameof(PredicateMatchCount)} requires override in derived type.");
                return 0;
            }
        }
        #endregion D U E    T O    L E G A C Y

        /// <summary>
        /// Responsible for raising the InputTextSettled event.
        /// </summary>
        /// <remarks>
        /// The override in ModeledMarkdownContest adds 
        /// authority, but should still make the call to base.
        /// </remarks>
        protected override async Task OnEpochFinalizingAsync(EpochFinalizingAsyncEventArgs e)
        {
            await base.OnEpochFinalizingAsync(e);
            if (!e.Cancel)
            {
                await OnInputTextSettled(new CancelEventArgs());
            }
        }

        public string[] GetTableNames() => FilterQueryDatabase.GetTableNames();

        public IDisposable BeginBusy() => DHostBusy.GetToken();

        /// <summary>
        /// Parses the current <see cref="InputText"/> and raises the <see cref="RecordsetRequest"/> event.
        /// </summary>
        /// <remarks>
        /// Represents the transition point between input parsing and recordset acquisition.
        /// Subscribers may use the current SQL expression to supply a recordset, but are not required to do so.
        ///
        /// This method defines the execution boundary for Query mode. Unlike Filter mode, which
        /// applies changes after a debounced settling interval, Query mode does not impose a
        /// settling timeout on input changes and instead requires an explicit commit.
        /// </remarks>
        public void Commit()
        {
            if (IsFiltering)
            {
                nameof(MarkdownContext).ThrowSoft<InvalidOperationException>(
                    $"{nameof(Commit)} cannot execute while {nameof(IsFiltering)} is true. " +
                    $"Caller must ensure filtering is not active before invoking {nameof(Commit)}."
                );
                return;
            }
            else
            {
                var e = new RecordsetRequestEventArgs(sql: ParseSqlMarkdown());
                using (BeginBusy())
                {
                    OnCommit(e);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="RecordsetRequest"/> event for the current commit.
        /// </summary>
        protected virtual void OnCommit(RecordsetRequestEventArgs e)
        {
            RecordsetRequest?.Invoke(this, e);
        }

        public event EventHandler<RecordsetRequestEventArgs>? RecordsetRequest;
    }
}
