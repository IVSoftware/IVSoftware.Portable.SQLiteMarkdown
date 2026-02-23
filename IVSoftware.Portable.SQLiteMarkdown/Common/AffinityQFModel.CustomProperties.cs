using IVSoftware.Portable.Common.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class AffinityQFModel : ICustomProperties
    {

        [SQLite.Column("CustomProperties"), JsonProperty]
        public string CustomProperties_JSON
        {
            get => JsonConvert.SerializeObject(ChainOfCustody, Formatting.Indented);
            set
            {
                try
                {
                    CustomProperties =
                        string.IsNullOrWhiteSpace(value)
                        ? new()
                        : JsonConvert.DeserializeObject<Dictionary<string, string?>>(value)!;
                }
                catch (Exception ex)
                {
                    this.RethrowSoft(ex);
                }
            }
        }

        [SQLite.Ignore, JsonIgnore]
        public IDictionary<string, string?> CustomProperties { get; protected set; } = new Dictionary<string, string?>();
    }
}
