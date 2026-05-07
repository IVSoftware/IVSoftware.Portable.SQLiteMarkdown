using IVSoftware.Portable.Common.Attributes;
using System;
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
        public int? RecordsetCount { get; set; }
    }
}
