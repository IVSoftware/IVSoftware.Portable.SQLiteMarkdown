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

        /// <summary>
        /// Optional recordset supplied by the handler for this request.
        /// </summary>
        /// <remarks>
        /// Null indicates no recordset was provided. 
        /// An empty collection indicates a successful query with no results.
        /// </remarks>
        [Careful("Null is not the same as Empty.")]
        public IList? CanonicalSuperset { get; internal set; }
    }
}
