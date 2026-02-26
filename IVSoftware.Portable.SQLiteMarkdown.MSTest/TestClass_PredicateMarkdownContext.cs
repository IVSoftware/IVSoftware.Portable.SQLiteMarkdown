using IVSoftware.Portable.SQLiteMarkdown.Common;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_PredicateMarkdownContext
{
    [TestMethod]
    public async Task Test_PMDCFSOL()
    {
        const int COUNT = 31;

        var opc = new ObservableCollection<AffinityQFModel>();
        Assert.AreEqual(
            COUNT,
            opc.PopulateForDemo().Count, 
            "Expecting initial population.");

        var pmdc = new PredicateMarkdownContext<AffinityQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Filter,
            ObservableProjection = opc,
        };
        Assert.AreEqual(
            0, 
            pmdc.UnfilteredCount, 
            "Expecting recordset IS NOT initialized.");
        pmdc.Recordset = opc;
        Assert.AreEqual(
            COUNT, 
            pmdc.UnfilteredCount, 
            "Expecting UNFILTERED COUNT is correct meaning RECORDSET is initialized.");

        Assert.AreEqual(
            SearchEntryState.QueryCompleteWithResults,
            pmdc.SearchEntryState,
            "Expecting state reflects recordset.");
        Assert.AreEqual(
            FilteringState.Armed,
            pmdc.FilteringState);
        { }

        pmdc.InputText = "green";
        await pmdc;
        { }
    }


    [TestMethod]
    public async Task Test_5_Items()
    {
        const int COUNT = 5;
        const string ParentId = "BC28ADEC-4F82-4B6F-B98A-284B528DD01A";
        string actual, expected;

        var opc = 
            new ObservableCollection<AffinityQFModel>()
            .PopulateForDemo(COUNT, PopulateOptions.RandomChecks);

        opc.Last().ParentPath = ParentId;

        Assert.AreEqual(
            COUNT,
            opc.Count,
            "Expecting initial population.");

        var pmdc = new PredicateMarkdownContext<AffinityQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Filter,
            ObservableProjection = (INotifyCollectionChanged)opc,
            Recordset = opc,
        };

        var az = pmdc.ObservableProjection;

        Assert.AreEqual(
            COUNT,
            pmdc.UnfilteredCount,
            "Expecting UNFILTERED COUNT is correct meaning RECORDSET is initialized.");

        Assert.AreEqual(
            SearchEntryState.QueryCompleteWithResults,
            pmdc.SearchEntryState,
            "Expecting state reflects recordset.");
        Assert.AreEqual(
            FilteringState.Armed,
            pmdc.FilteringState);
        { }

    }
}
