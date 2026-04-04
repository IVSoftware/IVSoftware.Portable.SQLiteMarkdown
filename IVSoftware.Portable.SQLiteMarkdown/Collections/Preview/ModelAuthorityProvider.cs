using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.StateRunner.Preview;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    public sealed class ModelAuthorityProvider<T> : DisposableHost
    {
        public ReadOnlyCollection<T> Snapshot { get; private set; } = null!;
        IList _listFTR = null!;

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            _cancel = false;
            Phase = ModelAuthority.Preview;
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
            try
            {
                IsDisposing = true;

                // If canceled, rollback all of the items to the original.
                if(_cancel)
                {
                    _listFTR.Clear();
                    foreach (var item in Snapshot)
                    {
                        _listFTR.Add(item);
                    }
                }

                var before = Snapshot;
                var after = _listFTR;

                var digest =
                    _cancel
                    ? new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Reset,
                        reason: NotifyCollectionChangeReason.Digest | NotifyCollectionChangeReason.Cancel)
                    : before.Diff(
                        after,
                        reason: NotifyCollectionChangeReason.Digest);

                var snapshot = e.Keys.ToDictionary(
                    key => key,
                    key => e[key]);

                snapshot["FinalList"] = _listFTR;
                snapshot["IsModified"] = _isModified;

                var eBatch = new SuppressedFinalDisposeEventArgs(
                    e.ReleasedSenders,
                    snapshot,
                    digest,
                    _listFTR);
                Phase = ModelAuthority.Commit;
                base.OnFinalDispose(eBatch);
            }
            finally
            {
                Phase = ModelAuthority.None;
                IsDisposing = false;
            }
            _isModified = false;
            _cancel = false;
        }

        public bool IsDisposing { get; private set; }

        public void CancelSuppressNotify() => _cancel = true;
        private bool _cancel;

        [Canonical]
        public new IDisposable GetToken(object? sender = null, Dictionary<string, object>? properties = null)
        {
            if (sender is not IList list)
                throw new NotSupportedException($"Sender must implement IList.");

            InitializeToken(list);
            return base.GetToken(sender, properties);
        }

        public new IDisposable GetToken(object sender, string key, object value)
        {
            if (sender is not IList list)
                throw new NotSupportedException($"Sender must implement IList.");

            InitializeToken(list);
            return base.GetToken(sender, key, value);
        }

        public new IDisposable GetToken(string key, object value)
            => throw new NotSupportedException("Sender is required.");

        private void InitializeToken(IList list)
        {
            if (IsZero())
            {
                try
                {
                    Snapshot = new ReadOnlyCollection<T>(list.Cast<T>().ToArray());
                }
                catch (InvalidCastException)
                {
                    throw new NotSupportedException($"Sender {nameof(IList)} must contain all {typeof(T).Name}.");
                }
                _listFTR = list;
                _isModified = false;
            }
        }
        bool _isModified = false;
        public ModelAuthority Phase { get; private set; } = ModelAuthority.None;
    }

    internal class SuppressedFinalDisposeEventArgs : FinalDisposeEventArgs
    {
        public SuppressedFinalDisposeEventArgs(
            IReadOnlyCollection<object> releasedSenders,
            IReadOnlyDictionary<string, object> snapshot,
            NotifyCollectionChangingEventArgs batchEventArgs,
            IList finalList)
            : base(releasedSenders, snapshot)
        {
            Digest = batchEventArgs;
            FinalList = finalList;
        }

        public NotifyCollectionChangingEventArgs Digest { get; }
        public IList FinalList { get; }
    }
}
