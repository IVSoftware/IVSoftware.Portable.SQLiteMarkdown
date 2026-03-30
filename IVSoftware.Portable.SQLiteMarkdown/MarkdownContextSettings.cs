using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public enum StdMarkdownContextSetting
    {
        /// <summary>
        /// Enables a fallback that (naively) pluralizes
        /// terms when a query produces no results, 
        /// </summary>
        AllowPluralize,

        /// <summary>
        /// Enables custom visual cues when a filter hides all the unfiltered items.
        /// </summary>
        UseAdaptiveShowAll,
    }

    /// <summary>
    /// Forward reference for settings dictionary with persistence.
    /// </summary>
    public class MarkdownContextSettings : Dictionary<StdMarkdownContextSetting, object> { }
}
