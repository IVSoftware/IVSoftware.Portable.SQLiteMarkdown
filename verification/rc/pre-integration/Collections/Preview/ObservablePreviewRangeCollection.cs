using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class ObservablePreviewRangeCollection<T>
        : ObservableRangeCollection<T>
        , IRangeable
        where T : new()
    {
        public ObservablePreviewRangeCollection() { }
    }
}
