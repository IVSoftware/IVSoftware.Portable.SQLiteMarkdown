using IVSoftware.Portable.SQLiteMarkdown.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// The non-generic "plug-and-play" class in version 1.0.0 had the unintended
    /// negative consequence (because of inverted inheritance) of giving incorrect
    /// type checking reads, i.e. 'if(someObject is ObservableQueryFilterSource){...}.
    /// </summary>
    /// <remarks>
    /// - Use the generic version, with StringWrapper for T, for a robust plug and play starter.
    /// - For type checking, use the interface, i.e. 'if(someObject is IObservableQueryFilterSource){...}.
    /// </remarks>
    [Obsolete("USE: ObservableQueryFilterSource<StringWrapper> and, for type checking, use 'IObservableQueryFilterSource')")]
    public abstract class ObservableQueryFilterSource { }
}
