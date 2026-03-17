using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class ModeledMarkdownContext<T> : MarkdownContext<T>, IModeledMarkdownContext
    {
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
                                                        MatchContainsProto.Add(id);
                                                    }
                                                    break;
                                                case XObjectChange.Remove:
                                                    MatchContainsProto.Remove(id);
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
        public HashSet<string> MatchContainsProto = new();

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
        /// Provides a typed, read-only view of the predicate-match subset.
        /// </summary>
        /// <remarks>
        /// The underlying collection is created by the base context using
        /// the element type supplied at construction. This property simply
        /// re-exposes that collection as <see cref="IReadOnlyList{T}"/>.
        /// Structural changes performed by the infrastructure remain visible
        /// through this view.
        /// </remarks>
        public new IReadOnlyList<T> PredicateMatchSubset
            => (IReadOnlyList<T>)base.PredicateMatchSubset;

        public new IReadOnlyList<T> CanonicalSuperset
            => (IReadOnlyList<T>)base.CanonicalSuperset;

        protected new ObservableCollection<T> CanonicalSupersetProtected
            => (ObservableCollection<T>)base.CanonicalSupersetProtected;
    }
}
