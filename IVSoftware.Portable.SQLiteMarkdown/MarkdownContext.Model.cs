using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Xml.Schema;

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
    public enum ReportFormat
    {
        StateReport,

        /// <summary>
        /// For objects with a native Model property, or that have 
        /// an implicit cast to XElement, return model.ToString().
        /// </summary>
        Model,

        //OptionsReport,
        //SettingsReport,
    }
    partial class MarkdownContext
    {
        public virtual XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new
                        XElement(nameof(StdMarkdownElement.model))
                        .WithBoundAttributeValue(this, StdMarkdownAttribute.mdc, "[MDC]")
                        .WithBoundAttributeValue(Histo, StdMarkdownAttribute.histo, "[Histo]")
                        .WithBoundAttributeValue(ActiveFilters, StdMarkdownAttribute.filters, "[No Active Filters]");

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
                                                Histo.Increment(std, xattr);
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
                    foreach (var xattr in xel.Attributes())
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std))
                        {
                            if (bool.TryParse(xattr.Value, out bool valid) && valid == false)
                            {   /* G T K - N O O P */
                                // POLICY: Explicit false values cannot modify the histogram.
                            }
                            else
                            {
                                // Increment *all* first.
                                Histo.Increment(std, xattr);
                            }
                        }
                    }
                    // Now: IFTTT on the stable histogram population.
                    foreach (var xattr in xel.Attributes())
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std)
                            && std.GetCustomAttribute<IFTTTAttribute>() is not null)
                        {
                            switch (std)
                            {
                                case StdMarkdownAttribute.qmatch:
                                case StdMarkdownAttribute.pmatch:
                                    // The IFTTT for 'match' wired and ready. 
                                    SetMatchAttributeValue(xel);
                                    break;
                            }
                        }
                    }
                    break;
                case XObjectChange.Remove:
                    foreach (var xattr in xel.Attributes())
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdMarkdownAttribute std))
                        {
                            if (bool.TryParse(xattr.Value, out bool valid) && valid == false)
                            {   /* G T K - N O O P */
                                // POLICY: Explicit false values cannot modify the histogram.
                            }
                            else
                            {
                                Histo.Decrement(std);
                            }
                        }
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
                            Histo.Increment(std, xattr);
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
                                Histo.Increment(std, xattr);
                                break;
                            case false:
                                Histo.Decrement(std);
                                break;
                        }
                        break;
                }
                if (xattr is XBoundAttribute xba)
                {
                    OnBoundItemObjectChange(xba, e.ObjectChange);
                }
                else
                {
                    switch (std)
                    {
                        case StdMarkdownAttribute.qmatch:
                        case StdMarkdownAttribute.pmatch:
                            SetMatchAttributeValue(pxel);
                            break;
                    }
                }
                #region L o c a l F x
                void localUpdateHisto()
                {
                    if (Model.Attribute(StdMarkdownAttribute.histo) is XBoundAttribute xba)
                    {
                        xba.Value = Histo.ToString(HistogrammerFormat.Default);
                    }
                }
                #endregion L o c a l F x
            }
        }

        /// <summary>
        /// Sets the 'match' attribute based on any explicit positive match signal.
        /// </summary>
        /// <remarks>
        /// - The 'qmatch' and 'pmatch' values are normalized to nullable signals
        ///   using histogram participation.
        /// - Mental Model:
        ///   "If no descendants explicitly match, all descendants are considered matches (no filter)."
        /// - The 'match' attribute is set explicity true if either 'qmatch' or 'pmatch'
        ///   are explictly true, otherwise null.
        /// EXAMPLE:
        /// 1. IME text is cleared -> all 'qmatch' attributes are removed -> show all items.
        /// 2. User enters text -> some items are marked 'qmatch' -> show only matching items.
        /// </remarks>
        void SetMatchAttributeValue(XElement @this)
        {
            bool valid; // Captures an explicit value, if parseable.

            // If none of the xitems have a qmatch then *all* of them implicily have a qmatch.
            bool? qmatch =
                Histo[StdMarkdownAttribute.qmatch] == 0
                ? null
                : bool.TryParse(@this.Attribute(StdMarkdownAttribute.qmatch)?.Value, out valid) ? valid : null;

            // If none of the xitems have a pmatch then *all* of them implicily have a pmatch.
            bool? pmatch =
                Histo[StdMarkdownAttribute.pmatch] == 0
                ? null
                : bool.TryParse(@this.Attribute(StdMarkdownAttribute.pmatch)?.Value, out valid) ? valid : null;
            if (qmatch == true || pmatch == true)
            {
                @this.SetStdAttributeValue(StdMarkdownAttribute.match, bool.TrueString);
            }
            else
            {
                @this.SetStdAttributeValue(StdMarkdownAttribute.match, null);
            }
        }

        protected EnumHistogrammer<StdMarkdownAttribute> Histo { get; } = new(ZeroCountOption.Remove);
        public string ToString(HistogrammerFormat formatting) => Histo.ToString(formatting);
        public string ToString(ReportFormat formattime)
        {
            var builder = new List<string>();
            switch (formattime)
            {
                case ReportFormat.StateReport:
                    builder.Add($"[IME Len: {InputText.Length}");
                    builder.Add($"IsFiltering: {IsFiltering}]");
                    if (this is IModeledMarkdownContext mmdc)
                    {
                        builder.Add($"[Net: {(mmdc.ObservableNetProjection is IList list ? list.Count : "null")}");
                    }
                    builder.Add($"CC: {CanonicalCount}");
                    builder.Add($"PMC: {PredicateMatchCount}]");
                    builder.Add($"[{QueryFilterConfig}: {SearchEntryState.ToFullKey()}");
                    builder.Add($"{FilteringState.ToFullKey()}]");
                    break;
                //case ReportFormat.OptionsReport:
                //    builder.Add($"{ProjectionTopology.ToFullKey()}");
                //    builder.Add($"{ReplaceItemsEventingOption.ToFullKey()}");
                //    return string.Join(", ", builder);
                default:
                    this.ThrowHard<NotSupportedException>($"The {formattime.ToFullKey()} case is not supported.");
                    break;
            }
            return string.Join(", ", builder);
        }

        public IReadOnlyDictionary<string, Enum> ActiveFilters
        {
            get
            {
                if (_activeFilters is null)
                {
                    _activeFilters = new ReadOnlyDictionary<string, Enum>(ActiveFiltersProtected);
                }
                return _activeFilters;
            }
        }
        IReadOnlyDictionary<string, Enum>? _activeFilters = null;
        protected Dictionary<string, Enum> ActiveFiltersProtected { get; } = new();


        Dictionary<XObject, XElement> _parentsOfRemoved = new();
        Dictionary<XAttribute, bool?> _oldValues = new();
    }
}
