using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext
    {
#if true
        Dictionary<XObject, XElement> _parentsOfRemoved = new();
        Dictionary<XAttribute, bool?> _oldValues = new();
        EnumHistogrammer<StdMarkdownAttribute> _histo = new(ZeroCountOption.Remove);
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
                        if (sender is XObject xob)
                        {
                            switch (e.ObjectChange)
                            {
                                case XObjectChange.Remove:
                                    _parentsOfRemoved[xob] = xob.Parent ?? throw new NullReferenceException();
                                    break;
                                case XObjectChange.Value when xob is XAttribute xattr:
                                    _oldValues[xattr] = bool.TryParse(xattr.Value, out var valid) ? valid : null;
                                    break;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    };

                    _model.Changed += (sender, e) =>
                    {
                        if (sender is XObject xob)
                        {
                            XElement? pxel = xob.Parent;
                            switch (e.ObjectChange)
                            {
                                case XObjectChange.Remove:
                                    pxel = _parentsOfRemoved[xob];
                                    _parentsOfRemoved.Remove(xob);
                                    break;
                                case XObjectChange.Value when xob is XAttribute xattr:
                                    var oldValue = _oldValues.TryGetValue(xattr, out var validOld) ? validOld : null;
                                    bool? newValue = bool.TryParse(xattr.Value, out var validNew) ? validNew : null;
                                    _oldValues.Remove(xattr);
                                    if (newValue is null ^ oldValue is null)
                                    {
                                        this.ThrowPolicyException(MarkdownContextPolicyViolation.XAttributeBooleanToggle);
                                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std))
                                        {
                                            if (oldValue == true)
                                            {
                                                _histo -= std;
                                            }
                                            else if (newValue == true)
                                            {
                                                _histo += std;
                                            }
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        if (newValue == oldValue)
                                        {
                                            return;
                                        }
                                        else
                                        {   /* G T K */
                                            // Toggle detected.
                                        }
                                    }
                                    break;
                            }
                            switch (sender)
                            {
                                case XElement xel:
                                    OnXElementChanged(xel, pxel ?? throw new NullReferenceException(), e);
                                    break;
                                case XAttribute xattr:
                                    OnXAttributeChanged(xattr, pxel ?? throw new NullReferenceException(), e);
                                    break;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    };
                }
                return _model;
            }
        }
        protected XElement? _model = null;
        protected virtual void OnXElementChanged(XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
            switch (e.ObjectChange)
            {
                case XObjectChange.Add:
                case XObjectChange.Remove:
                    foreach (var attr in xel.Attributes())
                    {
                        OnXAttributeChanged(attr, pxel, e);
                    }
                    break;
            }
        }

        protected virtual void OnXAttributeChanged(XAttribute xattr, XElement pxel, XObjectChangeEventArgs e)
        {
            if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std))
            {
                bool? newValue = bool.TryParse(xattr.Value, out var valid) ? valid : null;
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                        if (newValue != false)
                        {
                            _histo += std;
                        }
                        localUpdateAutocount();
                        break;
                    case XObjectChange.Remove:
                        if (newValue != false)
                        {
                            _histo -= std;
                        }
                        localUpdateAutocount();
                        break;
                    case XObjectChange.Value:
                        switch (newValue)
                        {
                            case null:
                                /* N O O P */
                                break;
                            case true:
                                _histo += std;
                                break;
                            case false:
                                _histo -= std;
                                break;
                        }
                        break;
                }

                #region L o c a l F x
                void localUpdateAutocount()
                {
                    // Count the actual model XBO objects
                    if (std == StdMarkdownAttribute.model)
                    {
                        var root = pxel.AncestorsAndSelf().Last();
                        if (root.Has<IMarkdownContext>())
                        {
                            root.SetStdAttributeValue(StdMarkdownAttribute.autocount, _histo[StdMarkdownAttribute.model]);
                        }
                    }
                }
                #endregion L o c a l F x
            }
        }
#else
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
                    modelRoot.SetStdAttributeValue(StdMarkdownAttribute.autocount, autocount);
                    // [Careful]
                    // It's too racey here to try and compare counts.
                }
            }
            #endregion L o c a l F x
        }
#endif
    }
}
