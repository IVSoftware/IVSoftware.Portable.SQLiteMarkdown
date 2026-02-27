using IVSoftware.Portable.Common.Exceptions;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Ephemeral = SQLite.IgnoreAttribute;
using Column = SQLite.ColumnAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class TemporalAffinityQFModel 
        : IChainOfCustody
    {
        public DateTimeOffset Created => ChainOfCustody.Created;

        /// <summary>
        /// SQLite backing store where column name is ChainOfCustody.
        /// </summary>
        [Column("ChainOfCustody"), JsonProperty("ChainOfCustody")]
        public string ChainOfCustody_JSON 
        {
            get => JsonConvert.SerializeObject(ChainOfCustody, Formatting.Indented);
            set
            {
                try
                {
                    ChainOfCustody =
                        string.IsNullOrWhiteSpace(value)
                        ? new()
                        : JsonConvert.DeserializeObject<ChainOfCustody>(value)!;
                }
                catch (Exception ex)
                {
                    this.RethrowSoft(ex);
                }
            }
        }

        [Ephemeral, JsonIgnore]
        public ChainOfCustody ChainOfCustody { get; protected set; } = new();

        public Task<DateTimeOffset> CommitLocalEdit(string identity) =>
            ((IChainOfCustody)ChainOfCustody).CommitLocalEdit(identity);

        public Task<ChainOfCustodyToken> CommitRemoteReceipt(string identity, DateTimeOffset remoteTimeStamp) =>
            ((IChainOfCustody)ChainOfCustody).CommitRemoteReceipt(identity, remoteTimeStamp);
    }
}
