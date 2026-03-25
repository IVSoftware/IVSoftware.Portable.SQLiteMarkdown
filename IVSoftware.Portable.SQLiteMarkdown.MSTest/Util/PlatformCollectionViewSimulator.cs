using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.StateMachine;
using System.Collections;
using System.Collections.Specialized;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Util
{
    class PlatformCollectionViewSimulator<T> : ObservablePreviewCollection<T>
    {
        public PlatformCollectionViewSimulator(IList itemsSource)
        {
            ItemsSource = itemsSource;
        }
        public IList ItemsSource { get; }

        /// <summary>
        /// Intercept and suppress the BC CollectionChanged event.
        /// </summary>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (ViewAuthority.Authority)
            {
                case CollectionChangeAuthority.Settle:
                case CollectionChangeAuthority.Predicate:
                    base.OnCollectionChanged(e);
                    break;
                default:
                    ItemsSource.Apply(e);
                    break;
            }
        }
        AuthorityEpochProvider ViewAuthority { get; } = new ();
    }
}
