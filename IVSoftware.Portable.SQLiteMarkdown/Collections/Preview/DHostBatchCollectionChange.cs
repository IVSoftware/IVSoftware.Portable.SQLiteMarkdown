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
                reason: NotifyCollectionChangeReason.None);
    }
}
