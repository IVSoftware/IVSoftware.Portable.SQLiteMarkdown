using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

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

    partial class MarkdownContext
    {
        /// <summary>
        /// Gets the contract type associated with this context.
        /// </summary>
        /// <remarks>
        /// Reference type for advisory reporting stream on the IVSoftware.Portable.Common.Exceptions.BeginThrowOrAdvise event.
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
                                // OnContractTypeChanged();
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
                                // OnContractTypeChanged();
                                OnPropertyChanged();
                            }
                            break;
                    }
                }
            }
        }
        Type? _contractType = null;

#if false

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
                _ = TryCreateTableForContractType();
                TableName = ResolveTableNameForPass(ContractType);
            }
        }
#endif
        public Type ProxyType
        {
            get => _proxyType;
            set
            {
                if (value is null)
                {
                    this.ThrowHard<NullReferenceException>(("Proxy type cannot be null."));
                }
                else if (value.IsInterface)
                {
                    this.ThrowHard<NullReferenceException>(("Proxy type must be concrete (abstract is ok)."));
                }
                else
                {
                    if (!Equals(_proxyType, value))
                    {
                        _proxyType = value;
                        if (_proxyType != ContractType)
                        {
                            TableMapping
                                contractMapping = ContractType.GetMapping(),
                                proxyMapping = _proxyType.GetMapping();

                            if (contractMapping.TableName == proxyMapping.TableName)
                            {
                                throw new NotImplementedException("ToDo");
                            }
                            else
                            {
                                this.ThrowPolicyException(SQLiteMarkdownPolicy.ProxyTableMapping);
                            }
                        }
                        OnPropertyChanged();
                    }
                }
            }
        }
        Type _proxyType = null!;
        public TableMapping ProxyTypeTableMapping
        {
            get => _proxyTypeTableMapping;
            set
            {
                if (!Equals(_proxyTypeTableMapping, value))
                {
                    _proxyTypeTableMapping = value;
                    OnPropertyChanged();
                }
            }
        }
        TableMapping _proxyTypeTableMapping = null!;    // Nulls are guarded.

        /// <summary>
        /// The table name for the current parse flow.
        /// </summary>
        public string TableName
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_tableName))
                {
                    this.ThrowFramework<NullReferenceException>(
                        $"{nameof(TableAttribute)} is 'guaranteed by design' to never yield null. " +
                        $"That guarantee has failed.",
                        @throw: true);
                }
                return _tableName;
            }
            set
            {
                if (!Equals(_tableName, value))
                {
                    _tableName = value;
                    OnPropertyChanged();
                }
            }
        }
        string _tableName = string.Empty;

        [Obsolete("Get table name from dedicated SQLite Connection.")]
        protected virtual string ResolveTableNameForPass(Type type)
        {
            if (type.TryGetTableNameFromBaseClass(out var bcTableName, out var bc))
            {
                // The BC is weighing in and we must advise on the possible outcomes:
                if (type.TryGetTableNameFromTableAttribute(out var explicitTableName))
                {
                    if (string.Equals(bcTableName, explicitTableName, StringComparison.Ordinal))
                    {   /* G T K + B C S */
                        // Nothing to see here. The proxy explicitly agrees with the base about the TableName
                    }
                    else
                    {
                        /* W C S */
                        string msg = $@"
{nameof(TableName)} Conflict:
Current Type '{type.Name}' is explicitly mapped to [Table(""{explicitTableName}"")].
Base    Type '{bc.Name}' is explicitly mapped to [Table(""{bcTableName}"")].
The rule is   : ""TO AVOID SPURIOUS TABLE CREATION - BASE CLASS WINS"".
Why it matters: This rule deliberately ignores an explicit attribute.
Rationale     : The contract database must be held stable for this inheritance tree.".TrimStart();
                        switch (ContractErrorLevel)
                        {
                            case ContractErrorLevel.ThrowSoft:
                                localWarnOnceForType(() => this.ThrowSoft<InvalidOperationException>(msg));
                                break;
                            case ContractErrorLevel.ThrowHard:
                                localWarnOnceForType(() => this.ThrowHard<InvalidOperationException>(msg));
                                break;
                            case ContractErrorLevel.Advisory:
                                localWarnOnceForType(() => this.Advisory(msg));
                                break;
                            default:
                                localWarnOnceForType(() => this.ThrowFramework<NotSupportedException>(
                                    $"The {ContractErrorLevel.ToFullKey()} case is not supported.",
                                    @throw: false));
                                break;
                        }
                    }
                }
                else
                {
                    localWarnOnceForType(() =>
                        this.Advisory($@"Type '{type.Name}' is explicitly mapped to [Table(""{bcTableName}"")] in base class."));
                }
            }
            else
            {
                // Accept the uncontroversial table name as seen by SQLite.
                bcTableName = Mapper.GetMapping(type).TableName;
            }
            return bcTableName;

            #region L o c a l F x 
            void localWarnOnceForType(Action warn)
            {
                bool allowWarn;
                lock(_warnLock)
                {
                    allowWarn = _warnedOnType.Add(type);
                }
                if(allowWarn)
                {
                    warn();
                }
            }
            #endregion L o c a l F x
        }
        private readonly HashSet<Type> _warnedOnType = new();
        private readonly object _warnLock = new();

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
