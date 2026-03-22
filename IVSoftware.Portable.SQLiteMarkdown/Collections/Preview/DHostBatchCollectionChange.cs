using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class DHostBatchCollectionChange : DisposableHost
    {
        IList 
            _listB4 = null!,
            _listFTR = null!;

        /// <summary>
        /// Heuristically determines the simplest approach to reaching ListFTR from ListB4.
        /// </summary>
        /// <remarks>
        /// The batch event is one of:
        /// - Simple Reset
        /// - Single or Multiple Add Only
        /// - Single or Multiple Remove Only
        /// - Single Relace only, or
        /// - IList consisting or multiple, single, indexed Replace events.
        /// The probably response when a consumer inspects NewItems and sees multiple replace events is a reset + add.
        /// </remarks>
        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            var before = (IList)_listB4;
            var after = _listFTR;

            var batchArgs = before.Diff(after);

            var snapshot = e.Keys.ToDictionary(
                key => key,
                key => e[key]);

            snapshot["FinalList"] = _listFTR;

            var eBatch = new BatchFinalDisposeEventArgs(
                e.ReleasedSenders,
                snapshot,
                batchArgs,
                _listFTR);

            base.OnFinalDispose(eBatch);
        }

        [Canonical]
        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
        {
            if (sender is IList list)
            {
                InitializeToken(list);
                return base.GetToken(list, properties);
            }
            throw new ArgumentException($"Expecting {nameof(sender)} is {nameof(INotifyCollectionChanged)}.");
        }

        public new IDisposable GetToken(object sender, string key, object value)
        {
            if (sender is IList list)
            {
                InitializeToken(list);
                return base.GetToken(list, key, value);
            }
            throw new ArgumentException($"Expecting {nameof(sender)} is {nameof(INotifyCollectionChanged)}.");
        }

        public new IDisposable GetToken(string key, object value)
        {
            throw new NotSupportedException();
        }

        void InitializeToken(IList list)
        {
            _listB4 = list.Cast<object>().ToList();
            _listFTR = list;
        }
    }

    internal class BatchFinalDisposeEventArgs : FinalDisposeEventArgs
    {
        public BatchFinalDisposeEventArgs(
            IReadOnlyCollection<object> releasedSenders,
            IReadOnlyDictionary<string, object> snapshot,
            NotifyCollectionChangingEventArgs batchEventArgs,
            IList finalList)
            : base(releasedSenders, snapshot)
        {
            BatchEventArgs = batchEventArgs;
            FinalList = finalList;
        }

        public NotifyCollectionChangingEventArgs BatchEventArgs { get; }

        public IList FinalList { get; }
    }
}
