using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    partial class TemporalAffinityQFModel 
        : IChainOfCustody
    {
        public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow.WithTestability();

        /// <summary>
        /// SQLite backing store where column name is ChainOfCustody.
        /// </summary>
        [SQLite.Column("ChainOfCustody"), JsonProperty("ChainOfCustody")]
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

        [SQLite.Ignore, JsonIgnore]
        public ChainOfCustody ChainOfCustody { get; protected set; } = new();

        public Task<DateTimeOffset> CommitLocalEdit(string identity) =>
            ((IChainOfCustody)ChainOfCustody).CommitLocalEdit(identity);

        public Task<ChainOfCustodyToken> CommitRemoteReceipt(string identity, DateTimeOffset remoteTimeStamp) =>
            ((IChainOfCustody)ChainOfCustody).CommitRemoteReceipt(identity, remoteTimeStamp);
    }
}
