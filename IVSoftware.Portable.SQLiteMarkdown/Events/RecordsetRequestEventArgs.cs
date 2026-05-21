using System.Collections;

namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public class RecordsetRequestEventArgs
    {
        public RecordsetRequestEventArgs(string sql)
        {
            SQL = sql;
        }
        public string SQL { get; }

        public IList? Recordset { get; set; }
    }
}
