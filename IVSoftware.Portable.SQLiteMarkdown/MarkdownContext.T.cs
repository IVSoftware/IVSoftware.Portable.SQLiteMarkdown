using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class MarkdownContext<T> : MarkdownContext
    {
        public MarkdownContext() : base(typeof(T)) { }
        public IReadOnlyList<T> PredicateMatchSubset { get; } = new List<T>();
        protected List<T> PredicateMatchSubsetProtected { get; } = new List<T>();
        protected override void OnModelSettled(NotifyCollectionChangedEventArgs eBCL)
        {
            base.OnModelSettled(eBCL);
        }
        protected override void OnXAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            base.OnXAttributeChanged(xattr, e);
        }
    }
}
