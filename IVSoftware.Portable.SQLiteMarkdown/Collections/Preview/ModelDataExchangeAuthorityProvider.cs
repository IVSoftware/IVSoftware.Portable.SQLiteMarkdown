using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.StateRunner.Preview;
using IVSoftware.Portable.Xml.Linq.Collections;
using IVSoftware.Portable.Xml.Linq.Collections.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    public sealed class ModelDataExchangeAuthorityProvider<T> 
        : DisposableHost
    {
        /// <summary>
        /// Raises a coalesced change event for subsequent changes to the source during the epoch.
        /// </summary>
        /// <remarks>
        /// The initial source must be passed in. There's no way to know if this is:
        /// - Before and after Range suppression (could me a non-modeled collection)
        /// - Before and after Filter change (source is routed to PMSS)
        /// </remarks>
        [Canonical]
        public IDisposable GetToken(ModelDataExchangeAuthority authority, IList source)
        {
            try
            {
                _source = source;            // Catch the IList constraint here.
                _incc = source as INotifyCollectionChanged;
                Snapshot = new(source.Cast<T>().ToArray());
            }
            catch (InvalidCastException ex)
            {
                this.RethrowHard(ex);
                // Only reachable if RethrowHard is handled.
                Snapshot = new(Array.Empty<T>());
                authority = (ModelDataExchangeAuthority)FsmReserved.NoAuthority;
            }
            return base.GetToken(sender: authority);
        }

        public ReadOnlyCollection<T> Snapshot { get; private set; } = null!;

        IList? _source = null;
        public INotifyCollectionChanged? INCC
        {
            get => _incc;
            set
            {
                if(value is null)
                {
                    Debug.Fail($@"ADVISORY - First Time.");
                }
                if (!Equals(_incc, value))
                {
                    _incc?.CollectionChanged -= OnCollectionChanged;
                    _incc = value;
                    _incc?.CollectionChanged += OnCollectionChanged;
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _isModified = true;
        }

        INotifyCollectionChanged? _incc = null;

        protected override void OnBeginUsing(BeginUsingEventArgs e)
        {
            _cancel = false;
            _isModified = false;
            if(e.AutoDisposableContext.Sender is ModelDataExchangeAuthority authority)
            {
                if(_source is null)
                {
                    this.ThrowHard<InvalidOperationException>(
                        $"{nameof(GetToken)} must be called with {nameof(ModelDataExchangeAuthority)} and {nameof(IList)} arguments.");
                    // Only reachable if RethrowHard is handled.
                    Snapshot = new(Array.Empty<T>());
                    Authority = (ModelDataExchangeAuthority)FsmReserved.NoAuthority;
                    return;
                }
                Authority = authority;
            }
            else
            {
                this.ThrowHard<ArgumentException>($"Requires a token whose sender is {nameof(ModelDataExchangeAuthority)}");
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
                if (_source is null)
                {
                    // Already warned.
                    base.OnFinalDispose(e);
                }
                else
                {
                    // If canceled, rollback all of the items to the original.
                    if (_cancel)
                    {
                        _source.Clear();
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

                    var eFinalDisposeCollectionChangeWrapper = new ModelDataExchangeFinalDisposeEventArgs(
                        e.ReleasedSenders,
                        snapshot,
                        digest,
                        _source);

                    snapshot["FinalList"] = _source;

                    base.OnFinalDispose(eFinalDisposeCollectionChangeWrapper);
                }
            }
            finally
            {
                Authority = ModelDataExchangeAuthority.Collection;
                IsDisposing = false;
            }
            _cancel = false;
            _source = null;
        }

        public bool IsDisposing { get; private set; }

        public void CancelSuppressNotify() => _cancel = true;
        private bool _cancel;

        public new IDisposable GetToken(string key, object value)
            => throw new NotSupportedException("Sender is required.");
        bool _isModified = false;
        public ModelDataExchangeAuthority Authority { get; private set; } = ModelDataExchangeAuthority.Collection;
    }

    public class ModelDataExchangeFinalDisposeEventArgs : FinalDisposeEventArgs
    {
        public ModelDataExchangeFinalDisposeEventArgs(
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
