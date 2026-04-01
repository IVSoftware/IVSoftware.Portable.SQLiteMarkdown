using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    internal sealed class DHostSuppress<T> : DisposableHost
    {
        public ReadOnlyCollection<T> Snapshot { get; private set; } = null!;
        INotifyCollectionChangedSuppress<T> _listFTR = null!;

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            _cancel = false;
            Phase = SuppressionPhase.Preview;
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
        /// - Single Replace only, or
        /// - IList consisting or multiple, single, indexed Replace events.
        /// The probably response when a consumer inspects NewItems and sees multiple replace events is a reset + add.
        /// </remarks>
        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            var before = Snapshot;
            var after = _listFTR;

            var digest = 
                _cancel
                ? new NotifyCollectionChangingEventArgs(
                    action: NotifyCollectionChangeAction.Reset,
                    reason: NotifyCollectionChangeReason.Coalesce | NotifyCollectionChangeReason.Cancel)
                : before.Diff(
                    after,
                    reason: NotifyCollectionChangeReason.Coalesce);

            var snapshot = e.Keys.ToDictionary(
                key => key,
                key => e[key]);

            snapshot["FinalList"] = _listFTR;
            snapshot["IsModified"] = _isModified;

            var eBatch = new CoalescingFinalDisposeEventArgs(
                e.ReleasedSenders,
                snapshot,
                digest,
                _listFTR);
            try
            {
                Phase = SuppressionPhase.Commit;
                base.OnFinalDispose(eBatch);
            }
            finally
            {
                Phase = SuppressionPhase.None;
            }
            _isModified = false;
            _cancel = false;
        }

        public void CancelSuppressNotify() => _cancel = true;
        private bool _cancel;

        [Canonical]
        public IDisposable GetToken(
            INotifyCollectionChangedSuppress<T> list,
            Dictionary<string, object>? properties = null)
        {
            InitializeToken(list);
            return base.GetToken(sender: list, properties: properties);
        }

        public IDisposable GetToken(
            INotifyCollectionChangedSuppress<T> list,
            string key,
            object value)
        {
            InitializeToken(list);
            return base.GetToken(sender: list, key, value);
        }

        // Hard block all ambiguous entry points
        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
            => throw new NotSupportedException($"Use {nameof(GetToken)}({nameof(SuppressionPhase)}, IList, ...)");

        public new IDisposable GetToken(object sender, string key, object value)
            => throw new NotSupportedException($"Use {nameof(GetToken)}({nameof(SuppressionPhase)}, IList, ...)");

        public new IDisposable GetToken(string key, object value)
            => throw new NotSupportedException();

        private void InitializeToken(INotifyCollectionChangedSuppress<T> list)
        {
            Snapshot = new ReadOnlyCollection<T>(list.ToArray<T>());
            _listFTR = list;
            _isModified = false;
        }
        bool _isModified = false;
        public SuppressionPhase Phase { get; private set; } = SuppressionPhase.None;
    }

    internal class CoalescingFinalDisposeEventArgs : FinalDisposeEventArgs
    {
        public CoalescingFinalDisposeEventArgs(
            IReadOnlyCollection<object> releasedSenders,
            IReadOnlyDictionary<string, object> snapshot,
            NotifyCollectionChangingEventArgs batchEventArgs,
            IList finalList)
            : base(releasedSenders, snapshot)
        {
            Coalesced = batchEventArgs;
            FinalList = finalList;
        }

        public NotifyCollectionChangingEventArgs Coalesced { get; }
        public IList FinalList { get; }
    }
}
