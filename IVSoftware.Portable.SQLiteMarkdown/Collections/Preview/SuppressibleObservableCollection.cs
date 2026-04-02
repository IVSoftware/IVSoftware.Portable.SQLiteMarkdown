using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace IVSoftware.Portable.Collections.Preview
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
    internal class SuppressibleObservableCollection<T> 
        : ObservableCollection<T>
        , INotifyCollectionChangedSuppress<T>
    {        
        public SuppressibleObservableCollection(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)
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
            if (DHostSuppress.IsZero())
            {
                base.OnCollectionChanged(e);
            }
        }
        public IDisposable BeginSuppress() => DHostSuppress.GetToken(this);

        public void CancelSuppress() => DHostSuppress.CancelSuppressNotify();
        public SuppressionPhase Phase => DHostSuppress.Phase;

        public DHostSuppress<T> DHostSuppress
        {
            get
            {
                if (_dhostSuppress is null)
                {
                    _dhostSuppress = new DHostSuppress<T>();
                    _dhostSuppress.FinalDispose += (sender, e) => OnFinalCoalesce((CoalescingFinalDisposeEventArgs)e);
                }
                return _dhostSuppress;
            }
        }
        DHostSuppress<T>? _dhostSuppress = null;
        private void OnFinalCoalesce(CoalescingFinalDisposeEventArgs e)
        {
            OnCollectionChanged((e.Coalesced));
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public new IEnumerator<T> GetEnumerator()
            => DHostSuppress.IsZero()
            ? base.GetEnumerator()
            : DHostSuppress.Snapshot.GetEnumerator();
        public new int Count
            => DHostSuppress.IsZero()
            ? base.Count
            : DHostSuppress.Snapshot.Count;
    }
}
