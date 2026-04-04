using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.StateRunner.Preview;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    public sealed class ModelDataExchangeAuthorityProvider<T> 
        : DisposableHost
    {
        public ModelDataExchangeAuthorityProvider(IList source)
        {
            _source = source;
        }
        IList _source;
        public ReadOnlyCollection<T> Snapshot { get; private set; } = null!;

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            _cancel = false;
            _isModified = false;
            if(e.AutoDisposableContext.Sender is ModelDataExchangeAuthority authority)
            {
                try
                {
                    Snapshot = new(_source.Cast<T>().ToArray());
                    Authority = authority;
                }
                catch (InvalidCastException ex)
                {
                    this.RethrowHard(ex);
                    // Only reachable if RethrowHard is handled.
                    Snapshot = new(Array.Empty<T>());
                    Authority = (ModelDataExchangeAuthority)FsmReserved.NoAuthority;
                    return;
                }
            }
            else
            {
                Authority = (ModelDataExchangeAuthority)FsmReserved.NoAuthority;
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
        protected override void OnFinalDispose(FinalDisposeEventArgs e)
        {
            try
            {
                IsDisposing = true;

                // If canceled, rollback all of the items to the original.
                if(_cancel)
                {
                    ((IList)_source).Clear();
                    foreach (var item in Snapshot)
                    {
                        _source.Add(item);
                    }
                }

                var before = Snapshot;
                var after = _source;

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

                snapshot["FinalList"] = _source;
                snapshot["IsModified"] = _isModified;

                var eBatch = new ModelDateExchangeFinalDisposeEventArgs(
                    e.ReleasedSenders,
                    snapshot,
                    digest,
                    _source);
                base.OnFinalDispose(eBatch);
            }
            finally
            {
                Authority = ModelDataExchangeAuthority.Collection;
                IsDisposing = false;
            }
            _isModified = false;
            _cancel = false;
        }

        public bool IsDisposing { get; private set; }

        public void CancelSuppressNotify() => _cancel = true;
        private bool _cancel;

        public new IDisposable GetToken(string key, object value)
            => throw new NotSupportedException("Sender is required.");
        bool _isModified = false;
        public ModelDataExchangeAuthority Authority { get; private set; } = ModelDataExchangeAuthority.Collection;
    }

    internal class ModelDateExchangeFinalDisposeEventArgs : FinalDisposeEventArgs
    {
        public ModelDateExchangeFinalDisposeEventArgs(
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
