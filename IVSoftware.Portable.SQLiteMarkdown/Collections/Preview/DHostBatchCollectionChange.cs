using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class DHostBatchCollectionChange : DisposableHost
    {
        INotifyCollectionChanged _listB4 = null!;
        IList _listFTR = null!;
        NotifyCollectionChangedEventHandler? _handler;
        bool _initialized;

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            base.OnBeginUsing(e);
        }
        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            if (_handler is not null)
            {
                _listB4.CollectionChanged -= _handler;
                _handler = null;
            }

            var before = (_listB4 as IList)?.Cast<object>().ToList() ?? new List<object>();
            var after = _listFTR.Cast<object>().ToList();

            var oldItems = before.Except(after).ToList();
            var newItems = after.Except(before).ToList();

            NotifyCollectionChangingEventArgs batchArgs;

            if (newItems.Count > 0 && oldItems.Count == 0)
            {
                batchArgs = new NotifyCollectionChangingEventArgs(
                    NotifyCollectionChangeAction.Add,
                    newItems,
                    NotifyCollectionChangeReason.Batch);
            }
            else if (oldItems.Count > 0 && newItems.Count == 0)
            {
                batchArgs = new NotifyCollectionChangingEventArgs(
                    NotifyCollectionChangeAction.Remove,
                    oldItems,
                    NotifyCollectionChangeReason.Batch);
            }
            else if (newItems.Count == 0 && oldItems.Count == 0)
            {
                batchArgs = new NotifyCollectionChangingEventArgs(
                    NotifyCollectionChangeAction.Reset,
                    NotifyCollectionChangeReason.Batch);
            }
            else
            {
                batchArgs = new NotifyCollectionChangingEventArgs(
                    NotifyCollectionChangeAction.Replace,
                    newItems,
                    oldItems,
                    NotifyCollectionChangeReason.Batch);
            }

            var snapshot = e.Keys.ToDictionary(
                key => key,
                key => e[key]);

            var eBatch = new BatchFinalDisposeEventArgs(
                e.ReleasedSenders,
                snapshot,
                batchArgs);

            base.OnFinalDispose(eBatch);
        }

        [Canonical]
        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
        {
            if (sender is INotifyCollectionChanged incc)
            {
                InitializeToken(incc);
                return base.GetToken(incc, properties);
            }
            throw new ArgumentException($"Expecting {nameof(sender)} is {nameof(INotifyCollectionChanged)}.");
        }

        public new IDisposable GetToken(object sender, string key, object value)
        {
            if (sender is INotifyCollectionChanged incc)
            {
                InitializeToken(incc);
                return base.GetToken(incc, key, value);
            }
            throw new ArgumentException($"Expecting {nameof(sender)} is {nameof(INotifyCollectionChanged)}.");
        }

        public new IDisposable GetToken(string key, object value)
        {
            throw new NotSupportedException();
        }

        void InitializeToken(INotifyCollectionChanged incc)
        {
            if (_initialized) return;
            _initialized = true;

            _listB4 = incc;

            if (incc is not IList list)
                throw new ArgumentException(nameof(incc));

            _listFTR = list.Cast<object>().ToList();

            _handler = (s, e) =>
            {
                _listFTR.Apply(e);
            };

            _listB4.CollectionChanged += _handler;
        }
    }

    internal class BatchFinalDisposeEventArgs : FinalDisposeEventArgs
    {
        public BatchFinalDisposeEventArgs(
            IReadOnlyCollection<object> releasedSenders,
            IReadOnlyDictionary<string, object> snapshot,
            NotifyCollectionChangingEventArgs batchEventArgs)
            : base(releasedSenders, snapshot)
        {
            BatchEventArgs = batchEventArgs;
        }

        public NotifyCollectionChangingEventArgs BatchEventArgs { get; }
    }
}
