using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{

    partial class ModeledMarkdownContext<T> : ITopology
    {
        IList? ITopology.ObservableNetProjection => ObservableNetProjection;

        IList ITopology.CanonicalSuperset => (IList)CanonicalSuperset;

        IList ITopology.PredicateMatchSubset => (IList)PredicateMatchSubset;
    }
}