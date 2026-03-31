using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
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
using System.Security.Cryptography;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    /// <summary>
    /// Listed in order of preference.
    /// </summary>
    internal enum ModelingCapability
    {
        /// <summary>
        /// Detected a string property named FullPath.
        /// </summary>
        FullPath,

        /// <summary>
        /// A [PrimaryKey] property or a string property named Id.
        /// </summary>
        Id,

        /// <summary>
        /// A [PrimaryKey] property or a string property named Description.
        /// </summary>
        Description,

        /// <summary>
        /// A [PrimaryKey] property or a string property named Text.
        /// </summary>
        Text,

        /// <summary>
        /// Failed to find a suitable modeling property.
        /// </summary>
        Unavailable,
    }
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
            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Add,
                scope: EventScope,
                newItems: new[] { item },
                newStartingIndex: index);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);

                if (e.Cancel) return;

                if (!DHostBatch.TryAppend(e))
                {
                    if (e.IsModified)
                    {
                        ApplyChanges(e);
                    }
                    else
                    {
                        base.InsertItem(index, item);
                    }
                }
            }
            else
            {
                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Replace,
                scope: EventScope,
                newItems: new[] { item },
                oldItems: new[] { oldItem },
                newStartingIndex: index,
                oldStartingIndex: index);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);

                if (e.Cancel) return;

                if (!DHostBatch.TryAppend(e))
                {
                    if (e.IsModified)
                    {
                        ApplyChanges(e);
                    }
                    else
                    {
                        base.SetItem(index, item);
                    }
                }
            }
            else
            {
                base.SetItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Remove,
                scope: EventScope,
                oldItems: new[] { item },
                oldStartingIndex: index);

            if(DHostApply.IsZero())
            {
                OnCollectionChanging(e);
                if (!e.Cancel)
                {
                    if (!DHostBatch.TryAppend(e))
                    {
                        if (e.IsModified)
                        {
                            ApplyChanges(e);
                        }
                        else
                        {
                            base.RemoveItem(index);
                        }
                    }
                }
            }
            else
            {
                base.RemoveItem(index);
            }
        }
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var item = this[oldIndex];

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Move,
                scope: EventScope,
                newItems: new[] { item },
                oldItems: new[] { item },
                newStartingIndex: newIndex,
                oldStartingIndex: oldIndex);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);

                if (e.Cancel) return;

                if (!DHostBatch.TryAppend(e))
                {
                    if (e.IsModified)
                    {
                        ApplyChanges(e);
                    }
                    else
                    {
                        base.MoveItem(oldIndex, newIndex);
                    }
                }
            }
            else
            {
                base.MoveItem(oldIndex, newIndex);
            }
        }

        protected override void ClearItems()
        {
            var snapshot = this.ToArray();

            var e = new NotifyCollectionChangingEventArgs(
                action: NotifyCollectionChangeAction.Reset,
                scope: EventScope,
                oldItems: snapshot,
                oldStartingIndex: -1);

            if (DHostApply.IsZero())
            {
                OnCollectionChanging(e);

                if (e.Cancel) return;

                if (!DHostBatch.TryAppend(e))
                {
                    if (e.IsModified)
                    {
                        ApplyChanges(e);
                    }
                    else
                    {
                        base.ClearItems();
                    }
                }
            }
            else
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
            if (DHostBatch.IsZero() && ! BatchDisposing)
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
            XElement model = new XElement(nameof(StdMarkdownElement.model));
            model.SetAttributeValue(@this.ModelingCapability);
            int itemCount = 0;
            if (@this.ModelingCapability != ModelingCapability.Unavailable)
            {
                foreach (var item in @this)
                {
                    if (@this.GetFullPath?.Invoke(item) is { } fullPath
                        && !string.IsNullOrWhiteSpace(fullPath))
                    {
                        var placerResult = model.Place(fullPath, out var xel);
                        switch (placerResult)
                        {
                            case PlacerResult.Exists:
                                break;
                            case PlacerResult.Created:
                                xel.Name = nameof(StdMarkdownElement.xitem);
                                xel.SetBoundAttributeValue(
                                    tag: item,
                                    name: nameof(StdMarkdownAttribute.model));

                                xel.SetAttributeValue(nameof(StdMarkdownAttribute.order), itemCount++);
                                break;
                            default:
                                "ObservablePreviewCollection".ThrowFramework<NotSupportedException>(
                                    $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                break;
                        }
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// Determine the highest fidelity full path for T.
        /// </summary>
        public ModelingCapability ModelingCapability
        {
            get
            {                
                if (_modelingCapability is null)
                {
                    var type = typeof(T);
                    foreach (ModelingCapability capability in Enum.GetValues(typeof(ModelingCapability)))
                    {
                        _modelingCapability = capability;
                        switch (capability)
                        {
                            case ModelingCapability.Id:
                                _fullPathPI = type.GetSQLiteMapping()?.PK?.PropertyInfo;
                                if (_fullPathPI is null)
                                {
                                    _fullPathPI = type.GetProperty(capability.ToString());
                                }
                                if (_fullPathPI is null) // Still...
                                {
                                    break;
                                }
                                else
                                {
                                    goto breakFromInner;
                                }
                            case ModelingCapability.FullPath:
                            case ModelingCapability.Description:
                            case ModelingCapability.Text:
                            case ModelingCapability.Unavailable:
                                _fullPathPI = type.GetProperty(capability.ToString());
                                if (_fullPathPI is null)
                                {
                                    break;
                                }
                                else
                                {
                                    goto breakFromInner;
                                }
                            default:
                                this.ThrowHard<NotSupportedException>($"The {capability.ToFullKey()} case is not supported.");
                                _modelingCapability = ModelingCapability.Unavailable;
                                // If handled, allow loop to continue;
                                break;
                        }
                    }
                }
                breakFromInner:
                return (ModelingCapability)_modelingCapability!;
            }
        }
        ModelingCapability? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;

        public Func<T, string>? GetFullPath
        {
            get
            {
                if (ModelingCapability == ModelingCapability.Unavailable)
                {
                    return null;
                }
                else
                {
                    if (_getFullPath is null)
                    {
                        var instance = Expression.Parameter(typeof(T), "item");
                        var property = Expression.Property(instance, _fullPathPI);

                        Expression body =
                            property.Type == typeof(string)
                            ? property
                            : Expression.Call(property, nameof(object.ToString), Type.EmptyTypes);

#if DEBUG
                        Debug.WriteLine($"260331.A {Expression.Lambda<Func<T, string>>(body, instance)}");
                        { }
#endif

                        _getFullPath =
                            Expression.Lambda<Func<T, string>>(body, instance)
                            .Compile();
                    }
                    return _getFullPath;
                }
            }
        }
        Func<T, string>? _getFullPath;


        #region D H O S T
        IDisposable BeginApply() => DHostApply.GetToken(this);
        DisposableHost DHostApply { get; } = new();
        public IDisposable BeginBatch() => DHostBatch.GetToken(this);

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

        DHostBatchCollectionChange DHostBatch
        {
            get
            {
                if (_dhostBatch is null)
                {
                    _dhostBatch = new DHostBatchCollectionChange();
                    _dhostBatch.FinalDispose += (sender, e) =>
                    {
                        if (e is BatchFinalDisposeEventArgs eFD)
                        {
                            try
                            {
                                BatchDisposing = true;
                                ApplyChanges(eFD.Digest);
                            }
                            finally
                            {
                                BatchDisposing = false;
                                OnCollectionChanged(eFD.Digest);
                            }
                        }
                    };
                }
                return _dhostBatch;
            }
        }

        public bool BatchDisposing { get; private set; }

        DHostBatchCollectionChange? _dhostBatch = null;
        #endregion D H O S T
    }

    internal partial class ObservablePreviewCollection<T> : IRangeable
    {
        public void AddRange(IEnumerable items)
        {
            using (BeginBatch())
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
            using (BeginBatch())
            {

            }
            return 0;
        }

        public void InsertRange(int startingIndex, IEnumerable items)
        {
            using (BeginBatch())
            {

            }
        }

        public int RemoveMultiple(IEnumerable items)
        {
            using (BeginBatch())
            {

            }
            return 0;
        }

        public void RemoveRange(int startingIndex, int endingIndex)
        {
            using (BeginBatch())
            {

            }
        }
    }
}
