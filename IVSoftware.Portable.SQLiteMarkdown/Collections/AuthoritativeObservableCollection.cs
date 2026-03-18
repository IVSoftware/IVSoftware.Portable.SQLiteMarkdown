using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public class AuthoritativeObservableCollection<T> : ObservableCollection<T>
    {
        public AuthoritativeObservableCollection(IModeledMarkdownContext mmdc)
        {
            MMDC = mmdc;
        }
        IModeledMarkdownContext MMDC { get; }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Yield to ANY explicit authority.
            // All that matters is when IList redirects to collection *directly*.
            if (MMDC.Authority == 0)
            {
                base.OnCollectionChanged(e);
            }
            else
            {   /* G T K - N O O P */
            }
        }
    }
}
