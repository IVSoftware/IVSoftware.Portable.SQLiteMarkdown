using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public class MarkdownContext<T> : MarkdownContext
    {
        public MarkdownContext() : base(typeof(T)) { }
        public MarkdownContext(IList projection) : base(typeof(T), projection) { }
    }
}
