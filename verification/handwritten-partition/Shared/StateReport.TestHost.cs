// Original diagnostic formatter, relocated into the verification host verbatim.
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;
internal static class StateReportTestHost
{
public static string StateReport(this IModeledCollection @this, bool includeRK = false)
        {
            var builder = new List<string>();

            if (@this.Model.To<MarkdownContext>() is { } mdc)
            {
                return localReportWithMDC();
            }
            else
            {
                return localReportIMCOnly();
            }

            #region L o c a l F x
            string localReportIMCOnly()
            {
                if (@this is IList list)
                {
                    builder.Add($"[Net: {list.Count}]");
                }
                else
                {
                    builder.Add($"[Net: null]");
                }
                return string.Join(", ", builder);
            }
            string localReportWithMDC()
            {
                builder.Add($"[IME Len: {mdc.InputText.Length}");
                builder.Add($"IsFiltering: {mdc.IsFiltering}]");
                if (@this is IList list)
                {
                    builder.Add($"[Net: {list.Count}");
                }
                else
                {
                    builder.Add($"[Net: null");
                }
                builder.Add($"CC: {mdc.CanonicalCount}");
                builder.Add($"PMC: {mdc.PredicateMatchCount}]");
                builder.Add($"[{mdc.QueryFilterConfig}: {mdc.SearchEntryState.ToFullKey()}");
                if (includeRK && @this.GetType().GetProperty("RouteKey") is { } pi)
                {
                    builder.Add($"{mdc.FilteringState.ToFullKey()}");
                    builder.Add($"{(((Enum)pi.GetValue(@this))?.ToFullKey() ?? "null")}]");
                }
                else
                {
                    builder.Add($"{mdc.FilteringState.ToFullKey()}]");
                }
                return string.Join(", ", builder);
            }
            #endregion L o c a l F x
        }
}
