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

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (ViewAuthority.Authority)
            {
                case ModeledCollectionChangeAuthority.Settle:
                case ModeledCollectionChangeAuthority.Predicate:
                    break;
                default:
                    base.OnCollectionChanged(e);
                    ItemsSource.Apply(e);
                    break;
            }
        }
        AuthorityEpochProvider ViewAuthority { get; } = new ();
    }
}
