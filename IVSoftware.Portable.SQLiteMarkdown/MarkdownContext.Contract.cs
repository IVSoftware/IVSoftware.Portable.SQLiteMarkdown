using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using SQLite;
using System;
using System.Diagnostics;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public enum ContractErrorLevel
    {
        /// <summary>
        /// Default reporting level on IVSoftware.Portable.Common.Exceptions.BeginThrowOrAdvise event.
        /// </summary>
        /// <remarks>
        /// This *will not throw* an exception *unless* escalated by external 
        /// subscriber, ensuring that the stricter 2.0+ semantics are non-breaking.
        /// </remarks>
        ThrowSoft,

        /// <summary>
        /// Strict reporting level on IVSoftware.Portable.Common.Exceptions.BeginThrowOrAdvise event.
        /// </summary>
        /// <remarks>
        /// This *will throw* an exception *unless* handled by external subscriber.
        /// </remarks>
        ThrowHard,

        /// <summary>
        /// Produce a message on the connection point without no possibility of throwing an exception.
        /// </summary>
        Advisory,
    }
    public interface IMarkdownContextContract
    {
        Type ContractType { get; set; }

        string TableName { get; }
        string PrimaryKeyName { get; }

        ContractErrorLevel ContractErrorLevel{ get; set; }
    }
    partial class MarkdownContext : IMarkdownContextContract
    {

        /// <summary>
        /// Gets the contract type associated with this context.
        /// </summary>
        /// <remarks>
        /// - In v2.0.0+ the distinction between anonymous (parser-only) and
        ///   contract-bound contexts has been formalized.
        /// - When constructed using the parameterless constructor, the context
        ///   operates in anonymous mode and no contract type is assigned. In this 
        ///   state, contract-dependent features (such as table identity and
        ///   SQLite-backed filtering) are not initialized and will throw if accessed.    
        /// - When a contract type is provided, the context becomes table-aware
        ///   and enforces contract semantics. Accessing this property in
        ///   anonymous mode triggers a guarded failure, preserving the boundary
        ///   between parser-only and contract-bound behavior.
        /// </remarks>
        public Type ContractType
        {
            get
            {
                if (_contractType is null)
                {
                    this.ThrowHard<NullReferenceException>($"{nameof(ContractType)} cannot be null.");
                }
                return _contractType!;
            }
            set
            {
                if (value is null)
                {
                    this.ThrowHard<NullReferenceException>($"{nameof(ContractType)} cannot be null.");
                }
                else
                {
                    switch (_activeQFMode)
                    {
                        case QueryFilterMode.Query:
                            // Allow unconditional
                            if (!Equals(_contractType, value))
                            {
                                _contractType = value;
                                OnContractTypeChanged();
                                OnPropertyChanged();
                            }
                            break;
                        case QueryFilterMode.Filter:
                            // Allow only if Type not set by previous query.
                            Debug.Assert(
                                QueryFilterConfig == QueryFilterConfig.Filter,
                                "Expecting Query before Filter in any other mode!"
                            );

                            if (_contractType is null)
                            {
                                _contractType = value;
                                OnContractTypeChanged();
                                OnPropertyChanged();
                            }
                            break;
                    }
                }
            }
        }
        Type? _contractType = default;

        /// <summary>
        /// Creates or recreates a memory database containing a table for the ContractType
        /// </summary>
        protected virtual void OnContractTypeChanged()
        {
            if (ContractType is null)
            {
                this.ThrowHard<NullReferenceException>($"{nameof(ContractType)} cannot be null.");
            }
            else
            {
                if (IsFilterExecutionEnabled)
                {
                    if (FilterQueryDatabase != null)
                    {
                        FilterQueryDatabase.Dispose();
                    }
                    FilterQueryDatabase = new SQLiteConnection(":memory:");
                    FilterQueryDatabase.CreateTable(ContractType);
                    // Loopback 'as seen by' SQLite
                    var mapping = FilterQueryDatabase.GetMapping(ContractType);
                }
            }
        }

        /// <summary>
        /// Heuristic - Follows internal rules that dictate contract intent.
        /// </summary>
        public string TableName
        {
            get
            {
                if (_tableName is null)
                {
                    if (_contractType is null) // Perform null check on backing store, not the property.
                    {
                        this.ThrowHard<NullReferenceException>($"The {nameof(ContractType)} must be assigned first.");
                        _tableName = null!; // We warned you.
                    }
                    else
                    {
                        if(ContractType.TryGetTableNameFromBaseClass(out _tableName, out var bc))
                        {
                            // The BC is weighing in and we must advise on the possible outcomes:
                            if(ContractType.TryGetTableNameFromTableAttribute(out var explicitTableName))
                            {
                                if (string.Equals(_tableName, explicitTableName, StringComparison.Ordinal))
                                {   /* G T K + B C S */
                                    // Nothing to see here. The proxy explicitly agrees with the base about the TableName
                                }
                                else
                                {   /* W C S */
                                    string msg = $@"
{nameof(TableName)} Conflict:

{ContractType.Name} is explicitly mapped to [Table(""{_tableName}"") in base class.
";
                                    switch (ContractErrorLevel)
                                    {
                                        case ContractErrorLevel.ThrowSoft:
                                            break;
                                        case ContractErrorLevel.ThrowHard:
                                            break;
                                        case ContractErrorLevel.Advisory:
                                            this.Advisory($@"{ContractType.Name} is explicitly mapped to [Table(""{_tableName}"") in base class.");
                                            break;
                                        default:
                                            this.ThrowFramework<NotSupportedException>(
                                                $"The {ContractErrorLevel.ToFullKey()} case is not supported.",
                                                @throw: false);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                this.Advisory($@"{ContractType.Name} is explicitly mapped to [Table(""{_tableName}"") in base class.");
                            }
                        }
                        else
                        {
                            // Accept the uncontroversial table name as seen by SQLite.
                            _tableName = Mapper.GetMapping(ContractType).TableName;
                        }
                    }
                }
                return _tableName;
            }
        }
        string? _tableName = null;

        public string PrimaryKeyName
        {
            get
            {
                if (_primaryKeyName is null) // Perform null check on backing store, not the property.
                {
                    if (_contractType is null)
                    {
                        this.ThrowHard<NullReferenceException>($"The {nameof(ContractType)} must be assigned first.");
                        _primaryKeyName = null!; // We warned you.
                    }
                    else
                    {
                        _primaryKeyName = Mapper.GetMapping(ContractType).PK?.Name ?? "rowid";
                    }
                }
                return _primaryKeyName;
            }
        }
        string? _primaryKeyName = null;

        protected static SQLiteConnectionMapper Mapper
        {
            get
            {
                if (_mapper is null)
                {
                    _mapper = new SQLiteConnectionMapper();
                }
                return _mapper;
            }
        }
        static SQLiteConnectionMapper? _mapper = null;

        string GetTableNameHeuristic(Type type)
        {
            if (type is null)
            {
                return string.Empty;
            }
            else if (FilterQueryDatabase is null)
            {
                return type.GetCustomAttribute<TableAttribute>()?.Name ?? type.Name;
            }
            else
            {
                return FilterQueryDatabase.GetMapping(type).TableName;
            }
        }
        public ContractErrorLevel ContractErrorLevel { get; set; } = ContractErrorLevel.ThrowSoft;

        protected sealed class SQLiteConnectionMapper
        {
            public TableMapping GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
                => Mapper.GetMapping(type, createFlags);
            SQLiteConnection Mapper
            {
                get
                {
                    if (_mapper is null)
                    {
                        _mapper = new SQLiteConnection(":memory:");
                    }
                    return _mapper;
                }
            }
            SQLiteConnection? _mapper = null;
        }
    }
}
