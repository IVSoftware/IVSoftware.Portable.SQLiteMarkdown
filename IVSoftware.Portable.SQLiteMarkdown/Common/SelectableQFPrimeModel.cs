using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Threading;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    [Table("items")]
    public partial class SelectableQFPrimeModel 
        : SelectableQFModel
        , IGenesis
        , ICustomProperties
        , IChainOfCustody
    {
        public DateTimeOffset Created
        {
            get
            {
                var e = new AwaitedEventArgs
                {
                   { nameof(Created), _created }
                };
                // Expose just the string, not the item, as sender.
                nameof(Created).OnAwaited();
                return (DateTimeOffset) e[nameof(Created)];
            }
            set
            {
                if (!Equals(_created, value))
                {
                    _created = value;
                    OnPropertyChanged();
                }
            }
        }
        DateTimeOffset _created = DateTimeOffset.UtcNow;

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
