using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.StateRunner.Preview
{
    [DebuggerDisplay("Count={ReferenceCount} Authority={Authority}")]
    class AuthorityEpochProvider<T> : DisposableHost where T : struct, Enum
    {
        static T NoAuthority => (T)(object)FsmReserved.NoAuthority;

        public IDisposable BeginAuthority(T authority) => GetToken(authority);

        public IDisposable GetToken(T authority)
            => base.GetToken(sender: authority, properties: null);

        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
        {
            sender =
                (sender is T authority)
                ? authority
                : NoAuthority;
            return base.GetToken(sender, properties);
        }

        public new IDisposable GetToken(string key, object value)
            => base.GetToken(NoAuthority, key, value);

        public new IDisposable GetToken(object sender, string? key, object? value)
        {
            sender =
                (sender is T authority)
                ? authority
                : NoAuthority;
            return base.GetToken(sender, key, value);
        }

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            base.OnBeginUsing(e);

            if (e.AutoDisposableContext.Sender is T authority)
            {
                Authority = authority;
            }
            else
            {
                this.ThrowFramework<InvalidOperationException>(
                    $"{nameof(Authority)} must be specified as token sender of type {typeof(T).Name}.");
            }
        }

        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            Authority = NoAuthority;
            base.OnFinalDispose(e);
        }

        /// <summary>
        /// The primary authority for this epoch.
        /// </summary>
        /// <remarks>
        /// Captured when the reference count transitions from 0 to 1.
        /// </remarks>
        public T Authority { get; private set; } = NoAuthority;

        /// <summary>
        /// Returns true if any active token carries the specified authority.
        /// </summary>
        public bool HasAuthority(T authority) =>
            Tokens.Any(_ => _?.Sender is T a && EqualityComparer<T>.Default.Equals(a, authority));
    }
}