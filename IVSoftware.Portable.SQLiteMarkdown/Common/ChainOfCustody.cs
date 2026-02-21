using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    public class ChainOfCustody : IEnumerable<KeyValuePair<string, ChainOfCustodyToken>>
    {
        public DateTimeOffset Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Local principal presets its identity (e.g., guid).
        /// </summary>
        public DateTimeOffset GetToken(string identity)
        {
            var now = DateTime.UtcNow;

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
                    token.LocalTimestamp = now;;
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

        public ChainOfCustodyToken CommitToken(string identity, DateTimeOffset remoteTimeStamp)
        {
            if (_coc.TryGetValue(identity, out var token))
            {
                token.RemoteTimestamp = remoteTimeStamp;
                return token;
            }
            else
            {
                this.ThrowHard<InvalidOperationException>();
                return null!;
            }
        }

        private readonly Dictionary<string, ChainOfCustodyToken> _coc = new();

        public IEnumerator<KeyValuePair<string, ChainOfCustodyToken>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, ChainOfCustodyToken>>)_coc).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_coc).GetEnumerator();
        }
    }

    public class ChainOfCustodyToken
    {
        /// <summary>
        /// The local device commits an edit and asserts a claim of ownership.
        /// </summary>
        public DateTimeOffset LocalTimestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The timestamp of the remote file transaction as obtained in the upload receipt meta.
        /// </summary>
        public DateTimeOffset? RemoteTimestamp { get; set; }

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
