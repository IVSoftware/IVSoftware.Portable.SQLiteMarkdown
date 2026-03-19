using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class ModeledMarkdownContext<T>
        : IEnumerable 
        , IEnumerable<T>
    {

        public IEnumerator<T> GetEnumerator()
        {
            return
                Model
                .Descendants()
                .Select(_ => _.To<T>())
                .OfType<T>()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
