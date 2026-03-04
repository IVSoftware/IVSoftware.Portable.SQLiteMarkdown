using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using System;
using System.Collections.Generic;
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


        public IDisposable BeginAuthorityClaim() => DHostClaimAuthority.GetToken();

        [Probationary]
        protected DisposableHost DHostClaimAuthority
        {
            get
            {
                if (_dhostClaimAuthority is null)
                {
                    _dhostClaimAuthority = new DisposableHost();
                    _dhostClaimAuthority.BeginUsing += (sender, e)
                        => CollectionChangeAuthority = NotifyCollectionChangedEventAuthority.MarkdownContext;
                    _dhostClaimAuthority.FinalDispose += (sender, e)
                        => CollectionChangeAuthority = NotifyCollectionChangedEventAuthority.NetProjection;
                }
                return _dhostClaimAuthority;
            }
        }
        DisposableHost? _dhostClaimAuthority = null;

        public NotifyCollectionChangedEventAuthority CollectionChangeAuthority { get; private set; }
    }
}
