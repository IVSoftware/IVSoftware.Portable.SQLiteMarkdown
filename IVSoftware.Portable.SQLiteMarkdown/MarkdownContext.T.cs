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

        protected override IList PredicateMatchSubsetProtected
        {
            get
            {
                if(base.PredicateMatchSubsetProtected is not IList<T>)
                {
                    base.PredicateMatchSubsetProtected = new List<T>();
                }
                return base.PredicateMatchSubsetProtected;
            }
        }
    }
}
