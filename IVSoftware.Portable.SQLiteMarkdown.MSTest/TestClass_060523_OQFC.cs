using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.WinOS.MSTest.Extensions;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_060523_OQFC
{
    [TestMethod, DoNotParallelize]
    public void Test_IsFilteringEdgeTests()
    {
        using var te = this.TestableEpoch();

        string actual, expected;
        List<string> builder = new();
        foreach (var omc in MakeOMCs(QueryFilterConfig.QueryAndFilter))
        {
            subtest_PopulateBeforeFilter(omc);
        }
        foreach (var omc in MakeOMCs(QueryFilterConfig.Filter))
        {
            subtest_FilterBeforePopulate(omc);
        }
        foreach (var omc in MakeOMCs(QueryFilterConfig.Filter))
        {
            subtest_FilterBeforeReplace(omc);
        }
        #region S U B T E S T S
        void subtest_PopulateBeforeFilter(IModeledCollection<SelectableQFModel> omc)
        {
            IObservableQueryFilterSource<SelectableQFModel> 
                oqfs = (IObservableQueryFilterSource<SelectableQFModel>)omc;
            Assert.AreEqual(
                QueryFilterConfig.QueryAndFilter,
                oqfs.QueryFilterConfig, 
                "Expecting valid cast to IOQFS and default config.");

            omc.PopulateForDemo(10);
            actual = omc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 10, CC: 10, PMC: 0], [QueryAndFilter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 items presented as query result." +
                "MentalModel: 'New keystrokes will filter the visible collection'."
            );
            oqfs.QueryFilterConfig = QueryFilterConfig.Filter;
            actual = omc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 10, CC: 10, PMC: 0], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 items presented as canonical filter source." +
                "MentalModel: 'New keystrokes will filter the visible collection'."
            );
        }
        void subtest_FilterBeforePopulate(IModeledCollection<SelectableQFModel> omc)
        {
            IObservableQueryFilterSource<SelectableQFModel>
                oqfs = (IObservableQueryFilterSource<SelectableQFModel>)omc;
            Assert.AreEqual(
                QueryFilterConfig.Filter,
                oqfs.QueryFilterConfig,
                "Expecting valid cast to IOQFS and default config.");

            actual = omc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 0, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting state reflects empty canon." +
                "MentalModel: 'A filter-only collection is always either armed or active (even with 0 items).'"
            );

            omc.PopulateForDemo(10);
            actual = omc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 10, CC: 10, PMC: 0], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 items presented as canonical filter source."
            );
        }

        void subtest_FilterBeforeReplace(IModeledCollection<SelectableQFModel> omc)
        {
            IObservableQueryFilterSource<SelectableQFModel>
                oqfs = (IObservableQueryFilterSource<SelectableQFModel>)omc;
            Assert.AreEqual(
                QueryFilterConfig.Filter,
                oqfs.QueryFilterConfig,
                "Expecting valid cast to IOQFS and default config.");

            actual = omc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 0, CC: 0, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting state reflects empty canon."
                "MentalModel: 'A filter-only collection is always either armed or active (even with 0 items).'"
            );

            oqfs.ReplaceItems(default (IList<SelectableQFModel>).PopulateForDemo(10));
            actual = omc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 10, CC: 10, PMC: 0], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 items presented as canonical filter source." +
                "MentalModel: 'New keystrokes will filter the visible collection'."
            );
        }
        #endregion S U B T E S T S

        #region L o c a l F x
        IModeledCollection<SelectableQFModel>[] MakeOMCs(QueryFilterConfig config) =>
        [
            new ObservableQueryFilterCollection<SelectableQFModel>{ QueryFilterConfig = config },
            new ObservableQueryFilterSource<SelectableQFModel>{ QueryFilterConfig = config },
        ];
        #endregion L o c a l F x
    }
}
