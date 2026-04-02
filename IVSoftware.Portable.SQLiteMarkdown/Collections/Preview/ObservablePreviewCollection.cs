using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    }
}
