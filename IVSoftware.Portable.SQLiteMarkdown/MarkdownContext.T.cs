using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class MarkdownContext<T> : MarkdownContext
    {
        public MarkdownContext() : base(typeof(T)) { }

        public new IReadOnlyList<T> PredicateMatchSubset
        {
            get
            {
                if (_predicateMatchSubset is null)
                {
                    _predicateMatchSubset = new ReadOnlyCollection<T>(PredicateMatchSubsetProtected);
                }
                return _predicateMatchSubset;
            }
        }
        IReadOnlyList<T>? _predicateMatchSubset = null;

        public new IList<T> PredicateMatchSubsetProtected
        {
            get
            {
                if(base.PredicateMatchSubsetProtected is not IList<T>)
                {
                    base.PredicateMatchSubsetProtected = new List<T>();
                }
                return (IList<T>)base.PredicateMatchSubsetProtected;
            }
        }

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
