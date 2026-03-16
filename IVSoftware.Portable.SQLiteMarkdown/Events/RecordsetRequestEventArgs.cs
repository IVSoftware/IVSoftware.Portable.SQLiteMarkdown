using IVSoftware.Portable.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
        public object[]? CanonicalSuperset { get; internal set; }
    }
}
