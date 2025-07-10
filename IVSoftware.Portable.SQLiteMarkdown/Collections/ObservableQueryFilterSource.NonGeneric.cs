using IVSoftware.Portable.SQLiteMarkdown.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public class ObservableQueryFilterSource : ObservableQueryFilterSource<StringWrapper>
    {
        public ObservableQueryFilterSource() { }
        public ObservableQueryFilterSource(SelectionMode selectionMode) 
            : base(selectionMode) { }
    }
}
