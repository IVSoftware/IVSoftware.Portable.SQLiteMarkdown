using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Collections.Modeled;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using IVSoftware.Portable.Common.Collections.Internal;

namespace IVSoftware.Portable.Collections.Preview
{
    /// <summary>
    /// Suppressible collection with Preview semantics (but no Range semantics).
    /// </summary>
    internal partial class ObservablePreviewCollection<T>
        : ObservableModeledCollection<T>
        , INotifyCollectionChanging
    {
        public ObservablePreviewCollection(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)
        {
            EventScope = eventScope;
        }

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
            switch (DHostModelEpoch.Authority)
            {
                case ModelDataExchangeAuthority.Collection:
                case ModelDataExchangeAuthority.Model:
                    CollectionChanging?.Invoke(this, e);
                    break;
                case ModelDataExchangeAuthority.CollectionDeferred:
                case ModelDataExchangeAuthority.ModelDeferred:
                    switch (CollectionChangingEventingOption)
                    {
                        case CollectionChangingEventingOption.Discrete:
                            CollectionChanging?.Invoke(this, e);
                            break;
                        case CollectionChangingEventingOption.Deferred:
                            if (DHostModelEpoch.IsDisposing)
                            {
                                CollectionChanging?.Invoke(this, e);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {DHostModelEpoch.Authority.ToFullKey()} case is not supported.");
                    break;
            }
        }
        public event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging;

        public CollectionChangingEventingOption CollectionChangingEventingOption { get; set; }

        protected override void OnModelEpochFinalizing(ModelEpochDisposeEventArgs e)
        {
            OnCollectionChanging(e.Digest);
            base.OnModelEpochFinalizing(e);
        }

        public static implicit operator XElement(ObservablePreviewCollection<T> @this)
        {
            @this.ToString(out XElement model);
            model.SetAttributeValue(@this.ModelingCapabilityInfo.StdModelPath);
            return model;
        }

        /// <summary>
        /// Determine the highest fidelity full path for T.
        /// </summary>
        public ModeledFullPathInfo ModelingCapabilityInfo
        {
            get
            {                
                if (_modelingCapability is null)
                {
                    _modelingCapability = typeof(T).GetModeledPathInfo();
                }
                return _modelingCapability!;
            }
        }

        public NotifyCollectionChangeScope EventScope { get; }

        ModeledFullPathInfo? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;
    }
}
