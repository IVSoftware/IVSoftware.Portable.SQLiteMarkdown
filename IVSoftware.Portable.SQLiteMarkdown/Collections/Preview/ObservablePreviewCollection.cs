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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    internal partial class ObservablePreviewCollection<T>
        : SuppressibleObservableCollection<T>
        , INotifyCollectionChanging
    {
        public ObservablePreviewCollection(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)
            : base(eventScope) { }

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

        ModelPreviewDelegate PreviewDlgt
        {
            get
            {
                if (_PreviewDlgt is null)
                {
                    _PreviewDlgt = this.GetModelPreviewDlgt<T>();
                }
                return _PreviewDlgt;
            }
        }
        ModelPreviewDelegate? _PreviewDlgt = null;

        public static implicit operator XElement(ObservablePreviewCollection<T> @this)
        {
            @this.ToString(@this.PreviewDlgt, out XElement model);
            model.SetAttributeValue(@this.ModelingCapability);
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

        public GetFullPathDelegate<T>? GetFullPathDlgt
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
                        Debug.WriteLine($"260331.A {Expression.Lambda<GetFullPathDelegate<T>>(body, instance)}");
                        { }
#endif

                        _getFullPath =
                            Expression.Lambda<GetFullPathDelegate<T>>(body, instance)
                            .Compile();
                    }
                    return _getFullPath;
                }
            }
        }
        GetFullPathDelegate<T>? _getFullPath;

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
            using (BeginSuppressNotify())
            {
                int newStartingIndex = Count;
                foreach (var item in items)
                {
                    if (item is T itemT && GetFullPathDlgt?.Invoke(itemT) is string fullPath)
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
            }
            return 0;
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
