using IVSoftware.Portable.Common.Collections;
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
    public class ObservableModeledCollectionOR<T> 
        : ObservableCollection<T>
        , IModeledNotifyCollectionChanged<T>
    {        
        public ObservableModeledCollectionOR(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)
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
            if (DHostModelEpoch.IsZero())
            {
                base.OnCollectionChanged(e);
            }
        }
        public IDisposable RequestModelEpochAuthority(ModelDataExchangeAuthority authority, IList source)
            => DHostModelEpoch.GetToken(authority, source);

        public void CancelSuppress() => DHostModelEpoch.CancelSuppressNotify();
        public ModelDataExchangeAuthority Phase => DHostModelEpoch.Authority;

        public ModelDataExchangeAuthorityProvider<T> DHostModelEpoch
        {
            get
            {
                if (_dhostDHostModelEpoch is null)
                {
                    _dhostDHostModelEpoch = new ModelDataExchangeAuthorityProvider<T>();
                    _dhostDHostModelEpoch.FinalDispose += (sender, e) => OnDHostModelEpochFinalizing((ModelDataExchangeFinalDisposeEventArgs)e);
                }
                return _dhostDHostModelEpoch;
            }
        }
        ModelDataExchangeAuthorityProvider<T>? _dhostDHostModelEpoch = null;
        protected virtual void OnDHostModelEpochFinalizing(ModelDataExchangeFinalDisposeEventArgs e)
        {
            OnCollectionChanged((e.Digest));
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public new IEnumerator<T> GetEnumerator()
            => DHostModelEpoch.IsZero()
            ? base.GetEnumerator()
            : DHostModelEpoch.Snapshot.GetEnumerator();
        public new int Count
        {
            get
            {
                switch (DHostModelEpoch.Authority)
                {
                    case ModelDataExchangeAuthority.CollectionDeferred when !DHostModelEpoch.IsDisposing:
                        return DHostModelEpoch.Snapshot.Count;
                    case ModelDataExchangeAuthority.ModelDeferred when !DHostModelEpoch.IsDisposing:
                        return DHostModelEpoch.Snapshot.Count;
                    default:
                        return base.Count;
                }
            }
        }
    }
}
