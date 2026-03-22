using IVSoftware.Portable.Disposable;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class DHostBatchCollectionChange : DisposableHost
    {
        public NotifyCollectionChangingEventArgs eBatch = new (
                action: NotifyCollectionChangeAction.Replace,                
                reason: NotifyCollectionChangeReason.Batch,
                oldItems: new List<CollectionChangingEventArgs>(),
                newItems: new List<CollectionChangingEventArgs>());
        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            base.OnBeginUsing(e);
            eBatch.OldItems!.Clear();
            eBatch.NewItems!.Clear();
        }
    }
}
