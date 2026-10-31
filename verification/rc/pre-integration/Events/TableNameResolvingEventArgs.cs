
namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public enum TableNameResolution
    {
        /// <summary>
        /// [Table] attribute on base class conflicts with proxy type used for SQL generation.
        /// </summary>
        Conflicted,

        /// <summary>
        /// Authority: SELECT * FROM {current TableName}
        /// </summary>
        UseCurrent,

        /// <summary>
        /// Authority: SELECT * FROM {baseClassTableName}
        /// </summary>
        UseInherited,
    }
    public class ResolveTableNameConflictEventArgs
    {
        public ResolveTableNameConflictEventArgs(string currentTableName, string baseClassTableName)
        {
            CurrentTableName = currentTableName;
            BaseClassTableName = baseClassTableName;
        }
        public string CurrentTableName { get; }
        public string BaseClassTableName { get; }

        public TableNameResolution TableNameResolution { get; set; } = TableNameResolution.Conflicted;
    }
}