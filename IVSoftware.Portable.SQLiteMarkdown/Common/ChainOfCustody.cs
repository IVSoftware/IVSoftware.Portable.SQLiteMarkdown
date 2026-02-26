using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    /// <summary>
    /// Maintains per-principal local and remote epoch timestamps for simple chain-of-custody tracking.
    /// </summary>
    /// <remarks>
    /// Designed for a single local principal mutating this instance.
    /// Concurrency is guarded by a single SemaphoreSlim to protect against
    /// overlapping async flows (e.g., UI edits and cloud callbacks).
    /// This type is intentionally coarse-grained and non-reentrant.
    /// Global serialization is acceptable because mutation frequency is low
    /// and custody integrity is prioritized over throughput.
    /// </remarks>

    public class ChainOfCustody : IChainOfCustody
    {
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow.WithTestability();

        /// <summary>
        /// Local principal presets its identity (e.g., guid).
        /// </summary>
        public async Task<DateTimeOffset> CommitLocalEdit(string identity)
        {
            try
            {
                await _busy.WaitAsync();
                var now = DateTimeOffset.UtcNow;

                if(!_coc.TryGetValue(identity, out var token))
                {
                    token = new();
                    _coc[identity] = token;
                }
                else
                {
                    // Otherwise, a "closed" epoch will have a remote receipt
                    // timestamped more recently than the local timestamp.
                    if(token.LocalTimestamp <= token.RemoteTimestamp)
                    {
                        token.LocalTimestamp = now;
                        _coc[identity] = token;
                    }
                    else
                    {   /* G T K */
                        // N O O P
                        // Local change epoch is already in progress.
                    }
                }
                return token.LocalTimestamp;
            }
            catch (Exception ex)
            {
                this.RethrowHard(ex);
                return DateTimeOffset.MinValue;
            }
            finally
            {
                _busy.Release();
            }
        }

        public async Task<ChainOfCustodyToken> CommitRemoteReceipt(string identity, DateTimeOffset remoteTimeStamp)
        {
            try
            {
                await _busy.WaitAsync();
                if (_coc.TryGetValue(identity, out var token))
                {
                    token.RemoteTimestamp = remoteTimeStamp;
                    return token;
                }
                else
                {
                    throw new InvalidOperationException($"{nameof(CommitLocalEdit)} must be called first. No local change epoch exists.");
                }
            }
            catch (Exception ex)
            {
                this.RethrowHard(ex);
                return null!;
            }
            finally
            {
                _busy.Release();
            }
        }

        private readonly SemaphoreSlim _busy = new SemaphoreSlim(1, 1);

        ChainOfCustody IChainOfCustody.ChainOfCustody => this;

        // Json.NET will populate this.
        // Not publicly writable.
        [JsonProperty]
        private Dictionary<string, ChainOfCustodyToken> Coc
        {
            get => _coc;
            set => _coc = value ?? new();
        }
        private Dictionary<string, ChainOfCustodyToken> _coc = new();
    }

    /// <summary>
    /// Represents a per-principal epoch with local authorship and remote settlement timestamps.
    /// </summary>
    /// <remarks>
    /// LocalTimestamp records the most recent local edit assertion.
    /// RemoteTimestamp represents the trusted cloud receipt for that edit.
    /// An epoch is considered "open" when LocalTimestamp > RemoteTimestamp.
    /// ModifiedFlags allow merge granularity without relying solely on time ordering.
    /// </remarks>
    public class ChainOfCustodyToken
    {
        /// <summary>
        /// The local device commits an edit and asserts a claim of ownership.
        /// </summary>
        public DateTimeOffset LocalTimestamp { get; set; } = DateTimeOffset.UtcNow.WithTestability();

        /// <summary>
        /// The timestamp of the remote file transaction as obtained from 
        /// the upload receipt metadata (the trusted authority).
        /// </summary>
        public DateTimeOffset RemoteTimestamp { get; set; } = DateTimeOffset.MinValue;

        /// <summary>
        /// User-mappable flags that may be used for merge granularity.
        /// </summary>
        /// <remarks>
        /// When INPC is mapped to specific flags, it means that
        /// - PrincipleA can change PropertyA
        /// - PrincipleB can change PropertyB
        /// - The merge will be lossless even if epochs overlap.
        /// </remarks>
        public long ModifiedFlags { get; set; }
    }
}
