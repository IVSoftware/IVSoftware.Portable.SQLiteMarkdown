using System;
using System.Collections;
using System.Collections.Generic;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    public class ChainOfCustody : IEnumerable<KeyValuePair<string, ChainOfCustodyEntry>>
    {
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime BeginModify(string modifiedBy)
        {
            var now = DateTime.UtcNow;

            var entry = _coc.TryGetValue(modifiedBy, out var exists) && exists is not null
            ? exists
            : new();

            entry.ModifiedIn = now;

            // This is set after a round trip to the 'cloud' where
            // the transaction yields an authoritative time stamp.
            entry.ModifiedOut = null;

            _coc[modifiedBy] = entry;

            return now;
        }

        private readonly Dictionary<string, ChainOfCustodyEntry> _coc = new();

        public IEnumerator<KeyValuePair<string, ChainOfCustodyEntry>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, ChainOfCustodyEntry>>)_coc).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_coc).GetEnumerator();
        }
    }
    public class ChainOfCustodyEntry
    {
        public DateTime ModifiedIn { get; set; }

        public DateTime? ModifiedOut { get; set; }
    }
}
