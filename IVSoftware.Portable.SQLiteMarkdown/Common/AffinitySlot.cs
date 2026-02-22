using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    /// <summary>
    /// Partial epoch that is constrained by fixed time epochs in the collection.
    /// </summary>
    public class AffinitySlot
    {
        /// <summary>
        /// Earliest available start time in context,
        /// </summary>
        public DateTimeOffset UtcStart { get; set; }

        /// <summary>
        /// Latest available start time in context,
        /// </summary>
        public DateTimeOffset UtcEnd { get; set; }

        /// <summary>
        /// UtcEnd - UtcStart
        /// </summary>
        public TimeSpan AvailableTime => UtcEnd - UtcStart;
    }
}
