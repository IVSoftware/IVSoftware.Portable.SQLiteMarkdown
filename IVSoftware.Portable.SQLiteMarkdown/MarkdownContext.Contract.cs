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

        string ContractTableName { get; }
        string ContractPrimaryKeyName { get; }

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
                _contractTableName = null;
                _contractPrimaryKeyName = null;
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

        protected string TableName { get; set; }

        /// <summary>
        /// Heuristic - Follows internal rules that dictate contract intent.
        /// </summary>
        public string ContractTableName
        {
            get
            {
                if (_contractTableName is null)
                {
                    if (_contractType is null) // Perform null check on backing store, not the property.
                    {
                        this.ThrowHard<NullReferenceException>($"The {nameof(ContractType)} must be assigned first.");
                        _contractTableName = null!; // We warned you.
                    }
                    else
                    {
                        _contractTableName = ResolveTableNameForPass(ContractType);
#if false && MOVED
                        if(ContractType.TryGetTableNameFromBaseClass(out _contractTableName, out var bc))
                        {
                            // The BC is weighing in and we must advise on the possible outcomes:
                            if(ContractType.TryGetTableNameFromTableAttribute(out var explicitTableName))
                            {
                                if (string.Equals(_contractTableName, explicitTableName, StringComparison.Ordinal))
                                {   /* G T K + B C S */
                                    // Nothing to see here. The proxy explicitly agrees with the base about the TableName
                                }
                                else
                                {   /* W C S */
                                    string msg = $@"
{nameof(ContractTableName)} Conflict:
Current Type '{ContractType.Name}' is explicitly mapped to [Table(""{explicitTableName}"")].
Base    Type '{bc.Name}' is explicitly mapped to [Table(""{_contractTableName}"")].
The rule is   : ""TO AVOID SPURIOUS TABLE CREATION - BASE CLASS WINS"".
Why it matters: This rule deliberately ignores an explicit attribute.
Rationale     : The contract database must be held stable for this inheritance tree.".TrimStart();
                                    switch (ContractErrorLevel)
                                    {
                                        case ContractErrorLevel.ThrowSoft:
                                            this.ThrowSoft<InvalidOperationException>(msg);
                                            break;
                                        case ContractErrorLevel.ThrowHard:
                                            this.ThrowHard<InvalidOperationException>(msg);
                                            break;
                                        case ContractErrorLevel.Advisory:
                                            this.Advisory(msg);
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
                                this.Advisory($@"Type '{ContractType.Name}' is explicitly mapped to [Table(""{_contractTableName}"")] in base class.");
                            }
                        }
                        else
                        {
                            // Accept the uncontroversial table name as seen by SQLite.
                            _contractTableName = Mapper.GetMapping(ContractType).TableName;
                        }
#endif
                    }
                }
                return _contractTableName;
            }
        }
        string? _contractTableName = null;

        protected virtual string ResolveTableNameForPass(Type type)
        {
            if (type.TryGetTableNameFromBaseClass(out var contractTableName, out var bc))
            {
                // The BC is weighing in and we must advise on the possible outcomes:
                if (type.TryGetTableNameFromTableAttribute(out var explicitTableName))
                {
                    if (string.Equals(contractTableName, explicitTableName, StringComparison.Ordinal))
                    {   /* G T K + B C S */
                        // Nothing to see here. The proxy explicitly agrees with the base about the TableName
                    }
                    else
                    {   /* W C S */
                        string msg = $@"
{nameof(ContractTableName)} Conflict:
Current Type '{type.Name}' is explicitly mapped to [Table(""{explicitTableName}"")].
Base    Type '{bc.Name}' is explicitly mapped to [Table(""{contractTableName}"")].
The rule is   : ""TO AVOID SPURIOUS TABLE CREATION - BASE CLASS WINS"".
Why it matters: This rule deliberately ignores an explicit attribute.
Rationale     : The contract database must be held stable for this inheritance tree.".TrimStart();
                        switch (ContractErrorLevel)
                        {
                            case ContractErrorLevel.ThrowSoft:
                                this.ThrowSoft<InvalidOperationException>(msg);
                                break;
                            case ContractErrorLevel.ThrowHard:
                                this.ThrowHard<InvalidOperationException>(msg);
                                break;
                            case ContractErrorLevel.Advisory:
                                this.Advisory(msg);
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
                    this.Advisory($@"Type '{type.Name}' is explicitly mapped to [Table(""{contractTableName}"")] in base class.");
                }
            }
            else
            {
                // Accept the uncontroversial table name as seen by SQLite.
                contractTableName = Mapper.GetMapping(type).TableName;
            }
            return contractTableName;
        }

        public string ContractPrimaryKeyName
        {
            get
            {
                if (_contractPrimaryKeyName is null) // Perform null check on backing store, not the property.
                {
                    if (_contractType is null)
                    {
                        this.ThrowHard<NullReferenceException>($"The {nameof(ContractType)} must be assigned first.");
                        _contractPrimaryKeyName = null!; // We warned you.
                    }
                    else
                    {
                        _contractPrimaryKeyName = Mapper.GetMapping(ContractType).PK?.Name ?? "rowid";
                    }
                }
                return _contractPrimaryKeyName;
            }
        }
        string? _contractPrimaryKeyName = null;

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
        public ContractErrorLevel ContractErrorLevel { get; set; } = ContractErrorLevel.ThrowSoft;

        protected sealed class SQLiteConnectionMapper
        {
            public TableMapping GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
                => Mapper.GetMapping(type, createFlags);
            public TableMapping GetMapping(
                Type type,
                out string? pkName,
                out string? pkPropertyName,
                CreateFlags createFlags = CreateFlags.None)
            {
                var mapper = Mapper.GetMapping(type, createFlags);
                pkName = mapper.PK?.Name;
                pkPropertyName = mapper.PK?.PropertyName;
                return mapper;
            }
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
