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

        public bool HasCounts(int canonical, int matches, int? database = null)
        {
            if(canonical != CanonicalCount)
            {
                return false;
            }
            if(matches != PredicateMatchCount)
            {
                return false;
            }
            if(database is not null)
            {
                var preview = FilterQueryDatabase.ExecuteScalar<int>("Select count(*) from items");
                if(database != preview)
                {
                    return false;
                }
            }
            return true;
        }
    }
    partial class MarkdownContext<T> : IEnumerable<T>
    {
        public new IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException("ToDo");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
