using IVSoftware.Portable.Common.Attributes;
using System.Collections;
using System.ComponentModel;

namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public class RecordsetRequestEventArgs : HandledEventArgs
    {
        public RecordsetRequestEventArgs(string sql)
        {
            SQL = sql;
        }
        public string SQL { get;  }

        [Careful("Null is not the same as Empty.")]
        public IList? CanonicalSuperset { get; internal set; }
    }
}
