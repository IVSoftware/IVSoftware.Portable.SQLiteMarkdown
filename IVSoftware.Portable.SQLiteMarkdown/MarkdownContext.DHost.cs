using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Indicates that the MDC is pushing changes onto the NetProjection.
        /// </summary>
        /// <remarks>
        /// Acts as a circularity guard so that keeps these
        /// changes from appearing to be originated on the UI.
        /// </remarks>
        public IDisposable BeginAuthorityClaim() => DHostCollectionChangeAuthority.GetToken();

        protected DHostClaimAuthorityProvider DHostCollectionChangeAuthority { get; } = new();

        protected class DHostClaimAuthorityProvider : DisposableHost
        {
            protected override void OnBeginUsing(BeginUsingEventArgs e)
            {
                Authority = NotifyCollectionChangedEventAuthority.MarkdownContext;
                base.OnBeginUsing(e);   // <- Last
            }
            protected override void OnFinalDispose(FinalDisposeEventArgs e)
            {
                base.OnFinalDispose(e); // <- First
                Authority = NotifyCollectionChangedEventAuthority.NetProjection;
            }
            public NotifyCollectionChangedEventAuthority Authority { get; protected set; }
        }

        public IDisposable BeginResetWithEventSuppression() => DHostResetWithEventSuppression.GetToken();

        /// <summary>
        /// Requires initialization from subclass.
        /// </summary>
        protected DHostResetProvider DHostResetWithEventSuppression
        {
            get
            {
                if (_dhostReset is null)
                {
                    // Has no actions, but is better than null.
                    _dhostReset = new DHostResetProvider(this, []);
                }
                return _dhostReset;
            }
            set
            {
                _dhostReset = value;
            }
        }
        DHostResetProvider? _dhostReset = null;

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
        protected class DHostResetProvider : DisposableHost
        {
            internal DHostResetProvider(IMarkdownContext mdc, IEnumerable<Action> onResetActions)
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
                    this.Advisory("Reentrant BeginReset suppressed.");
                    return;
                }

                try
                {
                    if (_onResetActions.Length == 0)
                    {
                        this.Advisory(
                            $"Starting {nameof(DHostResetProvider)} epoch with no reset actions.");
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

                    using (_mdc.BeginAuthorityClaim())
                    {
                        foreach (var action in _onResetActions)
                        {
                            action();
                        }
                    }
                }
                finally
                {
                    _lock = 0;
                }
            }
        }
    }
}
