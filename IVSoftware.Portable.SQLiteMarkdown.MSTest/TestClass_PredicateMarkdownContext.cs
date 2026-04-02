using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Linq;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

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


            actual = JsonConvert.SerializeObject(recordset, Newtonsoft.Json.Formatting.Indented);
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
        ObservableCollection<SelectableQFModel> opc = new();
        opc.PopulateForDemo(10);

        subtest_TriggerBy_ProjectionBeforeState();
        subtest_TriggerBy_StateBeforeProjection();
        subtest_TriggerBy_FilteringState();
        subtest_TriggerBy_RecordsetProperty();

        #region S U B T E S T S
        void subtest_TriggerBy_ProjectionBeforeState()
        {
            var mdc = new ModeledMarkdownContext<SelectableQFModel>();
            mdc.SetObservableNetProjection(opc);

            // In this test, the items are already populated
            // before switching into filter mode.
            mdc.QueryFilterConfig = QueryFilterConfig.Filter;
            Assert.IsTrue(mdc.IsFiltering, "Expecting ALWAYS TRUE in Filter mode.");

            actual = mdc.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mdc=""[MDC]"" histo=""[model:10 match:0 qmatch:0 pmatch:0]"" filters=""[No Active Filters]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""9"" />
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
            var mdc = new ModeledMarkdownContext<SelectableQFModel>
            {
                QueryFilterConfig = QueryFilterConfig.Filter,
            };
            Assert.IsTrue(mdc.IsFiltering, "Expecting ALWAYS TRUE in Filter mode.");
            actual = mdc.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mdc=""[MDC]"" histo=""[Histo]"" filters=""[No Active Filters]"" />"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting EMPTY because ONP is not assigned yet."
            );

            mdc.SetObservableNetProjection(opc);

            actual = mdc.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mdc=""[MDC]"" histo=""[model:10 match:0 qmatch:0 pmatch:0]"" filters=""[No Active Filters]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[SelectableQFModel]"" order=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[SelectableQFModel]"" order=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[SelectableQFModel]"" order=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[SelectableQFModel]"" order=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[SelectableQFModel]"" order=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[SelectableQFModel]"" order=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" model=""[SelectableQFModel]"" order=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" model=""[SelectableQFModel]"" order=""9"" />
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
    [TestMethod, DoNotParallelize, Ignore]
    public async Task Test_TemporalAffinityQFModel()
    {
        string actual, expected;
        using var te = this.TestableEpoch();
        var builder = new List<string>();
        int busyCount = 0;

        const bool INCLUDE_LIVE_DEMO = true;
        int COUNT = INCLUDE_LIVE_DEMO ? 37 : 31;

        var opc = new ObservableCollection<TemporalAffinityQFModel>();

        Assert.AreEqual(
            COUNT,  // Is conditional so check.
            opc.PopulateForDemo(includeLiveDemo: true).Count,
            "Expecting initial population.");

        // Filter-only MDC: Wakes up loaded with opc as canon.
        var pmdc = new PredicateMarkdownContext<TemporalAffinityQFModel>
        {
            QueryFilterConfig = QueryFilterConfig.Filter,
        };

        actual = pmdc.TopologyReport();
        actual.ToClipboardExpected();
        { }
        expected = @" 
NetProjectionTopology.None, ReplaceItemsEventingOption.StructuralReplaceEvent";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting initial."
        );

        pmdc.SetObservableNetProjection(opc);

        Assert.IsTrue(pmdc.IsFiltering);
        Assert.AreEqual(
            COUNT, 
            pmdc.CanonicalCount, 
            "Expecting CANONICAL COUNT is correct meaning canon is initialized.");


        actual = pmdc.StateReport();
        actual.ToClipboardExpected();
        { }
        if(INCLUDE_LIVE_DEMO)
        {
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 37, CC: 37, PMC: 0], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]";
        }
        else
        {
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 31, CC: 37, PMC: 0], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]";
        }

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting PREDICATE MATCH COUNT is 0 until filtering activity takes place."
        );

        Assert.AreEqual(
            FilteringState.Armed,
            pmdc.FilteringState);

        #region L o c a l F x
        void localOnModelUpdated(object? sender, NotifyCollectionChangedEventArgs e)
        {
            builder.Add(e.ToString(ReferenceEquals(sender, pmdc.ObservableNetProjection)));
            switch (pmdc.ProjectionTopology)
            {
                case NetProjectionTopology.ObservableOnly:
                    break;
                case NetProjectionTopology.AllowDirectChanges:
                    break;
                default:
                    this.ThrowHard<NotSupportedException>($"The {pmdc.ProjectionTopology.ToFullKey()} case is not supported.");
                    break;
            }
        }

        void localOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(pmdc.Busy):
                    if(pmdc.Busy) busyCount++;
                    break;
                default:
                    break;
            }
        }
        #endregion L o c a l F x

        using (pmdc.WithOnDispose(
            onInit: (sender, e) =>
            {
                pmdc.ModelChanged += localOnModelUpdated;
                pmdc.PropertyChanged += localOnPropertyChanged;
            },
            onDispose: (sender, e) =>
            {
                pmdc.ModelChanged -= localOnModelUpdated;
                pmdc.PropertyChanged -= localOnPropertyChanged;
            }))
        {
            actual = pmdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 37, CC: 37, PMC: 0], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting State Report to match.");

            actual = pmdc.TopologyReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
