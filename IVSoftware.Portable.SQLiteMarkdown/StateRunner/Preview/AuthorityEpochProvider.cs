using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IVSoftware.Portable.StateRunner.Preview
{
    internal enum StdAuthorityProperty
    {
        Snapshot,
    }
    [DebuggerDisplay("Count={ReferenceCount} Authority={Authority}")]
    internal abstract class AuthorityEpochProvider : DisposableHost
    {
        public IDisposable BeginAuthority(Enum authority, ICollection snapshot)
        {
            var disp = GetToken(sender: authority, new Dictionary<string, object> 
            {
                { nameof(StdAuthorityProperty.Snapshot), snapshot },
            });
            return disp;
        }
        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            if (e.AutoDisposableContext.Sender is Enum authority)
            {
                Authority = authority;
#if DEBUG
                Debug.WriteLine($"260403.B BEGIN AUTHORITY {authority.ToFullKey()}");
#endif
                base.OnBeginUsing(e);
            }
            else
            {
                this.ThrowFramework<InvalidOperationException>(
                    $"{nameof(Authority)} must be specified as token sender that is {nameof(Enum)}.");
            }
        }
        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            IsDisposing = true;
            try
            {
                base.OnFinalDispose(e);
            }
            finally
            {
                IsDisposing = false;
                Authority = FsmReserved.NoAuthority;
            }
        }
        public bool IsDisposing { get; private set; } = false;

        /// <summary>
        /// The primary authority for this epoch.
        /// </summary>
        /// <remarks>
        /// This value is captured by the BeginUsing override when the count goes from 0->1.
        /// </remarks>
        public Enum Authority { get; private set; } = FsmReserved.NoAuthority;

        /// <summary>
        /// Returns true if any active token carries the specified authority.
        /// </summary>
        /// <remarks>
        /// Authority is inferred from token Sender values. Uses a snapshot of Tokens,
        /// so enumeration is safe without additional locking. Returns false when no
        /// matching participant is present.
        /// </remarks>
        public bool HasAuthority(Enum authority) =>
            Tokens.Any(_ => Equals(_.Sender, authority));
    }
}
