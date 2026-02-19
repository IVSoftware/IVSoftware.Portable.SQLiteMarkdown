using SQLite;
using System;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public class TableNameResolvingEventArgs : EventArgs
    {
        public TableNameResolvingEventArgs(Type type, SQLiteConnection? cnx)
        {
            Type = type;

            // I M P O R T A N T
            // When cnx is not null:
            // - This *does* create a TableMapping in FilterQueryDatabase.
            // - But it *does not* create an actual Table.
            TableNameFromSQLiteMapping = cnx?.GetMapping(type)?.TableName;

            TableNameFromAttribute = type.GetCustomAttribute<TableAttribute>(inherit: false)?.Name;

            foreach (var @base in type.BaseTypes())
            {
                TableNameFromBaseClassAttribute = @base.GetCustomAttribute<TableAttribute>(inherit: false)?.Name;
                if(!string.IsNullOrWhiteSpace(TableNameFromBaseClassAttribute))
                {
                    break;
                }
            }
        }

        public string TableNameFromMetadata =>
            TableNameFromSQLiteMapping
            ?? TableNameFromAttribute
            ?? Type.Name;

        /// <summary>
        /// Authoritative table name with modification by handler.
        /// </summary>
        public string TableName
        {
            get => 
                string.IsNullOrWhiteSpace(_tableName)
                ? TableNameFromMetadata
                : _tableName!;
            set
            {
                if (!Equals(_tableName, value))
                {
                    _tableName = value;
                }
            }
        }
        string? _tableName = null;

        /// <summary>
        /// The calling type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The explicit [Table] name or null.
        /// </summary>
        public string? TableNameFromAttribute { get; }

        /// <summary>
        /// The explicit [Table] name from SQLiteMapping is available.
        /// </summary>
        public string? TableNameFromSQLiteMapping { get; }

        /// <summary>
        /// The explicit [Table] name from the first base class that has one.
        /// </summary>
        public string? TableNameFromBaseClassAttribute { get; }

        bool _isUserSetName => !string.IsNullOrWhiteSpace(_tableName);

        public bool IsConflict =>
            !( _isUserSetName
               ||string.IsNullOrWhiteSpace(TableNameFromBaseClassAttribute)
               || Equals(TableName, TableNameFromBaseClassAttribute));
    }
}