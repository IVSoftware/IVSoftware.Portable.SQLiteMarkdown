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
[IME Len: 0, IsFiltering: False], [Net: 10, CC: 10, PMC: 0], [QueryAndFilter: SearchEntryState.Cleared, FilteringState.Ineligible]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 items presented as query result."
            );
            oqfs.QueryFilterConfig = QueryFilterConfig.Filter;
            actual = omc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 10, CC: 10, PMC: 0], [Filter: SearchEntryState.QueryCompleteNoResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 items presented as canonical filter source."
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
                "Expecting state reflects empty canon."
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
                "Expecting 10 items presented as canonical filter source."
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
