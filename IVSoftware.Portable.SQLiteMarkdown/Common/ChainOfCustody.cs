using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    public class ChainOfCustody : IEnumerable<KeyValuePair<string, ChainOfCustodyEntry>>
    {
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public ChainOfCustodyEntry this[string key]
        {
            set
            {
                if (value is null)
                {
                    _coc.Remove(key);
                }
                else
                {
                    var entry = _coc.TryGetValue(key, out var exists) && exists is not null
                    ? exists
                    : new();

                    JsonConvert.PopulateObject(JsonConvert.SerializeObject(value), entry);
                    _coc[key] = entry;
                }
            }
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
        /// <summary>
        /// Current GUID
        /// </summary>
        public string ModifiedByIn { get; set; } = string.Empty;
        public DateTime ModifiedIn { get; set; }

        /// <summary>
        /// Modified GUID
        /// </summary>
        public string ModifiedByOut { get; set; } = string.Empty;
        public DateTime ModifiedOut { get; set; }
    }
}
