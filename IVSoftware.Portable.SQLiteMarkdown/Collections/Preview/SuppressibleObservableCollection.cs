using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class SuppressibleObservableCollection<T> 
        : ObservableCollection<T>
        , INotifyCollectionChangedSuppress<T>
    {        
        public SuppressibleObservableCollection(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)
        {
            EventScope = eventScope;
        }
        public NotifyCollectionChangeScope EventScope { get; }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (DHostSuppress.IsZero())
            {
                base.OnCollectionChanged(e);
            }
        }
        protected override void ClearItems()
        {
            base.ClearItems();
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
