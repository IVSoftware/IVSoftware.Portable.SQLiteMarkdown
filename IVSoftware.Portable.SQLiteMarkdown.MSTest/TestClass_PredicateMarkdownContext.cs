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
        using var te = this.TestableEpoch();

        const int COUNT = 5;
        string ParentId = new Guid().WithTestability().ToString();
        string actual, expected, sql;
        List<AffinityQFModel> recordset;

        using var cnx = new SQLiteConnection(":memory:");
        cnx.CreateTable<AffinityQFModel>();

        IList<AffinityQFModel> opc;

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
               new ObservableCollection<AffinityQFModel>()
               .PopulateForDemo(COUNT, PopulateOptions.RandomChecks);
            // Assign a parent path to last item.
            opc.Last().ParentPath = ParentId;

            Assert.AreEqual(
                COUNT,
                opc.Count,
                "Expecting initial population.");

            Assert.AreEqual(COUNT, cnx.InsertAll(opc));

            // Query SPECIFICALLY on ParentId alone.
            sql = $"Select * from items where ParentId='{ParentId}'";
            recordset = cnx.Query<AffinityQFModel>(sql);


            actual = JsonConvert.SerializeObject(recordset, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Priority"": 630822892800000000,
    ""ParentPath"": ""312d1c21-0000-0000-0000-000000000000"",
    ""ParentId"": ""312d1c21-0000-0000-0000-000000000000"",
    ""UtcStart"": null,
    ""Duration"": ""00:00:00"",
    ""Remaining"": ""00:00:00"",
    ""AffinityMode"": null,
    ""AffinityParent"": null,
    ""AffinityChildMode"": null,
    ""Slots"": [],
    ""AffinityTimeDomain"": null,
    ""IsRoot"": false,
    ""IsTimeDomainEnabled"": false,
    ""UtcEnd"": null,
    ""IsDone"": null,
    ""IsDonePendingConfirmation"": null,
    ""IsPastDue"": null,
    ""Available"": null,
    ""Created"": ""2000-01-01T09:10:00+07:00"",
    ""ChainOfCustody"": ""{\r\n  \""Created\"": \""2000-01-01T09:09:00+07:00\"",\r\n  \""Coc\"": {}\r\n}"",
    ""CustomProperties"": ""{}"",
    ""Id"": ""312d1c21-0000-0000-0000-000000000005"",
    ""Description"": ""Item05"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": true,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000005"",
    ""QueryTerm"": ""item05"",
    ""FilterTerm"": ""item05"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item05\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting json serialization to match with Testable Guids and DateTimeOffset."
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
