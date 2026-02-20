using IVSoftware.Portable.Common.Exceptions;
using SQLite;
using System;
using System.Diagnostics;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public interface IMarkdownContextContract
    {
        Type ContractType { get; set; }
    }
    partial class MarkdownContext : IMarkdownContextContract
    {
        /// <summary>
        /// The attributed CLR type used to resolve which properties participate in term matching.
        /// </summary>
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
                    TableName = mapping.TableName;
                    if (mapping.PK is null)
                    {
                        // Fallback to SQLite implicit rowid if no explicit PK exists.
                        // Consumer may override by handling the Throw.

                        PrimaryKeyName = "rowid";

                        string msg = $@"
Context has been created with filter execution enabled
'{ContractType.Name}' is a model that provides no [PrimaryKey].
If this is deliberate, mark this Throw as Handled.
Overriding the OnContractTypeChanged method in a subclass offers full control.
".TrimStart();
                        this.ThrowHard<SQLiteException>(msg);
                    }
                    else
                    {
                        PrimaryKeyName = mapping.PK.Name;
                    }
                }
                else
                {
                    TableName = GetTableNameHeuristic(ContractType);
                }
            }
        }
        protected string TableName { get; set; }
        protected string PrimaryKeyName { get; set; }

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
    }
}
