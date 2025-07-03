using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.DemoDB
{
    [DebuggerDisplay("{Description}")]
    [Table("items")]
    public class AtomicQuoteTestModel : SelfIndexed, ISelectableQueryFilterItem
    {
        public OnePageItemSelection Selection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
