using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    /// <summary>
    /// Suppressible collection with Preview semantics (but no Range semantics).
    /// </summary>
    internal partial class ObservablePreviewCollection<T>
        : ObservableCollection<T>
        , INotifyCollectionChanging
    {
        public ObservablePreviewCollection(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)        
        {
            EventScope = eventScope;
        }

        /// <summary>
        /// Defines the extent to which a preview handler may interact with a pending
        /// collection change proposal.
        /// </summary>
        /// <remarks>
        /// This enumeration constrains what a handler is permitted to do during the
        /// preview (Changing) phase. It does not describe the change itself, but rather
        /// the allowed level of participation in shaping or rejecting it.
        ///
        /// - ReadOnly   : Observe only. No modification or cancellation is permitted.
        /// - CancelOnly : The proposal may be rejected but not altered.
        /// - FullControl: The proposal may be rewritten or rejected entirely.
        ///
        /// These flags are enforced by the preview pipeline. Handlers opting into
        /// higher scopes assume responsibility for producing a valid and internally
        /// consistent change contract.
        /// </remarks>
        public NotifyCollectionChangeScope EventScope { get; }
        protected override void InsertItem(int index, T item)
        {
            var ePre = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Add,
                scope: EventScope,
                newItems: new[] { item },
                newStartingIndex: index);
            OnCollectionChanging(ePre);
            if (!ePre.Cancel)
            {
                base.InsertItem(index, item);
            }
        }
        protected override void SetItem(int index, T item)
        {
            var ePre = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Replace,
                scope: EventScope,
                newItems: new[] { item },
                oldItems: new[] { this[index] },
                newStartingIndex: index,
                oldStartingIndex: index);

            OnCollectionChanging(ePre);
            if (!ePre.Cancel)
            {
                base.SetItem(index, item);
            }
        }
        protected override void RemoveItem(int index)
        {
            var item = this[index];

            var ePre = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Remove,
                scope: EventScope,
                oldItems: new[] { item },
                oldStartingIndex: index);
            OnCollectionChanging(ePre);
            if (!ePre.Cancel)
            {
                base.RemoveItem(index);
            }
        }
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var item = this[oldIndex];

            var ePre = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Move,
                scope: EventScope,
                newItems: new[] { item },
                oldItems: new[] { item },
                newStartingIndex: newIndex,
                oldStartingIndex: oldIndex);
            OnCollectionChanging(ePre);
            if (!ePre.Cancel)
            {
                base.MoveItem(oldIndex, newIndex);
            }
        }

        protected override void ClearItems()
        {
            var snapshot = this.ToArray();

            var ePre = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Reset,
                scope: EventScope,
                oldItems: snapshot,
                oldStartingIndex: -1);
            OnCollectionChanging(ePre);
            if (!ePre.Cancel)
            {
                base.ClearItems();
            }
        }

        protected virtual void OnCollectionChanging(NotifyCollectionChangingEventArgs e)
        {
            CollectionChanging?.Invoke(this, e);
        }
        public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (DHostSuppressNotify.IsZero() && ! BatchDisposing)
            {
                base.OnCollectionChanged(e);
            }
        }
        protected virtual void ApplyChanges(NotifyCollectionChangingEventArgs e)
        {
            using (BeginApply())
            {
                this.Apply(e);
            }
        }

        public static implicit operator XElement(ObservablePreviewCollection<T> @this)
        {
            @this.ToString(out XElement model);
            model.SetAttributeValue(@this.ModelingCapabilityInfo.ModelingCapability);
            return model;
        }

        /// <summary>
        /// Determine the highest fidelity full path for T.
        /// </summary>
        public ModelingCapabilityInfo ModelingCapabilityInfo
        {
            get
            {                
                if (_modelingCapability is null)
                {
                    _modelingCapability = typeof(T).GetModelingCapability();
                }
                return (ModelingCapabilityInfo)_modelingCapability!;
            }
        }
        ModelingCapabilityInfo? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;

        #region D H O S T
        IDisposable BeginApply() => DHostApply.GetToken(this);
        DisposableHost DHostApply { get; } = new();
        public IDisposable BeginSuppressNotify() => DHostSuppressNotify.GetToken(this);
        public void CancelSuppressNotify() => DHostSuppressNotify.CancelSuppressNotify();

        public virtual string ToString(ReportFormat formatting)
        {
            switch (formatting)
            {
                case ReportFormat.Model:
                    return ((XElement)this).ToString();
                default:
                    throw new NotSupportedException($"{formatting.ToFullKey()} is not supported by this object.");
            }
        }

        public virtual string ToString(ModelPreviewDelegate preview)
        {
            var model = ((XElement)this);

            int index = 0;
            foreach (var xel in model.Descendants())
            {
                var item = this[index++];
                xel.SetStdAttributeValue(StdMarkdownAttribute.preview, preview(item));
            }
            return model.ToString();
        }

        DHostSuppress<T> DHostSuppressNotify
        {
            get
            {
                if (_dhostBatch is null)
                {
                    _dhostBatch = new DHostSuppress<T>();
                    _dhostBatch.FinalDispose += (sender, e) =>
                    {
                        if (e is CoalescingFinalDisposeEventArgs eFD)
                        {
                            try
                            {
                                BatchDisposing = true;
                                ApplyChanges(eFD.Coalesced);
                            }
                            finally
                            {
                                BatchDisposing = false;
                                OnCollectionChanged(eFD.Coalesced);
                            }
                        }
                    };
                }
                return _dhostBatch;
            }
        }

        public bool BatchDisposing { get; private set; }

        DHostSuppress<T>? _dhostBatch = null;
        #endregion D H O S T
    }

    internal partial class ObservablePreviewCollection<T> : IRangeable
    {
        public void AddRange(IEnumerable items)
        {
            using (BeginSuppressNotify())
            {
                int newStartingIndex = Count;
                foreach (var item in items)
                {
                    if(item is T itemT)
                    {
                        // [Careful]
                        // Use Insert.
                        // We can't use Add because the collection
                        // doesn't actually change until the end.
                        InsertItem(newStartingIndex++, itemT);
                    }
                    else
                    {
                        item.ThrowHard<InvalidCastException>($"All range items must be {typeof(T).Name}");
                        return;
                    }
                }
            }
        }

        public int AddRangeDistinct(IEnumerable items)
        {
            XElement model = this;

            if (ModelingCapabilityInfo.GetFullPath is not GetFullPathDlgt dlgt)
            {
                return 0;
            }
            else
            {
                int changed = 0;
                using (BeginSuppressNotify())
                {
                    int newStartingIndex = Count;
                    foreach (var item in items)
                    {
                        if (item is T itemT && dlgt(itemT) is string fullPath)
                        {
                            if (string.IsNullOrWhiteSpace(fullPath))
                            {
                                "ObservablePreviewCollection".ThrowHard<ArgumentException>($"The '{nameof(fullPath)}' argument cannot be empty.");
                                CancelSuppressNotify();
                                return 0;
                            }

                            switch (model.Place(fullPath))
                            {
                                case PlacerResult.Created:
                                    InsertItem(newStartingIndex++, itemT);
                                    changed++;
                                    break;
                                default:
                                    /* G T K - N O O P */
                                    // Skipping this item as non-distinct.
                                    break;
                            }
                        }
                        else
                        {
                            item.ThrowHard<InvalidCastException>($"All range items must be {typeof(T).Name}");
                            CancelSuppressNotify();
                            return 0;
                        }
                    }
                    return changed;
                }
            }
        }

        public void InsertRange(int startingIndex, IEnumerable items)
        {
            using (BeginSuppressNotify())
            {

            }
        }

        public int RemoveMultiple(IEnumerable items)
        {
            using (BeginSuppressNotify())
            {

            }
            return 0;
        }

        public void RemoveRange(int startingIndex, int endingIndex)
        {
            using (BeginSuppressNotify())
            {

            }
        }
    }
}
