using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IVSoftware.Portable.StateRunner.Preview
{
    [DebuggerDisplay("Count={ReferenceCount} Authority={Authority}")]
    class AuthorityEpochProvider<T> : AuthorityEpochProvider where T : struct, Enum
    {
        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            if (e.AutoDisposableContext.Sender is T authority)
            {
                base.OnBeginUsing(e);
            }
            else
            {
                this.ThrowFramework<InvalidOperationException>(
                    $"{nameof(Authority)} must be specified as token sender of type {typeof(T).Name}.");
            }
        }
        /// <summary>
        /// The primary authority for this epoch.
        /// </summary>
        /// <remarks>
        /// Captured when the reference count transitions from 0 to 1.
        /// </remarks>
        public new T Authority => (T)base.Authority;

        /// <summary>
        /// Returns true if any active token carries the specified authority.
        /// </summary>
        public bool HasAuthority(T authority) =>
            Tokens.Any(_ => _?.Sender is T a && EqualityComparer<T>.Default.Equals(a, authority));
    }
}