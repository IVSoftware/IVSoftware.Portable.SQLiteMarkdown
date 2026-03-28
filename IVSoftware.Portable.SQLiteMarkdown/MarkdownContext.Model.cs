using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext
    {

        public virtual XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model =
                        new XElement(
                            nameof(StdMarkdownElement.model),
                            new XBoundAttribute(nameof(StdMarkdownAttribute.mdc), this, $"[MDC]"),
                            new XAttribute(nameof(StdMarkdownAttribute.autocount), 0));
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
        protected XElement? _model = null;

        protected Dictionary<XElement, XElement> _parentsOfRemoved = new();

        protected virtual void OnXAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            if (Enum.TryParse(xattr.Name.LocalName, out StdMarkdownAttribute std))
            {
                string id = null!;
                if (xattr.Parent is not null)
                {
                    if (xattr.Parent.Parent is null)
                    {
                        localRootProcess();
                    }
                    else
                    {
                        localDefaultProcess();
                    }
                }
                #region L o c a l F x
                void localRootProcess()
                {
                    switch (std)
                    {
                        case StdMarkdownAttribute.matches:
                            break;
                    }
                }
                void localDefaultProcess()
                {
                    if (xattr.Parent.Attribute(StdMarkdownAttribute.model) is XBoundAttribute xbaModel
                        && xbaModel.Tag is { } model
                        && !string.IsNullOrWhiteSpace(id = model.GetId()))
                    {
                        if (ReferenceEquals(xattr, xbaModel))
                        {
                            OnBoundItemObjectChange(xbaModel, e.ObjectChange);
                        }
                        else
                        {
                            switch (xattr)
                            {
                                case XBoundAttribute:
                                    break;
                                default:
                                    switch (std)
                                    {
                                        case StdMarkdownAttribute.match:
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
                #endregion L o c a l F x
            }
        }
        [Probationary]
        public HashSet<string> MatchContainsProto = new();
        protected virtual void OnXElementChanged(XElement xel, XElement pxel, XObjectChangeEventArgs e)
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
            }
            #endregion L o c a l F x
        }


    }
}
