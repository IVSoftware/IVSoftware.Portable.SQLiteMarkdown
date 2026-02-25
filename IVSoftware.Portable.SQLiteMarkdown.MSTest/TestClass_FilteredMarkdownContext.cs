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

        var pmdc = new PredicateMarkdownContext<AffinityQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Filter,
            ObservableProjection = opc,
        };
        Assert.AreEqual(0, pmdc.UnfilteredCount, "Expecting recordset IS NOT initialized.");
        pmdc.Recordset = opc;
        Assert.AreEqual(31, pmdc.UnfilteredCount, "Expecting recordset IS initialized.");

        Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, pmdc.SearchEntryState);
        Assert.AreEqual(FilteringState.Armed, pmdc.FilteringState);
        { }
    }
}
