using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using System;
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


        /// <summary>
        /// Identifies provenance of INCC.
        /// </summary>
        /// <remarks>
        /// Acts as an authority monitor and circularity guard for DDX between collections.
        /// </remarks>
        public IDisposable BeginAuthority(CollectionChangeAuthority authority) => DHostAuthorityClaim.GetToken(authority);

        protected DHostAuthorityEpochProvider DHostAuthorityClaim { get; } = new();

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
                base.OnBeginUsing(e);   // <- Last
            }
            protected override void OnFinalDispose(FinalDisposeEventArgs e)
            {
                base.OnFinalDispose(e); // <- First
            }
            public CollectionChangeAuthority Authority =>
                Tokens.LastOrDefault()?.Sender is CollectionChangeAuthority authority
                ? authority
                : 0;
        }

        #region R E S E T    E P O C H
        public IDisposable BeginResetEpoch() => DHostResetEpoch.GetToken();

        /// <summary>
        /// Requires initialization from subclass.
        /// </summary>
        protected DHostResetEpochProvider DHostResetEpoch
        {
            get
            {
                if (_dhostReset is null)
                {
                    // Has no actions, but is better than null.
                    _dhostReset = new DHostResetEpochProvider(this, []);
                }
                return _dhostReset;
            }
            set
            {
                _dhostReset = value;
            }
        }
        DHostResetEpochProvider? _dhostReset = null;

        /// <summary>
        /// Reset epoch that suppresses collection change propagation during reset.
        /// </summary>
        /// <remarks>
        /// This host defines a reset epoch. Nested or concurrent BeginUsing transitions
        /// are suppressed to protect the epoch boundary, while the reset actions are
        /// executed only when the host returns to depth zero (FinalDispose).
        ///
        /// Delegate actions should inspect collection-change authority when necessary
        /// (for example, NetProjection vs Canonical) to determine whether work should
        /// actually execute.
        /// </remarks>
        protected class DHostResetEpochProvider : DisposableHost
        {
            internal DHostResetEpochProvider(IMarkdownContext mdc, IEnumerable<Action> onResetActions)
            {
                if (mdc is null)
                {
                    throw new ArgumentNullException(
                        "MDC cannot be null because circularity is certain otherwise.");
                }

                _mdc = mdc;

                List<Action> actions = new();
                foreach (var action in onResetActions ?? [])
                {
                    if (action is null)
                    {
                        this.ThrowHard<NullReferenceException>(
                            "Reset actions cannot contain null.");
                    }
                    actions.Add(action);
                }

                _onResetActions = actions.ToArray();
            }

            private readonly IMarkdownContext _mdc;
            private readonly Action[] _onResetActions;

            /// <summary>
            /// Guards the BeginUsing / FinalDispose transition edges.
            /// Only one thread may execute these transitions at a time.
            /// </summary>
            private int _lock;

            protected override void OnBeginUsing(BeginUsingEventArgs e)
            {
                // Defensive guard against reentrant or concurrent BeginUsing transitions.
                // This condition normally indicates a misuse of the reset paradigm and is avoidable.
                //
                // In correct usage, reset actions should consult collection-change
                // authority before executing work. For example:
                // EXAMPLE:
                // if (DHostCollectionChangeAuthority.Authority
                //     == NotifyCollectionChangedEventAuthority.NetProjection)
                // {
                //     ...
                // }
                //
                // When that pattern is followed, reentrant resets are naturally avoided.
                if (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                {
                    Debug.Fail($@"ADVISORY - This should really be avoided.");
                    this.Advisory("Reentrant BeginReset suppressed.");
                    return;
                }

                try
                {
                    if (_onResetActions.Length == 0)
                    {
                        this.Advisory(
                            $"Starting {nameof(DHostResetEpochProvider)} epoch with no reset actions.");
                    }

                    base.OnBeginUsing(e);
                }
                finally
                {
                    _lock = 0;
                }
            }
            protected override void OnFinalDispose(FinalDisposeEventArgs e)
            {
                if (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
                {
                    this.Advisory("Concurrent reset suppressed.");
                    return;
                }
                try
                {
                    base.OnFinalDispose(e);
                    foreach (var action in _onResetActions)
                    {
                        action();
                    }
                }
                finally
                {
                    _lock = 0;
                }
            }
        }
        #endregion R E S E T    E P O C H
    }
}
