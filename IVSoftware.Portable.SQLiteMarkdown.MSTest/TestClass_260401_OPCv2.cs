using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.StateRunner.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260401_OPCv2
{


    [TestMethod, DoNotParallelize]
    [Claim("00000000-0000-0000-0000-000000000000")]
    public void Test_Method()
    {
        string actual, expected;
        var builder = new List<string>();
        using var te = this.TestableEpoch();

        #region I T E M    G E N
        IList<SelectableQFModel>? eph = null;
        #endregion I T E M    G E N

        var itemsSource = new ObservableCollection<SelectableQFModel>();

        #region E V E N T S
        itemsSource.CollectionChanged += (sender, e) =>
        {
            builder.Add(e.ToString(ReferenceEquals(sender, itemsSource)));
        };
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion S U B T E S T S
    }

    private class SuppressibleObservableCollection<T>
        : ObservableCollection<T>
        , INotifyCollectionChangedSuppressible
    {

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
        }
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
        }
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }
        protected override void ClearItems()
        {
            base.ClearItems();
        }

        public IDisposable BeginSuppressNotify()
        {
            throw new NotImplementedException();
        }

        public void CancelSuppressNotify()
        {
            throw new NotImplementedException();
        }
        public SuppressionPhase SuppressionPhase => (SuppressionPhase)AuthorityProvider.Authority;

        protected DHostCoalescingCollectionChange DHostCoalesce { get; } = new();

        protected AuthorityEpochProvider AuthorityProvider { get; } = new();
    }
}
