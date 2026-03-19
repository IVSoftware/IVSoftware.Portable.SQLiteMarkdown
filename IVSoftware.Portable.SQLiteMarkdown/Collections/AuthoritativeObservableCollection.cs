using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    public class AuthoritativeObservableCollection<T> : ObservableCollection<T>
    {
        public AuthoritativeObservableCollection(Func<CollectionChangeAuthority> authorityRequest)
        {
            _authorityRequest = authorityRequest;
        }

        private readonly Func<CollectionChangeAuthority> _authorityRequest;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Yield to ANY explicit authority.
            // All that matters is when IList redirects to collection *directly*.

            switch (_authorityRequest())
            {
                case 0:
                    base.OnCollectionChanged(e);
                    break;
                case CollectionChangeAuthority.None:
                case CollectionChangeAuthority.Model:
                case CollectionChangeAuthority.Projection:
                default:
                    break;
            }
        }
    }
}
