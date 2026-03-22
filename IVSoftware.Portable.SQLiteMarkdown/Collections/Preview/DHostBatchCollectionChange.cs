using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class DHostBatchCollectionChange : DisposableHost
    {
        INotifyCollectionChanged _listB4 = null!;
        IList _listFTR = null!;
        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            base.OnBeginUsing(e);
        }
        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            base.OnFinalDispose(e);
        }

        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
        {
            throw new NotImplementedException("ToDo");
        }

        public new IDisposable GetToken(string key, object value)
        {
            throw new NotImplementedException("ToDo");
        }
        public new IDisposable GetToken(object sender, string key, object value)
        {
            throw new NotImplementedException("ToDo");
        }

        [Canonical]
        public IDisposable GetToken(INotifyCollectionChanged listB4)
        {
            _listB4 = listB4;
            return base.GetToken();
        }
    internal class BatchFinalDisposeEventArgs : FinalDisposeEventArgs
    {
        public BatchFinalDisposeEventArgs(
            IReadOnlyCollection<object> releasedSenders, 
            IReadOnlyDictionary<string, object> snapshot) : base(releasedSenders, snapshot)
        {
            throw new NotImplementedException("ToDo");
        }

        public NotifyCollectionChangingEventArgs BatchEventArgs { get; }
    }
}
