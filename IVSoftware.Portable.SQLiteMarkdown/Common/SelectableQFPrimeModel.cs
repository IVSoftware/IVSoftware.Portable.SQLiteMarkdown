using IVSoftware.Portable.Common.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    public partial class SelectableQFPrimeModel 
        : SelectableQFModel
        , IGenesis
        , IPositional
        , IUtcEpoch
        , ICustomProperties
        , IChainOfCustody
    {
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// General purpose optional date-time epoch.
        /// </summary>
        public DateTimeOffset? UtcStart { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UtcEnd { get; set; } = DateTimeOffset.Now;

        public long Position
        {
            get
            { 
                if(_position == 0)
                {
                    _position = Created.UtcTicks;
                }
                return _position; 
            }
            set
            {
                if (!Equals(_position, value))
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }
        long _position = 0;

        public IDictionary<string, string?> CustomProperties { get; protected set; } = new Dictionary<string, string?>();


        [SQLite.Column("ChainOfCustody"), JsonProperty]
        public string ChainOfCustodyJSON 
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
