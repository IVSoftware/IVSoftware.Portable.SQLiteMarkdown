using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
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

        var opc = new ObservableCollection<TemporalAffinityQFModel>();
        Assert.AreEqual(
            COUNT,
            opc.PopulateForDemo().Count, 
            "Expecting initial population.");

        var pmdc = new PredicateMarkdownContext<TemporalAffinityQFModel>
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


    [TestMethod, DoNotParallelize]
    public async Task Test_5_Items()
    {
        using var te = this.TestableEpoch();

        const int COUNT = 5;
        string parentId;
        string actual, expected, sql;
        List<TemporalAffinityQFModel> recordset;

        using var cnx = new SQLiteConnection(":memory:");
        cnx.CreateTable<TemporalAffinityQFModel>();

        IList<TemporalAffinityQFModel> opc;

        await subtest_EnsureParentIdSetterWorksDRY();
        await subtest_5_Items2();
        await subtest_5_Items3();
        await subtest_5_Items4();
        await subtest_5_Items5();
        await subtest_5_Items6();
        await subtest_5_Items7();
        await subtest_5_Items8();
        await subtest_5_Items9();
        await subtest_5_Items10();

        #region S U B T E S T S
        async Task subtest_EnsureParentIdSetterWorksDRY()
        {
            opc =
               new ObservableCollection<TemporalAffinityQFModel>()
               .PopulateForDemo(COUNT, PopulateOptions.RandomChecks);

            // Assign a parent path to last item.
            parentId = new Guid().WithTestability().ToString();
            opc.Last().ParentPath = parentId;

            Assert.AreEqual(
                COUNT,
                opc.Count,
                "Expecting initial population.");

            Assert.AreEqual(COUNT, cnx.InsertAll(opc));

            // Query SPECIFICALLY on ParentId alone.
            sql = $"Select * from items where ParentId='{parentId}'";
            recordset = cnx.Query<TemporalAffinityQFModel>(sql);


            actual = JsonConvert.SerializeObject(recordset, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""FullPath"": ""312d1c21-0000-0000-0000-000000000005\\312d1c21-0000-0000-0000-000000000004"",
    ""ParentPath"": ""312d1c21-0000-0000-0000-000000000005"",
    ""ParentId"": ""312d1c21-0000-0000-0000-000000000005"",
    ""Duration"": ""00:00:00"",
    ""Remaining"": ""00:00:00"",
    ""TemporalAffinity"": null,
    ""TemporalChildAffinity"": null,
    ""TemporalAffinityCurrentTimeDomain"": null,
    ""Slots"": [],
    ""Priority"": 630822890400000000,
    ""PriorityOverride"": null,
    ""UtcStart"": null,
    ""UtcEnd"": null,
    ""AvailableTimeSpan"": null,
    ""IsRoot"": false,
    ""IsDone"": null,
    ""OutOfTime"": false,
    ""IsPastDue"": null,
    ""Created"": ""2000-01-01T09:04:00+07:00"",
    ""ChainOfCustody"": ""{\r\n  \""Created\"": \""2000-01-01T09:04:00+07:00\"",\r\n  \""Coc\"": {}\r\n}"",
    ""CustomProperties"": ""{}"",
    ""Id"": ""312d1c21-0000-0000-0000-000000000004"",
    ""Description"": ""Item05"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": true,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000004"",
    ""QueryTerm"": ""item05"",
    ""FilterTerm"": ""item05"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item05\""\r\n}""
  }
]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting " +
                "1. Testable Guids and DateTimeOffset." +
                "2. Specifically, Last item is #...004 and ParentId is #...005."
            );
        }

        async Task subtest_5_Items2()
        {
#if false
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
#endif
        }
        async Task subtest_5_Items3()
        {
        }
        async Task subtest_5_Items4()
        {
        }
        async Task subtest_5_Items5()
        {
        }
        async Task subtest_5_Items6()
        {
        }
        async Task subtest_5_Items7()
        {
        }
        async Task subtest_5_Items8()
        {
        }
        async Task subtest_5_Items9()
        {
        }
        async Task subtest_5_Items10()
        {
        }
        #endregion S U B T E S T S

    }
}
