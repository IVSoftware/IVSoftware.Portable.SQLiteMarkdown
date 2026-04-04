using IVSoftware.Portable.Collections.Preview;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IVSoftware.Portable.Collections.Modeled
{
    /// <summary>
    /// Provides suppression infrastructure for observable collections.
    /// </summary>
    /// <remarks>
    /// - Supplies the underlying suppression, snapshot, and coalescing mechanics
    ///   used to aggregate multiple changes into a single final notification.
    /// - Higher-level features such as Preview and Range semantics may build
    ///   on this foundation.
    /// - Does not implement Preview or Range behavior itself; it only enables
    ///   those patterns to be layered on top.
    /// </remarks>
    public class ModeledObservableCollection<T> 
        : ObservableCollection<T>
        , INotifyCollectionChangedSuppress<T>
    {        
        public ModeledObservableCollection(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)
        {
            EventScope = eventScope;
        }

        /// <summary>
        /// Defines the extent to which a preview handler may interact with a pending
        /// collection change proposal.
        /// </summary>
        /// <remarks>
        /// This enumeration constrains what a handler is permitted to do during the
        /// preview (Changing) phase. It does not describe the change itself, but rather
        /// the allowed level of participation in shaping or rejecting it.
        ///
        /// - ReadOnly   : Observe only. No modification or cancellation is permitted.
        /// - CancelOnly : The proposal may be rejected but not altered.
        /// - FullControl: The proposal may be rewritten or rejected entirely.
        ///
        /// These flags are enforced by the preview pipeline. Handlers opting into
        /// higher scopes assume responsibility for producing a valid and internally
        /// consistent change contract.
        /// </remarks>
        public NotifyCollectionChangeScope EventScope { get; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (DHostModelExchangeAuthority.IsZero())
            {
                base.OnCollectionChanged(e);
            }
        }
        public IDisposable BeginSuppress() => DHostModelExchangeAuthority.GetToken(this);

        public void CancelSuppress() => DHostModelExchangeAuthority.CancelSuppressNotify();
        public ModelDataExchangeAuthority Phase => DHostModelExchangeAuthority.Authority;

        public ModelDataExchangeAuthorityProvider<T> DHostModelExchangeAuthority
        {
            get
            {
                if (_dhostModelExchangeAuthorityProvider is null)
                {
                    _dhostModelExchangeAuthorityProvider = new ModelDataExchangeAuthorityProvider<T>(this);
                    _dhostModelExchangeAuthorityProvider.FinalDispose += (sender, e) => OnFinalCoalesce((ModelDateExchangeFinalDisposeEventArgs)e);
                }
                return _dhostModelExchangeAuthorityProvider;
            }
        }
        ModelDataExchangeAuthorityProvider<T>? _dhostModelExchangeAuthorityProvider = null;
        private void OnFinalCoalesce(ModelDateExchangeFinalDisposeEventArgs e)
        {
            OnCollectionChanged((e.Digest));
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public new IEnumerator<T> GetEnumerator()
            => DHostModelExchangeAuthority.IsZero()
            ? base.GetEnumerator()
            : DHostModelExchangeAuthority.Snapshot.GetEnumerator();
        public new int Count
            => DHostModelExchangeAuthority.IsZero()
            ? base.Count
            : DHostModelExchangeAuthority.Snapshot.Count;
    }
}
