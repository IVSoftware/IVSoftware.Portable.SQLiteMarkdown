using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.StateMachine;
using System.Collections;
using System.Collections.Specialized;

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
            base.OnCollectionChanged(e);
        }
        AuthorityEpochProvider ViewAuthority { get; } = new ();
    }
}