NetProjectionTopology.AllowDirectChanges, ReplaceItemsEventingOption.StructuralReplaceEvent"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting Options Report to match.");

            pmdc.InputText = "green";
            await pmdc;

            Assert.AreEqual(1, busyCount);
            Assert.IsFalse(pmdc.Busy);

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Other.Replace NewItems= 3 OldItems=37 ModelSettledEventArgs                      NotifyCollectionChangeReason.ApplyFilter"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ModelSettledEvent."
            );
        }

        actual = pmdc.Model.ToString();
        actual.ToClipboardExpected();
        { }
        expected = @" 
<model mdc=""[MDC]"" histo=""[model:37 match:3 qmatch:3 pmatch:0]"" filters=""[No Active Filters]"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[TemporalAffinityQFModel]"" preview=""Brown Dog "" order=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[TemporalAffinityQFModel]"" preview=""Green Appl"" order=""1"" qmatch=""True"" match=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" model=""[TemporalAffinityQFModel]"" preview=""Yellow Ban"" order=""2"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" model=""[TemporalAffinityQFModel]"" preview=""Blue Bird "" order=""3"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" model=""[TemporalAffinityQFModel]"" preview=""Red Cherry"" order=""4"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" model=""[TemporalAffinityQFModel]"" preview=""Black Cat "" order=""5"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" model=""[TemporalAffinityQFModel]"" preview=""Orange Fox"" order=""6"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" model=""[TemporalAffinityQFModel]"" preview=""White Rabb"" order=""7"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" model=""[TemporalAffinityQFModel]"" preview=""Purple Gra"" order=""8"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" model=""[TemporalAffinityQFModel]"" preview=""Gray Wolf "" order=""9"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000a"" model=""[TemporalAffinityQFModel]"" preview=""Pink Flami"" order=""10"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000b"" model=""[TemporalAffinityQFModel]"" preview=""Golden Lio"" order=""11"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000c"" model=""[TemporalAffinityQFModel]"" preview=""Brown Bear"" order=""12"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000d"" model=""[TemporalAffinityQFModel]"" preview=""Green Pear"" order=""13"" qmatch=""True"" match=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000e"" model=""[TemporalAffinityQFModel]"" preview=""Red Strawb"" order=""14"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000000f"" model=""[TemporalAffinityQFModel]"" preview=""Black Pant"" order=""15"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000010"" model=""[TemporalAffinityQFModel]"" preview=""Yellow Lem"" order=""16"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000011"" model=""[TemporalAffinityQFModel]"" preview=""White Swan"" order=""17"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000012"" model=""[TemporalAffinityQFModel]"" preview=""Purple Plu"" order=""18"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000013"" model=""[TemporalAffinityQFModel]"" preview=""Blue Whale"" order=""19"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000014"" model=""[TemporalAffinityQFModel]"" preview=""Elephant  "" order=""20"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000015"" model=""[TemporalAffinityQFModel]"" preview=""Pineapple "" order=""21"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000016"" model=""[TemporalAffinityQFModel]"" preview=""Shark     "" order=""22"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000017"" model=""[TemporalAffinityQFModel]"" preview=""Owl       "" order=""23"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000018"" model=""[TemporalAffinityQFModel]"" preview=""Giraffe   "" order=""24"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000019"" model=""[TemporalAffinityQFModel]"" preview=""Coconut   "" order=""25"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001a"" model=""[TemporalAffinityQFModel]"" preview=""Kangaroo  "" order=""26"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001b"" model=""[TemporalAffinityQFModel]"" preview=""Dragonfrui"" order=""27"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001c"" model=""[TemporalAffinityQFModel]"" preview=""Turtle    "" order=""28"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001d"" model=""[TemporalAffinityQFModel]"" preview=""Mango     "" order=""29"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001e"" model=""[TemporalAffinityQFModel]"" preview=""Should NOT"" order=""30"" />
  <xitem text=""312d1c21-0000-0000-0000-00000000001f"" model=""[TemporalAffinityQFModel]"" preview=""Appetizer "" order=""31"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000020"" model=""[TemporalAffinityQFModel]"" preview=""Errata    "" order=""32"" qmatch=""True"" match=""True"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000021"" model=""[TemporalAffinityQFModel]"" preview=""Happy Camp"" order=""33"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000022"" model=""[TemporalAffinityQFModel]"" preview=""Great exam"" order=""34"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000023"" model=""[TemporalAffinityQFModel]"" preview=""Applicatio"" order=""35"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000024"" model=""[TemporalAffinityQFModel]"" preview=""App Store "" order=""36"" />
</model>"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting modeled matches."
        );

        // Assert.AreEqual(ProjectionTopology.Composition, pmdc.ProjectionTopology, "Because oc is INCC.");

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
