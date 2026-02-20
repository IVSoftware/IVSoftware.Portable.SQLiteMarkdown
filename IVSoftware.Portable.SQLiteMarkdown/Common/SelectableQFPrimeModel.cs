using IVSoftware.Portable.Common.Exceptions;
using Newtonsoft.Json;
using System;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    public interface IChainOfCustody
    {
        DateTime Created { get; }
        string ChainOfCustody { get; }
    }

    public partial class SelectableQFPrimeModel 
        : SelectableQFModel
        , IChainOfCustody
    {
        public DateTime Created { get; set; }

        [SQLite.Column("ChainOfCustody")]
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
        string IChainOfCustody.ChainOfCustody 
        { 
            get => ChainOfCustodyJSON;
        }
    }
}
