using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_PredicateMarkdownContext
{
    [TestMethod, DoNotParallelize]
    public void Test_IsFilteringEdgeTests()
    {
        using var te = this.TestableEpoch();

        string actual, expected;
        List<string> builder = new();
        IList<SelectableQFModel> opc = 
            new ObservableCollection<SelectableQFModel>()
            .PopulateForDemo(10);

        subtest_TriggerBy_ProjectionBeforeState();
        subtest_TriggerBy_StateBeforeProjection();
        subtest_TriggerBy_FilteringState();
        subtest_TriggerBy_RecordsetProperty();

        #region S U B T E S T S
        void subtest_TriggerBy_ProjectionBeforeState()
        {
            var mdc = new MarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = (INotifyCollectionChanged)opc,
            };

            // In this test, the items are already populated
            // before switching into filter mode.
            mdc.QueryFilterConfig = QueryFilterConfig.Filter;
            Assert.IsTrue(mdc.IsFiltering, "Expecting ALWAYS TRUE in Filter mode.");

            actual = mdc.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" xitem=""[SelectableQFModel]"" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" xitem=""[SelectableQFModel]"" sort=""1"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" xitem=""[SelectableQFModel]"" sort=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" xitem=""[SelectableQFModel]"" sort=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" xitem=""[SelectableQFModel]"" sort=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" xitem=""[SelectableQFModel]"" sort=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" xitem=""[SelectableQFModel]"" sort=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" xitem=""[SelectableQFModel]"" sort=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" xitem=""[SelectableQFModel]"" sort=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" xitem=""[SelectableQFModel]"" sort=""9"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 examples of UNKNOWN ITEM WITH PRIMARY KEY."
            );
        }
        void subtest_TriggerBy_StateBeforeProjection()
        {
            var mdc = new MarkdownContext<SelectableQFModel>
            {
                QueryFilterConfig = QueryFilterConfig.Filter,
            };
            Assert.IsTrue(mdc.IsFiltering, "Expecting ALWAYS TRUE in Filter mode.");
            actual = mdc.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting EMPTY because ONP is not assigned yet."
            );

            mdc.ObservableNetProjection = (INotifyCollectionChanged)opc;

            actual = mdc.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" xitem=""[SelectableQFModel]"" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" xitem=""[SelectableQFModel]"" sort=""1"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" xitem=""[SelectableQFModel]"" sort=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" xitem=""[SelectableQFModel]"" sort=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" xitem=""[SelectableQFModel]"" sort=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" xitem=""[SelectableQFModel]"" sort=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" xitem=""[SelectableQFModel]"" sort=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" xitem=""[SelectableQFModel]"" sort=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" xitem=""[SelectableQFModel]"" sort=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" xitem=""[SelectableQFModel]"" sort=""9"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 10 examples of UNKNOWN ITEM WITH PRIMARY KEY.");
        }
        void subtest_TriggerBy_FilteringState()
        {
        }
        void subtest_TriggerBy_RecordsetProperty()
        {
        }
        #endregion S U B T E S T S
    }

    /// <summary>
    /// Try out some basic extenal filters.
    /// </summary>
    [TestMethod, DoNotParallelize]
    public async Task Test_TemporalAffinityQFModel()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        const bool INCLUDE_LIVE_DEMO = true;
        int COUNT = INCLUDE_LIVE_DEMO ? 37 : 31;

        var opc = new ObservableCollection<TemporalAffinityQFModel>();
        Assert.AreEqual(
            COUNT,
            opc.PopulateForDemo(includeLiveDemo: true).Count, 
            "Expecting initial population.");

        // Filter-only MDC: Wakes up loaded with opc as canon.
        var pmdc = new PredicateMarkdownContext<TemporalAffinityQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Filter,
            ObservableNetProjection = opc,
        };

        Assert.IsTrue(pmdc.IsFiltering);
        Assert.AreEqual(
            COUNT, 
            pmdc.CanonicalCount, 
            "Expecting CANONICAL COUNT is correct meaning canon is initialized.");

        Assert.AreEqual(
            COUNT, 
            pmdc.PredicateMatchCount, 
            "Expecting PREDICATE MATCH COUNT is correct meaning canon is initialized.");

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

        actual = pmdc.Model.ToString();
        actual.ToClipboardExpected();
        { }
        expected = @" 
