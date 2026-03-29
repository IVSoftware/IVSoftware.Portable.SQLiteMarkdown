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
    public enum HistogrammerFormat
    {
        All,

        [HistogrammerFormat(
            StdMarkdownAttribute.model, 
            StdMarkdownAttribute.match,
            StdMarkdownAttribute.qmatch,
            StdMarkdownAttribute.pmatch)]
        Default,
    }
    partial class MarkdownContext
    {
        protected EnumHistogrammer<StdMarkdownAttribute> Histo { get; } = new(ZeroCountOption.Remove);
        public string ToString(HistogrammerFormat format) => Histo.ToString(format);

        Dictionary<XObject, XElement> _parentsOfRemoved = new();
        Dictionary<XAttribute, bool?> _oldValues = new();

        public virtual XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new
                        XElement(nameof(StdMarkdownElement.model))
                        .WithBoundAttributeValue(this, StdMarkdownAttribute.mdc, "[MDC]")
                        .WithBoundAttributeValue(Histo, StdMarkdownAttribute.histoZ, "[Histo]");

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
                                                Histo.Decrement(std);
                                            }
                                            else if (newValue == true)
                                            {
                                                Histo.Increment(std);
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
                            Histo.Increment(std);
                        }
                        localUpdateHisto();
                        break;
                    case XObjectChange.Remove:
                        if (newValue != false)
                        {
                            Histo.Decrement(std);
                        }
                        localUpdateHisto();
                        break;
                    case XObjectChange.Value:
                        switch (newValue)
                        {
                            case null:
                                /* N O O P */
                                break;
                            case true:
                                Histo.Increment(std);
                                break;
                            case false:
                                Histo.Decrement(std);
                                break;
                        }
                        break;
                }
                if(xattr is XBoundAttribute xba)
                {
                    OnBoundItemObjectChange(xba, e.ObjectChange);
                }

                #region L o c a l F x
                void localUpdateHisto()
                {
                    if(Model.Attribute(StdMarkdownAttribute.histoZ) is XBoundAttribute xba)
                    {
                        xba.Value = Histo.ToString(HistogrammerFormat.Default);
                    }
                }
                #endregion L o c a l F x
            }
        }
    }
}
