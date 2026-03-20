using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    partial class Topology<T> : IList
    {
        #region P O L I C Y    A R B I T R A T I O N
        public int Count => Read.Count;

        [JsonIgnore]
        public bool IsFixedSize =>
            ((IList?)ObservableNetProjection)?.IsFixedSize
            ?? ((IList)CanonicalSupersetInternal).IsFixedSize;

        [JsonIgnore]
        public bool IsSynchronized =>
            ((IList?)ObservableNetProjection)?.IsSynchronized
            ?? ((IList)CanonicalSupersetInternal).IsSynchronized;

        [JsonIgnore]
        public object SyncRoot =>
            ((IList?)ObservableNetProjection)?.SyncRoot
            ?? ((IList)CanonicalSupersetInternal).SyncRoot;

        [JsonIgnore]
        public bool IsReadOnly => ((ICollection<T>)CanonicalSupersetInternal).IsReadOnly;
        #endregion P O L I C Y    A R B I T R A T I O N

        [Indexer]
        object? IList.this[int index] 
        { 
            get => Read[index];
            set => ((IList)CanonicalSupersetInternal)[index] = value;
        }
        public int Add(object value)
        {
            return ((IList)CanonicalSupersetInternal).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList)CanonicalSupersetInternal).Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)CanonicalSupersetInternal).CopyTo(array, index);
        }

        public int IndexOf(object value)
        {
            return ((IList)CanonicalSupersetInternal).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)CanonicalSupersetInternal).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)CanonicalSupersetInternal).Remove(value);
        }

        IEnumerator IEnumerable.GetEnumerator() => Read.GetEnumerator();

        void IList.Clear()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Read routed, Write with authority.
    /// </summary>
    partial class Topology<T> : IList<T>
    {
        [Indexer]
        public T this[int index]
        { 
            get => ((IList<T>)CanonicalSupersetInternal)[index];
            set => ((IList<T>)CanonicalSupersetInternal)[index] = value;
        }

        public void Add(T item)
        {
            ((ICollection<T>)CanonicalSupersetInternal).Add(item);
        }

        /// <summary>
        /// "No surprises Clear in the IList explicit interface.
        /// </summary>
        [Careful("Explicit interface implementation only. Do not hide the base class default.")]
        void ICollection<T>.Clear() => base.Clear(all: true);

        public bool Contains(T item)
        {
            return ((ICollection<T>)CanonicalSupersetInternal).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)CanonicalSupersetInternal).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)CanonicalSupersetInternal).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)CanonicalSupersetInternal).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)CanonicalSupersetInternal).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)CanonicalSupersetInternal).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)CanonicalSupersetInternal).RemoveAt(index);
        }
    }

    partial class Topology<T> // IModeledMarkdownContext
    {
        public ProjectionTopology ProjectionTopology
        {
            get
            {
                if (ObservableNetProjection is null)
                {
                    return ProjectionTopology.None;
                }
                else
                {
                    var type = GetType();
                    if (type == typeof(Topology<T>))
                    {   /* G T K - N O O P */
                        // Leave this at None.
                    }
                    else
                    {
                        if (typeof(INotifyCollectionChanged).IsAssignableFrom(type))
                        {
                            _projectionTopology = ProjectionTopology.Inheritance;
                        }
                        else
                        {
                            _projectionTopology = ProjectionTopology.Composition;
                        }
                    }
                    return _projectionTopology;
                }
            }
            //internal set
            //{
            //    if (!Equals(_projectionTopology, value))
            //    {
            //        _projectionTopology = value;
            //        OnPropertyChanged();
            //    }
            //}
        }
        ProjectionTopology _projectionTopology = ProjectionTopology.None;


#if false
        /// <summary>
        /// Reports on whether this object is inherited or composed.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ProjectionTopology ProjectionTopology
        {
            get
            {
                if (_projectionTopology == ProjectionTopology.None)
                {
                    var type = GetType();
                    if(type != typeof(Topology<T>))
                    {
                        if (typeof(INotifyCollectionChanged).IsAssignableFrom(type)
                            && type != typeof(Topology<T>))
                        {
                            // _projectionTopology = ProjectionTopology.Inheritance;
                        }
                        else
                        {
                            // _projectionTopology = ProjectionTopology.Composition;
                        }
                    }
                    else
                    {   /* G T K - N O O P */
                        // Leave at None.
                    }
                }
                return _projectionTopology;
            }
        }
        ProjectionTopology _projectionTopology = ProjectionTopology.None;
#endif


        public ReplaceItemsEventingOption ReplaceItemsEventingOptions
        { 
            get;
            set;
        } = ReplaceItemsEventingOption.StructuralReplaceEvent;

        public virtual void LoadCanon(IEnumerable? recordset) 
        {
            using(BeginCollectionChangeAuthority(CollectionChangeAuthority.Projection))
            {
                // This calls Model.Apply(e) in the Changing handler.
                CanonicalSupersetInternal.Clear();
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    Debug.Assert(FilterQueryDatabase.Table<T>().Count() == 0, "Must be EMPTY.");
                }

                if (recordset is not null)
                {
                    foreach (T item in recordset)
                    {
                        CanonicalSupersetInternal.Add(item);
                    }
                    switch (CanonicalCount)
                    {
                        case 0:
                            SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                            FilteringState = FilteringState.Ineligible;
                            break;
                        case 1:
                            SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                            FilteringState = FilteringState.Ineligible;
                            break;
                        default:
                            SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                            switch (QueryFilterConfig)
                            {
                                case QueryFilterConfig.Query:
                                case QueryFilterConfig.Filter:
                                default:
                                    FilteringState = FilteringState.Ineligible;
                                    break;
                                case QueryFilterConfig.QueryAndFilter:
                                    FilteringState = FilteringState.Armed;
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the singleton, non-replaceable root XElement, created on demand.
        /// </summary>
        /// <remarks>
        /// This represents the canonical ledger.
        /// </remarks>
        public override XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new XElement(nameof(StdMarkdownElement.model));
                    _model.Changing += (sender, e) =>
                    {
                        if (sender is XElement xel && e.ObjectChange == XObjectChange.Remove)
                        {
                            _parentsOfRemoved[xel] = xel.Parent;
                        }
                    };
                    _model.Changed += (sender, e) =>
                    {
                        switch (sender)
                        {
                            case XElement xel:
                                XElement pxel;
                                if (e.ObjectChange == XObjectChange.Remove)
                                {
                                    if (!_parentsOfRemoved.TryGetValue(xel, out pxel))
                                    {
                                        _parentsOfRemoved.ThrowSoft<NullReferenceException>(
                                            $"Expecting parent for removed XElement was cached prior." +
                                            $"Unless this throw is escalated, flow will continue with null parent.");
                                    }
                                    _parentsOfRemoved.Remove(xel);
                                }
                                else
                                {
                                    pxel = xel.Parent;
                                }
                                OnXElementChanged(xel, pxel, e);
                                break;
                            case XAttribute xattr:
                                OnXAttributeChanged(xattr, e);
                                break;
                        }
                    };
                }
                return _model;
            }
        }
        XElement? _model = null;

        Dictionary<XElement, XElement> _parentsOfRemoved = new();

        protected virtual void OnXAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            if (DHostAuthorityEpoch.Authority == CollectionChangeAuthority.None)
            {   /* G T K - N O O P */
            }
            else
            {
                string id = null!;
                if (xattr.Parent?.Attribute(StdMarkdownAttribute.model) is XBoundAttribute xbaModel
                    && xbaModel.Tag is { } model
                    && !string.IsNullOrWhiteSpace(id = model.GetId()))
                {
                    if (ReferenceEquals(xattr, xbaModel))
                    {
                        OnBoundItemObjectChange(xbaModel, e.ObjectChange);
                    }
                    else
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, out StdMarkdownAttribute std))
                        {
                            switch (xattr)
                            {
                                case XBoundAttribute:
                                    break;
                                default:
                                    switch (std)
                                    {
                                        case StdMarkdownAttribute.ismatch:
                                            bool isMatch = bool.Parse(xattr.Value);
                                            switch (e.ObjectChange)
                                            {
                                                case XObjectChange.Add:
                                                case XObjectChange.Value:
                                                    if (isMatch)
                                                    {
                                                        MatchContains.Add(id);
                                                    }
                                                    break;
                                                case XObjectChange.Remove:
                                                    MatchContains.Remove(id);
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [Probationary]
        protected HashSet<string> MatchContains = new();

        protected virtual void OnXElementChanged(XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
            if (DHostAuthorityEpoch.Authority == CollectionChangeAuthority.None)
            {   /* G T K - N O O P */
            }
            else
            {
                if (pxel is null)
                {
                    this.ThrowFramework<NullReferenceException>(
                        $"UNEXPECTED: The '{nameof(pxel)}' argument should be non-null by design.");
                }
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                    case XObjectChange.Remove:
                        var xbo =
                            xel
                            .Attributes()
                            .OfType<XBoundAttribute>()
                            .FirstOrDefault(_ => _.Tag?.GetType() == ContractType);
                        if (xbo is not null)
                        {
                            OnBoundItemObjectChange(xbo, e.ObjectChange);
                        }
                        localAutoCount();
                        break;
                }

                #region L o c a l F x
                void localAutoCount()
                {
                    XElement? modelRoot = pxel?.AncestorsAndSelf().LastOrDefault();
                    if (modelRoot is null)
                    {
                        this.ThrowFramework<NullReferenceException>(
                            $"UNEXPECTED: The '{nameof(modelRoot)}' argument should be non-null by design.");
                    }
                    else
                    {
                        var autocount = modelRoot.GetAttributeValue<int>(StdMarkdownAttribute.autocount);
                        switch (e.ObjectChange)
                        {
                            case XObjectChange.Add:
                                autocount++;
                                break;
                            case XObjectChange.Remove:
                                if (autocount == 0)
                                {
                                    this.ThrowFramework<InvalidOperationException>(
                                        $"UNEXPECTED: Illegal underflow detected '{nameof(autocount)}'. Count should be >= 0 by design.");
                                }
                                else
                                {
                                    autocount--;
                                }
                                break;
                        }
                        modelRoot.SetAttributeValue(StdMarkdownAttribute.autocount, autocount);
                        // [Careful]
                        // It's too racey here to try and compare counts.
                    }
#if false
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                        count++;
                        root?.SetAttributeValue(StdMarkdownAttribute.count, count);
                        break;
                    case XObjectChange.Remove:
                        if (count == 0)
                        {
                            this.ThrowFramework<InvalidOperationException>(
                                $"UNEXPECTED: Illegal underflow detected '{nameof(count)}'. Count should be >= 0 by design.");
                        }
                        else
                        {
                            count--;
                            {
                                root?.SetAttributeValue(StdMarkdownAttribute.count, count);
                            }
                        }
                        break;
                }
#endif
                }
                #endregion L o c a l F x
            }
        }

#if DEBUG
        const bool SQLITE_STRICT = true;
#else
        const bool SQLITE_STRICT = false;
#endif
        protected virtual void OnBoundItemObjectChange(XBoundAttribute xbo, XObjectChange action)
        {
            var item = xbo.Tag;
            switch (action)
            {
                case XObjectChange.Add:
                    localSetModelAuthority();
                    localAddEvents();
                    _ = localTryAddToDatabase();
                    break;
                case XObjectChange.Remove:
                    _ = localTryRemoveFromDatabase();
                    localRemoveEvents();
                    break;
            }
            #region L o c a l F x

            // Associate the xml Model governing this ddx.
            void localSetModelAuthority()
            {
                if (xbo.Tag is IAffinityModel modeled)
                {
                    if (xbo.Parent is null)
                    {
                        this.ThrowFramework<NullReferenceException>(
                            "UNEXPECTED: An attribute that is added should have a parent. What was it added *to*?");
                    }
                    else
                    {
                        modeled.Model = xbo.Parent;
                    }
                }
            }

            void localAddEvents()
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged += OnItemPropertyChanged;
                }
            }
            void localRemoveEvents()
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged -= OnItemPropertyChanged;
                }
            }
            bool? localTryAddToDatabase()
            {
                bool? isSuccess = null;
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    if (SQLITE_STRICT)
                    {
                        isSuccess = 1 == FilterQueryDatabase.Insert(item);
                    }
                    else
                    {
                        isSuccess = 1 == FilterQueryDatabase.InsertOrReplace(item);
                    }
                }
                else
                {   /* G T K - N O O P */
                    // There is no filter database to maintain.
                    isSuccess = null;
                }
                if (isSuccess == false)
                {
                    this.ThrowPolicyException(SQLiteMarkdownPolicyViolation.SQLiteOperationFailed);
                }
                return isSuccess;
            }

            bool? localTryRemoveFromDatabase()
            {
                bool? isSuccess = null;
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    isSuccess = 1 == FilterQueryDatabase.Delete(item);
                }
                else
                {
                    isSuccess = null;
                }
                return isSuccess;
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// Determines whether MDC is allowed to puppeteer the projection directly.
        /// </summary>
        public NetProjectionOption ProjectionOption
        {
            get =>
                ProjectionTopology == ProjectionTopology.Inheritance
                ? NetProjectionOption.Inherited
                // Guards against attempting to write when the projection is null.
                : ObservableNetProjection is null
                    ? NetProjectionOption.ObservableOnly
                    : _projectionOption;
            set
            {
                if (!Equals(_projectionOption, value))
                {
                    _projectionOption = value;
                    OnPropertyChanged();
                }
            }
        }
        NetProjectionOption _projectionOption = 0;

    }
}