<model>
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" xitem=""[TemporalAffinityQFModel]"" preview=""Brown Dog "" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" xitem=""[TemporalAffinityQFModel]"" preview=""Green Appl"" sort=""1"" ismatch=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" xitem=""[TemporalAffinityQFModel]"" preview=""Yellow Ban"" sort=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" xitem=""[TemporalAffinityQFModel]"" preview=""Blue Bird "" sort=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" xitem=""[TemporalAffinityQFModel]"" preview=""Red Cherry"" sort=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" xitem=""[TemporalAffinityQFModel]"" preview=""Black Cat "" sort=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" xitem=""[TemporalAffinityQFModel]"" preview=""Orange Fox"" sort=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" xitem=""[TemporalAffinityQFModel]"" preview=""White Rabb"" sort=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" xitem=""[TemporalAffinityQFModel]"" preview=""Purple Gra"" sort=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" xitem=""[TemporalAffinityQFModel]"" preview=""Gray Wolf "" sort=""9"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000a"" xitem=""[TemporalAffinityQFModel]"" preview=""Pink Flami"" sort=""10"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000b"" xitem=""[TemporalAffinityQFModel]"" preview=""Golden Lio"" sort=""11"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000c"" xitem=""[TemporalAffinityQFModel]"" preview=""Brown Bear"" sort=""12"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000d"" xitem=""[TemporalAffinityQFModel]"" preview=""Green Pear"" sort=""13"" ismatch=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000e"" xitem=""[TemporalAffinityQFModel]"" preview=""Red Strawb"" sort=""14"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000f"" xitem=""[TemporalAffinityQFModel]"" preview=""Black Pant"" sort=""15"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000010"" xitem=""[TemporalAffinityQFModel]"" preview=""Yellow Lem"" sort=""16"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000011"" xitem=""[TemporalAffinityQFModel]"" preview=""White Swan"" sort=""17"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000012"" xitem=""[TemporalAffinityQFModel]"" preview=""Purple Plu"" sort=""18"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000013"" xitem=""[TemporalAffinityQFModel]"" preview=""Blue Whale"" sort=""19"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000014"" xitem=""[TemporalAffinityQFModel]"" preview=""Elephant  "" sort=""20"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000015"" xitem=""[TemporalAffinityQFModel]"" preview=""Pineapple "" sort=""21"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000016"" xitem=""[TemporalAffinityQFModel]"" preview=""Shark     "" sort=""22"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000017"" xitem=""[TemporalAffinityQFModel]"" preview=""Owl       "" sort=""23"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000018"" xitem=""[TemporalAffinityQFModel]"" preview=""Giraffe   "" sort=""24"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000019"" xitem=""[TemporalAffinityQFModel]"" preview=""Coconut   "" sort=""25"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001a"" xitem=""[TemporalAffinityQFModel]"" preview=""Kangaroo  "" sort=""26"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001b"" xitem=""[TemporalAffinityQFModel]"" preview=""Dragonfrui"" sort=""27"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001c"" xitem=""[TemporalAffinityQFModel]"" preview=""Turtle    "" sort=""28"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001d"" xitem=""[TemporalAffinityQFModel]"" preview=""Mango     "" sort=""29"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001e"" xitem=""[TemporalAffinityQFModel]"" preview=""Should NOT"" sort=""30"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001f"" xitem=""[TemporalAffinityQFModel]"" preview=""Appetizer "" sort=""31"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000020"" xitem=""[TemporalAffinityQFModel]"" preview=""Errata    "" sort=""32"" ismatch=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000021"" xitem=""[TemporalAffinityQFModel]"" preview=""Happy Camp"" sort=""33"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000022"" xitem=""[TemporalAffinityQFModel]"" preview=""Great exam"" sort=""34"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000023"" xitem=""[TemporalAffinityQFModel]"" preview=""Applicatio"" sort=""35"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000024"" xitem=""[TemporalAffinityQFModel]"" preview=""App Store "" sort=""36"" />
</model>"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting modeled matches."
        );
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
    ""Duration"": ""00:00:00"",
    ""Remaining"": ""00:00:00"",
    ""TemporalAffinity"": null,
    ""TemporalChildAffinity"": null,
    ""TemporalAffinityCurrentTimeDomain"": null,
    ""Slots"": [],
    ""UtcStart"": null,
    ""UtcEnd"": null,
    ""AvailableTimeSpan"": null,
    ""IsDone"": null,
    ""OutOfTime"": false,
    ""IsPastDue"": null,
    ""Created"": ""2000-01-01T09:04:00+07:00"",
    ""ChainOfCustody"": ""{\r\n  \""Created\"": \""2000-01-01T09:04:00+07:00\"",\r\n  \""Coc\"": {}\r\n}"",
    ""Model"": ""<model preview=\""Item05    \"" />"",
    ""FullPath"": ""312d1c21-0000-0000-0000-000000000005\\312d1c21-0000-0000-0000-000000000004"",
    ""ParentPath"": ""312d1c21-0000-0000-0000-000000000005"",
    ""ParentId"": ""312d1c21-0000-0000-0000-000000000005"",
    ""Priority"": 630822890400000000,
    ""PriorityOverride"": null,
    ""IsRoot"": false,
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

        #endregion S U B T E S T S

    }
}
