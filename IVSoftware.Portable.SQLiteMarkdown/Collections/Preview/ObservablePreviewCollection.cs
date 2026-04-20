using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    /// <summary>
    /// Suppressible collection with Preview semantics (but no Range semantics).
    /// </summary>
    [Careful(
        "The CollectionChanging is protected; " +
        "This SAF object *does not* expose INotifyCollectionChanging")]
    internal partial class ObservablePreviewCollection<T> : ObservableModeledCollection<T>
    {
        public ObservablePreviewCollection() { }

		/// <summary>
		/// Promote the protected BC version for public INotifyPropertyChanging contract.
		/// </summary>
		public new event EventHandler<NotifyCollectionChangingEventArgs>? CollectionChanging
		{
			add => base.CollectionChanging += value;
			remove => base.CollectionChanging -= value;
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

        
        protected virtual void OnModelEpochFinalizing(ModelEpochDisposeEventArgs e)
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

        ModeledFullPathInfo? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;
    }
}
