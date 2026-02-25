using IVSoftware.Portable.SQLiteMarkdown.Common;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_FilteredMarkdownContext
{
    [TestMethod]
    public void Test_FMDCFSOL()
    {
        var opc = new ObservableCollection<AffinityQFModel>();
        Assert.AreEqual(31, opc.Populate().Count, "Expecting initial population.");

        var fmdc = new FilteredMarkdownContext<int>
        {
            QueryFilterConfig = QueryFilterConfig.Filter,
            ObservableProjection = opc,
        };
        fmdc.Recordset = opc;

        Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, fmdc.SearchEntryState);
        Assert.AreEqual(FilteringState.Armed, fmdc.FilteringState);
        { }
    }
}
