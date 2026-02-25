using IVSoftware.Portable.SQLiteMarkdown.Common;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_FilteredMarkdownContext
{
    [TestMethod]
    public void Test_PMDCFSOL()
    {
        var opc = new ObservableCollection<AffinityQFModel>();
        Assert.AreEqual(31, opc.Populate().Count, "Expecting initial population.");

        var pmdc = new PredicateMarkdownContext<int>
        {
            QueryFilterConfig = QueryFilterConfig.Filter,
            ObservableProjection = opc,
        };
        pmdc.Recordset = opc;

        Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, pmdc.SearchEntryState);
        Assert.AreEqual(FilteringState.Armed, pmdc.FilteringState);
        { }
    }
}
