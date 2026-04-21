using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class ObservablePreviewRangeCollection<T>
        : ObservableRangeCollection<T>
        , IRangeable
        where T : new()
    {
        public ObservablePreviewRangeCollection() { }

        /// <summary>
        /// PROMOTE the protected BC version for public INotifyPropertyChanging contract.
        /// </summary>
        public new CollectionChangingEventingPolicy CollectionChangingEventingPolicy
        {
            get => base.CollectionChangingEventingPolicy;
            set => base.CollectionChangingEventingPolicy = value;
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
                using (RequestStdModelAuthority(StdModelAuthority.SuspendForwardCollectionChanging))
                {
                    base.InsertItem(index, item);
                }
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
                using (RequestStdModelAuthority(StdModelAuthority.SuspendForwardCollectionChanging))
                {
                    base.SetItem(index, item);
                }
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
                using (RequestStdModelAuthority(StdModelAuthority.SuspendForwardCollectionChanging))
                {
                    base.RemoveItem(index);
                }
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
                using (RequestStdModelAuthority(StdModelAuthority.SuspendForwardCollectionChanging))
                {
                    base.MoveItem(oldIndex, newIndex);
                }
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
                using (RequestStdModelAuthority(StdModelAuthority.SuspendForwardCollectionChange))
                {
                    using (RequestStdModelAuthority(StdModelAuthority.SuspendForwardCollectionChanging))
                    {
                        base.ClearItems();
                    }
                }
            }
        }
        protected virtual void OnModelEpochFinalizing(ModelEpochDisposeEventArgs e)
        {
            OnCollectionChanging(e.Digest);
            base.OnModelEpochFinalizing(e);
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

        ModeledFullPathInfo? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;
    }
}
