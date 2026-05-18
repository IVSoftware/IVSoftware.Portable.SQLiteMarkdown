using IVSoftware.Portable.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public class ObservableQueryFilterCollection<T>
        : ObservableModeledCollection<T>
        , IObservableQueryFilterSource<T>
    {
        public ObservableQueryFilterCollection()
        {
        }
        protected MarkdownContext<T> MarkdownContext { get; }
    }
}
