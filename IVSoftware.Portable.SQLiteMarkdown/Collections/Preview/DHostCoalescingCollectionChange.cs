using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    internal sealed class DHostCoalescingCollectionChange : DisposableHost
    {
        object[] _listB4 = [];
        IList _listFTR = null!;

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            _cancel = false;
            if (e.AutoDisposableContext.Sender is SuppressionPhase phase)
            {
                Phase = phase;
            }
            else
            {
                this.ThrowFramework<InvalidOperationException>(
                    $"{nameof(Phase)} must be specified as token sender that is {nameof(SuppressionPhase)}.");
            }
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
        [Careful("Maintain authority while disposing")]
        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            var before = (IList)_listB4;
            var after = _listFTR;

            var digest = 
                _cancel
                ? new NotifyCollectionChangingEventArgs(
                    action: NotifyCollectionChangeAction.Reset,
                    reason: NotifyCollectionChangeReason.Batch | NotifyCollectionChangeReason.Cancel)
                : before.Diff(
                    after,
                    reason: NotifyCollectionChangeReason.Batch);

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
            IsDisposing = true;
            try
            {
                base.OnFinalDispose(eBatch);
            }
            finally
            {
                IsDisposing = false;
                Phase = SuppressionPhase.None;
            }
            _isModified = false;
            _cancel = false;
        }

        public void CancelSuppressNotify() => _cancel = true;
        private bool _cancel;

        [Canonical]
        public IDisposable GetToken(
            SuppressionPhase phase,
            IList list,
            Dictionary<string, object>? properties = null)
        {
            InitializeToken(list);
            return base.GetToken(sender: phase, properties: properties);
        }

        public IDisposable GetToken(
            SuppressionPhase phase,
            IList list,
            string key,
            object value)
        {
            InitializeToken(list);
            return base.GetToken(sender: phase, key, value);
        }

        // Hard block all ambiguous entry points
        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
            => throw new NotSupportedException($"Use {nameof(GetToken)}({nameof(SuppressionPhase)}, IList, ...)");

        public new IDisposable GetToken(object sender, string key, object value)
            => throw new NotSupportedException($"Use {nameof(GetToken)}({nameof(SuppressionPhase)}, IList, ...)");

        public new IDisposable GetToken(string key, object value)
            => throw new NotSupportedException();

        private void InitializeToken(IList list)
        {
            _listB4 = list.Cast<object>().ToArray();
            _listFTR = _listB4.ToList();
            _isModified = false;
        }

        /// <summary>
        /// Returns true if a batch is in progress.
        /// </summary>
        public bool TryAppend(NotifyCollectionChangedEventArgs e)
        {
            if(IsZero())
            {
                return false;
            }
            else
            {
                _listFTR.Apply(e);
                _isModified = true;
                return true;
            }
        }
        bool _isModified = false;

        public bool IsDisposing { get; private set; }
        public SuppressionPhase Phase { get; private set; }
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
