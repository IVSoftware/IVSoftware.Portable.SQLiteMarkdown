using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class AffinityQFModel : ICustomProperties
    {

#if ABSTRACT
recordset = cnx.Query<AffinityQFModel>($@"
    Select *
    From items 
    Where {"CustomProperties".JsonExtract("UserValue")} LIKE '%hello world%'");
#endif
        /// <summary>
        /// Domain-level JSON property bag.
        /// </summary>
        [Careful(@"
These are DIFFERENT:
CustomProperties (this property) is user-defined metadata.
SelfIndexed.Properties (base class property) is single self-indexed term for query.")]
        [SQLite.Column("CustomProperties"), JsonProperty("CustomProperties")]
        public string CustomProperties_JSON
        {
            get => JsonConvert.SerializeObject(CustomProperties, Formatting.Indented);
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
        /// <summary>
        /// In-memory representation of domain custom properties.
        /// </summary>
        /// <remarks>
        /// Not automatically indexed. Not part of SelfIndexed persistence.
        /// </remarks>
        [SQLite.Ignore, JsonIgnore]
        public IDictionary<string, string?> CustomProperties { get; protected set; } = new Dictionary<string, string?>();
    }
}
