using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// Reset epoch with collection changed authority.
        /// </summary>
        protected class DHostResetProvider : DisposableHost
        {
            internal DHostResetProvider(IMarkdownContext mdc, IEnumerable<Action> onResetActions)
            {
                if (mdc is null)
                {
                    // Rare circumstance where throwing a System.Exception is not optional.
                    throw new ArgumentNullException(
                        "MDC cannot be null because circularity is certain otherwise.");
                }
                _mdc = mdc;

                List<Action> nonNullActions = new();
                foreach (var action in onResetActions)
                {
                    if (action is null)
                    {
                        this.ThrowHard<NullReferenceException>("Reset actions cannot be null");
                    }
                    else
                    {
                        nonNullActions.Add(action);
                    }
                }
                _onResetActions = nonNullActions.ToArray();
            }
            IMarkdownContext _mdc;
            private Action[] _onResetActions;

            public IDisposable GetToken()
            {
                return base.GetToken(sender: NotifyCollectionChangedEventAuthority.MarkdownContext);
            }
            protected override void OnBeginUsing(BeginUsingEventArgs e)
            {
                if (_onResetActions.Length == 0)
                {
                    this.Advisory($"Starting {nameof(DHostResetProvider)} epoch with no dispose actions.");
                }
                base.OnBeginUsing(e);
            }
            protected override void OnFinalDispose(FinalDisposeEventArgs e)
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
        }
    }
}
