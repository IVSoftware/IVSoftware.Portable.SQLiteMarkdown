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
    class AuthorityEpochProvider : DisposableHost
    {
        public IDisposable BeginAuthority(Enum authority) => GetToken(authority);

        public IDisposable GetToken(Enum authority)
            => base.GetToken(sender: authority, properties: null);

        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
        {
            sender =
                (sender is Enum authority)
                ? authority
                : 0;
            return base.GetToken(sender, null, properties);
        }

        public new IDisposable GetToken(string key, object value)
            => base.GetToken((Enum)(object)0, key, value);

        public new IDisposable GetToken(object sender, string? key, object? value)
        {
            sender =
                (sender is Enum authority)
                ? authority
                : 0;
            return base.GetToken(sender, key, value);
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
            Authority = FsmReserved.NoAuthority;
            base.OnFinalDispose(e);
        }

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
