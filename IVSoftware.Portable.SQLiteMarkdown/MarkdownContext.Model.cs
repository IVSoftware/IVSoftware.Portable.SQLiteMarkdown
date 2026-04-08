using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Common.Collections.Internal;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public enum ReportFormat
    {
        StateReport,

        /// <summary>
        /// For objects with a native Model property, or that have 
        /// an implicit cast to XElement, return model.ToString().
        /// </summary>
        ModelWithPreview,

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
                        XElement(nameof(StdModelElement.model))
                        .WithBoundAttributeValue(this, StdModelAttribute.mdc, "[MDC]")
                        .WithBoundAttributeValue(Histo, StdModelAttribute.histo, "[Histo]")
                        .WithBoundAttributeValue(ActiveFilters, StdModelAttribute.filters, "[No Active Filters]");

                    Model.Changing += (sender, e) => OnXModelChange(new(sender, e.ObjectChange, XModelChangeState.Changing));
                    Model.Changed += (sender, e) => OnXModelChange(new(sender, e.ObjectChange, XModelChangeState.Changed));
                }
                return _model;
            }
        }

        protected virtual void OnXModelChange(XModelChangeEventArgs value)
        {
            throw new NotImplementedException();
        }

        protected XElement? _model = null;
        protected virtual void OnXElementChanged(XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
            switch (e.ObjectChange)
            {
                case XObjectChange.Add:
                    foreach (var xattr in xel.Attributes())
                    {
                    }
                    // Now: IFTTT on the stable histogram population.
                    foreach (var xattr in xel.Attributes())
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdModelAttribute std)
                            && std.GetCustomAttribute<IFTTTAttribute>() is not null)
                        {
                            switch (std)
                            {
                                case StdModelAttribute.qmatch:
                                case StdModelAttribute.pmatch:
                                    // The IFTTT for 'match' wired and ready. 
                                    OnIFTTTAttributeChanged(xattr, pxel, e, std);
                                    break;
                            }
                        }
                    }
                    break;
                case XObjectChange.Remove:
                    foreach (var xattr in xel.Attributes())
                    {
                        if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdModelAttribute std))
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
            if (Enum.TryParse(xattr.Name.LocalName, ignoreCase: false, out StdModelAttribute std))
            {
                bool? newValue = bool.TryParse(xattr.Value, out var valid) ? valid : null;
                if (xattr is XBoundAttribute xba)
                {
                    OnXBoundAttributeChanged(xba, e.ObjectChange);
                }
                else
                {
                    switch (std)
                    {
                        case StdModelAttribute.qmatch:
                        case StdModelAttribute.pmatch:
#if DEBUG
                            switch (e.ObjectChange)
                            {
                                case XObjectChange.Add:
                                    break;
                                case XObjectChange.Remove:
                                    break;
                                case XObjectChange.Value:
                                    break;
                            }
#endif
                            if (std.GetCustomAttribute<IFTTTAttribute>() is not null)
                            {
                                OnIFTTTAttributeChanged(xattr, pxel, e, std);
                            }
                            break;
                    }
                }
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
        protected virtual void OnIFTTTAttributeChanged(XAttribute xattr, XElement pxel, XObjectChangeEventArgs e, Enum std)
        {
            bool valid; // Captures an explicit value, if parseable.

            // Capture value now, without taking removal into consideration.
            bool? 
                match =  bool.TryParse(pxel.Attribute(StdModelAttribute.match)?.Value, out valid) ? valid : null,
                qmatch = bool.TryParse(pxel.Attribute(StdModelAttribute.qmatch)?.Value, out valid) ? valid : null,
                pmatch = bool.TryParse(pxel.Attribute(StdModelAttribute.pmatch)?.Value, out valid) ? valid : null;

            switch (std)
            {
                case StdModelAttribute.pmatch:
                    switch (e.ObjectChange)
                    {
                        case XObjectChange.Add:
                            pxel.SetStdModelAttributeValue(StdModelAttribute.match, true);
                            break;
                        case XObjectChange.Remove:
                            // Match relies on the 'other'.
                            pxel.SetStdModelAttributeValue(StdModelAttribute.match, qmatch);
                            break;
                        case XObjectChange.Value:
                            if (pmatch == true)
                            {   /* G T K */
                                // Idempotent change
                                Debug.Assert(match == true);
                            }
                            else
                            {
                                Debug.Fail($@"ADVISORY - First Time and it's Not Good.");
                            }
                            break;
                    }
                    break;
                case StdModelAttribute.qmatch:
                    switch (e.ObjectChange)
                    {
                        case XObjectChange.Add:
                            pxel.SetStdModelAttributeValue(StdModelAttribute.match, true);
                            break;
                        case XObjectChange.Remove:
                            // Match relies on the 'other'.
                            pxel.SetStdModelAttributeValue(StdModelAttribute.match, pmatch);
                            break;
                        case XObjectChange.Value:
                            if (qmatch == true)
                            {   /* G T K */
                                // Idempotent change
                                Debug.Assert(match == true);
                            }
                            else
                            {
                                Debug.Fail($@"ADVISORY - First Time and it's Not Good.");
                            }
                            break;
                    }
                    break;
            }

            //// If none of the items have a qmatch then *all* of them implicily have a qmatch.
            //bool? qmatch =
            //    Histo[StdModelAttribute.qmatch] == 0
            //    ? null
            //    : bool.TryParse(@this.Attribute(StdModelAttribute.qmatch)?.Value, out valid) ? valid : null;

            //// If none of the items have a pmatch then *all* of them implicily have a pmatch.
            //bool? pmatch =
            //    Histo[StdModelAttribute.pmatch] == 0
            //    ? null
            //    : bool.TryParse(@this.Attribute(StdModelAttribute.pmatch)?.Value, out valid) ? valid : null;
            //if (qmatch == true || pmatch == true)
            //{
            //    @this.SetStdModelAttributeValue(StdModelAttribute.match, bool.TrueString);
            //}
            //else
            //{
            //    @this.SetStdModelAttributeValue(StdModelAttribute.match, null);
            //}
        }

        protected EnumHistogrammer<StdModelAttribute> Histo { get; } = new(ZeroCountOption.Remove);
        public string ToString(HistogrammerFormat formatting) => Histo.ToString(formatting);
        public string ToString(ModelPreviewDelegate preview, bool keepPreviews = false)
        {
            foreach (var xel in Model.Descendants())
            {
                if(xel.Attribute(StdModelAttribute.model) is XBoundAttribute xba && xba.Tag is not null)
                {
                    xel.SetStdModelAttributeValue(StdModelAttribute.preview, preview(xba.Tag));
                }
            }
            var @string = Model.ToString();
            if(!keepPreviews)
            {
                Model.RemoveDescendantAttributes(StdModelAttribute.preview);
            }
            return @string;
        }
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
                case ReportFormat.ModelWithPreview:
                    if(ContractType.GetDescriptionPreviewDlgt() is { } dlgt)
                    {
                        var needPreview =
                            Model
                            .Descendants()
                            .Select(_=>_.Attribute(StdModelAttribute.model))
                            .OfType<XBoundAttribute>()
                            .Where(_=>_.Parent.Attribute(StdModelAttribute.preview) is null)
                            .ToArray();

                        foreach (var xba in needPreview)
                        {
                            xba.Parent.SetStdModelAttributeValue(StdModelAttribute.preview, dlgt(xba.Tag));
                        }
                        var report = Model.ToString();

                        foreach (var xba in needPreview)
                        {
                            xba.Parent.SetStdModelAttributeValue(StdModelAttribute.preview, null);
                        }
                        return report;
                    }
                    else
                    {
                        return Model.ToString();
                    }
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
    }
}
