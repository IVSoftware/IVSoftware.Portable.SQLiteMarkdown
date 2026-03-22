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
        INotifyCollectionChanged _listB4 = null!;
        IList _listFTR = null!;
        NotifyCollectionChangedEventHandler? _handler;
        bool _initialized;

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            base.OnBeginUsing(e);
        }

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

            if (newItems.Count == 0 && oldItems.Count == 0)
            {
                batchArgs = new NotifyCollectionChangingEventArgs(
                    NotifyCollectionChangeAction.Reset,
                    NotifyCollectionChangeReason.Batch);
            }
            else if (newItems.Count > 0 && oldItems.Count == 0)
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
            else if (newItems.Count == 1 && oldItems.Count == 1)
            {
                var newIndex = after.IndexOf(newItems[0]);
                var oldIndex = before.IndexOf(oldItems[0]);

                batchArgs = new NotifyCollectionChangingEventArgs(
                    NotifyCollectionChangeAction.Replace,
                    newItems[0],
                    oldItems[0],
                    newIndex,
                    NotifyCollectionChangeReason.Batch);
            }
            else
            {
                var ops = new List<object>();

                foreach (var item in oldItems)
                {
                    var oldIndex = before.IndexOf(item);
                    ops.Add(new
                    {
                        Action = NotifyCollectionChangeAction.Remove,
                        Item = item,
                        OldIndex = oldIndex
                    });
                }

                foreach (var item in newItems)
                {
                    var newIndex = after.IndexOf(item);
                    ops.Add(new
                    {
                        Action = NotifyCollectionChangeAction.Add,
                        Item = item,
                        NewIndex = newIndex
                    });
                }

                var replaces = new List<object>();

                var paired = Math.Min(oldItems.Count, newItems.Count);
                for (int i = 0; i < paired; i++)
                {
                    var oldItem = oldItems[i];
                    var newItem = newItems[i];

                    var oldIndex = before.IndexOf(oldItem);
                    var newIndex = after.IndexOf(newItem);

                    replaces.Add(new
                    {
                        Action = NotifyCollectionChangeAction.Replace,
                        OldItem = oldItem,
                        NewItem = newItem,
                        OldIndex = oldIndex,
                        NewIndex = newIndex
                    });
                }

                if (replaces.Count > 0)
                {
                    batchArgs = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Replace,
                        replaces,
                        NotifyCollectionChangeReason.Batch);
                }
                else
                {
                    batchArgs = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Replace,
                        ops,
                        NotifyCollectionChangeReason.Batch);
                }
            }

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
