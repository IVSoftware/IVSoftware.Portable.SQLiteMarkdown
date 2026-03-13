using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    public class RecordsetRequestEventArgs : EventArgs
    {
        public object CanonicalSuperset { get; internal set; }
    }
}
