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
        public IDisposable BeginAuthorityClaim() => DHostClaimAuthority.GetToken();
        public NotifyCollectionChangedEventAuthority CollectionChangeAuthority { get; private set; }
        protected DisposableHost DHostClaimAuthority
        {
            get
            {
                if (_dhostClaimAuthority is null)
                {
                    _dhostClaimAuthority = new DisposableHost();

                    _dhostClaimAuthority.BeginUsing += (sender, e) =>
                    {
                        CollectionChangeAuthority = NotifyCollectionChangedEventAuthority.MarkdownContext;
                    };

                    _dhostClaimAuthority.FinalDispose += (sender, e) =>
                    {
                        CollectionChangeAuthority = NotifyCollectionChangedEventAuthority.NetProjection;
                    };
                }
                return _dhostClaimAuthority;
            }
        }
        DisposableHost? _dhostClaimAuthority = null;

        /// <summary>
        /// Requires initialization from subclass.
        /// </summary>
        protected DHostResetProvider DHostReset
        {
            get
            {
                if (_dhostReset is null)
                {
                    // We need to return something...
                    _dhostReset = new DHostResetProvider([]);
                }
                return _dhostReset;
            }
            set => _dhostReset = value;
        }
        DHostResetProvider? _dhostReset = null;
        public IDisposable BeginResetWithEventSuppression() => DHostReset.GetToken();
        protected class DHostResetProvider : DisposableHost
        {
            public DHostResetProvider(IEnumerable<Action> onResetActions)
            {
                List<Action> nonNullActions = new();
                foreach (var action in onResetActions)
                {
                    if(action is null)
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
            private Action[] _onResetActions;
            protected override void OnBeginUsing(BeginUsingEventArgs e)
            {
                if(_onResetActions.Length == 0)
                {
                    this.Advisory($"Starting {nameof(DHostResetProvider)} epoch with no dispose actions.");
                }
                base.OnBeginUsing(e);
            }

            protected override void OnFinalDispose(FinalDisposeEventArgs e)
            {
                base.OnFinalDispose(e);
                foreach (var action in _onResetActions)
                {
                    action();
                }
            }
        }
    }
}
