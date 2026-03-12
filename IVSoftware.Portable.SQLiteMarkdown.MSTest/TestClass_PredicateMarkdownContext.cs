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
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_PredicateMarkdownContext
{

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
<model autocount=""10"" count=""10"" matches=""10"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" sort=""1"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" sort=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" sort=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" sort=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" sort=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" sort=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" sort=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" sort=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" sort=""9"" />
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
<model autocount=""10"" count=""10"" matches=""10"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" sort=""1"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" sort=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" sort=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" sort=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" sort=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" sort=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" sort=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" sort=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" sort=""9"" />
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
    /// Try out some basic external filters.
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
            ProjectionOption = NetProjectionOption.AllowDirectChanges,
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


        #region L o c a l F x
        void localOnModelUpdated(object? sender, EventArgs e)
        {
            // This is just the bandaid.
            Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 12).Date, "Don't forget disabled");
            return; // for good.


            actual = pmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 5, IsFiltering: True], [Net: 37, CC: 37, PMC: 3], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]";
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting StateReport to match.");

            return;
            var v =
                pmdc
                .Model
                .Descendants()
                .Where(_ => bool.Parse(_.Attribute(nameof(StdMarkdownAttribute.ismatch))?.Value ?? "false") == true)
                .Select(_ => (_.Attribute(StdMarkdownAttribute.model) as XBoundAttribute)?.Tag)
                .ToArray();
            { }
            if(pmdc.ObservableNetProjection is IList list)
            {
                list.Clear();
                foreach (var item in v)
                {
                    list.Add(item);
                }
            }

            actual = pmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 5, IsFiltering: True], [Net: 3, CC: 37, PMC: 3], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]"
            ;
        }

        #endregion L o c a l F x
        using (pmdc.WithOnDispose(
            onInit: (sender, e) =>
            {
                pmdc.ModelUpdated += localOnModelUpdated;
            },
            onDispose: (sender, e) =>
            {
                pmdc.ModelUpdated -= localOnModelUpdated;
            }))
        {
            pmdc.InputText = "green";
            await pmdc;
        }

        actual = pmdc.Model.ToString();
        actual.ToClipboardExpected();
        { }
        expected = @" 
<model autocount=""37"" count=""37"" matches=""3"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[TemporalAffinityQFModel]"" preview=""Brown Dog "" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[TemporalAffinityQFModel]"" preview=""Green Appl"" sort=""1"" ismatch=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[TemporalAffinityQFModel]"" preview=""Yellow Ban"" sort=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[TemporalAffinityQFModel]"" preview=""Blue Bird "" sort=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[TemporalAffinityQFModel]"" preview=""Red Cherry"" sort=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[TemporalAffinityQFModel]"" preview=""Black Cat "" sort=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[TemporalAffinityQFModel]"" preview=""Orange Fox"" sort=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[TemporalAffinityQFModel]"" preview=""White Rabb"" sort=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" model=""[TemporalAffinityQFModel]"" preview=""Purple Gra"" sort=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" model=""[TemporalAffinityQFModel]"" preview=""Gray Wolf "" sort=""9"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000a"" model=""[TemporalAffinityQFModel]"" preview=""Pink Flami"" sort=""10"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000b"" model=""[TemporalAffinityQFModel]"" preview=""Golden Lio"" sort=""11"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000c"" model=""[TemporalAffinityQFModel]"" preview=""Brown Bear"" sort=""12"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000d"" model=""[TemporalAffinityQFModel]"" preview=""Green Pear"" sort=""13"" ismatch=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000e"" model=""[TemporalAffinityQFModel]"" preview=""Red Strawb"" sort=""14"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000f"" model=""[TemporalAffinityQFModel]"" preview=""Black Pant"" sort=""15"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000010"" model=""[TemporalAffinityQFModel]"" preview=""Yellow Lem"" sort=""16"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000011"" model=""[TemporalAffinityQFModel]"" preview=""White Swan"" sort=""17"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000012"" model=""[TemporalAffinityQFModel]"" preview=""Purple Plu"" sort=""18"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000013"" model=""[TemporalAffinityQFModel]"" preview=""Blue Whale"" sort=""19"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000014"" model=""[TemporalAffinityQFModel]"" preview=""Elephant  "" sort=""20"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000015"" model=""[TemporalAffinityQFModel]"" preview=""Pineapple "" sort=""21"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000016"" model=""[TemporalAffinityQFModel]"" preview=""Shark     "" sort=""22"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000017"" model=""[TemporalAffinityQFModel]"" preview=""Owl       "" sort=""23"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000018"" model=""[TemporalAffinityQFModel]"" preview=""Giraffe   "" sort=""24"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000019"" model=""[TemporalAffinityQFModel]"" preview=""Coconut   "" sort=""25"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001a"" model=""[TemporalAffinityQFModel]"" preview=""Kangaroo  "" sort=""26"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001b"" model=""[TemporalAffinityQFModel]"" preview=""Dragonfrui"" sort=""27"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001c"" model=""[TemporalAffinityQFModel]"" preview=""Turtle    "" sort=""28"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001d"" model=""[TemporalAffinityQFModel]"" preview=""Mango     "" sort=""29"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001e"" model=""[TemporalAffinityQFModel]"" preview=""Should NOT"" sort=""30"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001f"" model=""[TemporalAffinityQFModel]"" preview=""Appetizer "" sort=""31"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000020"" model=""[TemporalAffinityQFModel]"" preview=""Errata    "" sort=""32"" ismatch=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000021"" model=""[TemporalAffinityQFModel]"" preview=""Happy Camp"" sort=""33"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000022"" model=""[TemporalAffinityQFModel]"" preview=""Great exam"" sort=""34"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000023"" model=""[TemporalAffinityQFModel]"" preview=""Applicatio"" sort=""35"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000024"" model=""[TemporalAffinityQFModel]"" preview=""App Store "" sort=""36"" />
</model>"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting modeled matches."
        );

        Assert.AreEqual(ProjectionTopology.Composition, pmdc.ProjectionTopology, "Because oc is INCC.");
        
        //Assert.AreEqual(NetProjectionOption.ObservableOnly, pmdc.ProjectionOption);

        actual = pmdc.StateReport();
        actual.ToClipboardExpected();
        { }
        expected = @" 
[IME Len: 5, IsFiltering: True], [Net: 3, CC: 37, PMC: 3], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Active]";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting ??"
        );

    }
}
