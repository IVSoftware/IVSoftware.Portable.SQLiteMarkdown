using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext
    {
        /// <summary>
        /// Reference counter for Busy property.
        /// </summary>
        /// <remarks>
        /// This does not hold the awaiter and should be used
        /// for visual activity indicator only.
        /// </remarks>
        public DisposableHost DHostBusy
        {
            get
            {
                if (_dhostBusy is null)
                {
                    _dhostBusy = new DisposableHost();
                    _dhostBusy.BeginUsing += (sender, e) =>
                    {
                        Busy = true;
                        OnPropertyChanged(nameof(Busy));
                    };
                    _dhostBusy.FinalDispose += (sender, e) =>
                    {
                        Busy = false;
                        OnPropertyChanged(nameof(Busy));
                    };
                }
                return _dhostBusy;
            }
        }
        DisposableHost? _dhostBusy = null;

        public DisposableHost DHostSelfIndexing { get; } = new();

        protected AuthorityEpochProvider AuthorityEpochProvider { get; } = new();

        /// <summary>
        /// Identifies provenance of INCC.
        /// </summary>
        /// <remarks>
        /// Acts as an authority monitor and circularity guard for DDX between collections.
        /// </remarks>
        public IDisposable BeginCollectionChangeAuthority(CollectionChangeAuthority authority) => AuthorityEpochProvider.GetToken(authority);

#if false
        protected DHostAuthorityEpochProvider DHostAuthorityEpoch { get; } = new();

        [DebuggerDisplay("Count={ReferenceCount} Authority={Authority}")]
        protected class DHostAuthorityEpochProvider : DisposableHost
        {
            public IDisposable GetToken(CollectionChangeAuthority authority)
                => base.GetToken(sender: authority, properties: null);

            public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
            {
                sender = 
                    (sender is CollectionChangeAuthority authority) 
                    ? authority 
                    : 0;
                return base.GetToken(sender , null, properties);
            }

            public new IDisposable GetToken(string key, object value)
                => base.GetToken((CollectionChangeAuthority)0, key, value);

            public new IDisposable GetToken(object sender, string? key, object? value)
            {
                sender =
                    (sender is CollectionChangeAuthority authority)
                    ? authority
                    : 0;
                return base.GetToken(sender, key, value);
            }
            protected override void OnBeginUsing(BeginUsingEventArgs e)
            {
                base.OnBeginUsing(e);
                if(e.AutoDisposableContext.Sender is CollectionChangeAuthority authority)
                {
                    Authority = authority;
                }
                else
                {
                    this.ThrowFramework<InvalidOperationException>(
                        $"{nameof(Authority)} must be specified as token sender that is {nameof(CollectionChangeAuthority)}.");
                }
            }
            protected override void OnFinalDispose(FinalDisposeEventArgs e)
            {
                Authority = 0;
                base.OnFinalDispose(e);
            }
            public CollectionChangeAuthority Authority { get; private set; } = 0;
        }
#endif
    }
}
