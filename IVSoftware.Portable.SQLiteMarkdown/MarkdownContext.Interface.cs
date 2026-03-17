using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static IVSoftware.Portable.SQLiteMarkdown.Internal.Extensions;
using static SQLite.SQLite3;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext : IMarkdownContext
    {
        protected virtual void OnItemPropertyChanged(object item, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new ItemPropertyChangedEventArgs(e.PropertyName, item));
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
                    this.ThrowPolicyException(SQLiteMarkdownPolicyViolation.FilterEngineUnavailable);
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
                    this.ThrowPolicyException(SQLiteMarkdownPolicyViolation.ConfigurationModifiedByDatabaseAssignment);
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
            protected set => Model.SetAttributeValue(StdMarkdownAttribute.count, value);
        }

        public int CanonicalCount => Model.GetAttributeValue<int>(StdMarkdownAttribute.count);

        public int PredicateMatchCount => Model.GetAttributeValue<int>(StdMarkdownAttribute.matches);

        public CollectionChangeAuthority Authority => DHostAuthorityEpoch.Authority;

        protected override async Task OnEpochFinalizingAsync(EpochFinalizingAsyncEventArgs e)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.Model))
            {
                await base.OnEpochFinalizingAsync(e);
                if (!e.Cancel)
                {
                    await OnInputTextSettled(new CancelEventArgs());
                }
            }
        }

        public string[] GetTableNames() => FilterQueryDatabase.GetTableNames();

        public IDisposable BeginBusy() => DHostBusy.GetToken();

        public void Commit()
        {
            var e = new RecordsetRequestEventArgs(ParseSqlMarkdown());
            using (BeginBusy())
            {
                OnCommit(e);
            }
        }
        protected virtual void OnCommit(RecordsetRequestEventArgs e)
        {
            RecordsetRequest?.Invoke(this, e);
            if (!e.Handled)
            {
                if(RecordsetRequest is null)
                {   /* G T K - N O O P */
                    // This event simply has no subscribers.
                }
                else
                {
                    // This on the other hand signals intention, so
                    // at least one of these things should be available:
                    if (e.CanonicalSuperset is null && MemoryDatabase is null)
                    {
                        // No recordset delivered. No obvious means of getting one.
                        nameof(MarkdownContext).ThrowHard<InvalidOperationException>(
                            $"Expecting {nameof(e.CanonicalSuperset)} or {nameof(e.Handled)}.");
                    }
                }
            }
        }

        public event EventHandler<RecordsetRequestEventArgs>? RecordsetRequest;
    }
}
