using IVSoftware.Portable.Collections.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public partial class ObservableQueryFilterSource<T>
        : MarkdownContext<T>
        , IObservableQueryFilterSource
        , IObservableQueryFilterSource<T>
    {
        public string Placeholder
        {
            get;
            protected set;
        } = string.Empty;

        public string Title
        {
            get;
            set;
        } = string.Empty;

        public string SQL => base.Query;

        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public void ReplaceItems(IEnumerable<T> items)
            => CanonicalSupersetProtected.LoadCanon((IList)items);

        public Task ReplaceItemsAsync(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }
    }
}
