using IVSoftware.Portable.Xml.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            return
                Model
                .Descendants()
                .Attributes()
                .OfType<XBoundAttribute>()
                .Where(_ => _.Tag.GetType() == ContractType)
                .Select(_=>_.Tag)
                .GetEnumerator();
        }
    }
    partial class MarkdownContext<T> : IEnumerable<T>
    {
        public new IEnumerator<T> GetEnumerator() => (IEnumerator<T>) base.GetEnumerator();
    }
}
