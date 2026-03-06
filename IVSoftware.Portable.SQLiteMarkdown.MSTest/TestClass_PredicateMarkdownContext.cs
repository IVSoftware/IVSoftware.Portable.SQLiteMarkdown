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
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" xitem=""[SelectableQFModel]"" />
</model>";

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
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000002"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000003"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000004"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000005"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000006"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000007"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000008"" xitem=""[SelectableQFModel]"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000009"" xitem=""[SelectableQFModel]"" />
</model>";

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
  <xitem text=""42480439-f88a-45b6-a107-67dd0042da25"" xitem=""[TemporalAffinityQFModel]"" preview=""Brown Dog "" sort=""0"" />
  <xitem text=""a6411079-280d-4f4f-9f85-f7e53438e218"" xitem=""[TemporalAffinityQFModel]"" preview=""Green Appl"" sort=""1"" ismatch=""True"" />
  <xitem text=""2d0285bf-6dec-4aea-8003-7b11cbecb2c6"" xitem=""[TemporalAffinityQFModel]"" preview=""Yellow Ban"" sort=""2"" />
  <xitem text=""115c5db9-9da4-468c-ab17-947e85394d1d"" xitem=""[TemporalAffinityQFModel]"" preview=""Blue Bird "" sort=""3"" />
  <xitem text=""06b8bf99-5d4e-4090-bb3d-475203fb2e2b"" xitem=""[TemporalAffinityQFModel]"" preview=""Red Cherry"" sort=""4"" />
  <xitem text=""a8c96871-7ee5-4795-901c-1c6289dd7928"" xitem=""[TemporalAffinityQFModel]"" preview=""Black Cat "" sort=""5"" />
  <xitem text=""bcf677c1-f364-4c51-8baa-46f3bd95d131"" xitem=""[TemporalAffinityQFModel]"" preview=""Orange Fox"" sort=""6"" />
  <xitem text=""3c3d088c-2569-43a4-beee-5c0e424a9504"" xitem=""[TemporalAffinityQFModel]"" preview=""White Rabb"" sort=""7"" />
  <xitem text=""e531d655-f473-4c21-a3fb-1e54cf39b7d1"" xitem=""[TemporalAffinityQFModel]"" preview=""Purple Gra"" sort=""8"" />
  <xitem text=""2d013ac8-0339-495f-9fc3-868fa4e8a782"" xitem=""[TemporalAffinityQFModel]"" preview=""Gray Wolf "" sort=""9"" />
  <xitem text=""b03961e7-e53b-4781-ae67-10ce0d0afc26"" xitem=""[TemporalAffinityQFModel]"" preview=""Pink Flami"" sort=""10"" />
  <xitem text=""d5db75fd-b461-4c57-966b-f5aaf8528779"" xitem=""[TemporalAffinityQFModel]"" preview=""Golden Lio"" sort=""11"" />
  <xitem text=""1b0211d0-65f6-47fd-9d6a-8a199af67ada"" xitem=""[TemporalAffinityQFModel]"" preview=""Brown Bear"" sort=""12"" />
  <xitem text=""c54a1c58-7248-487a-82e0-1df6ea0dc111"" xitem=""[TemporalAffinityQFModel]"" preview=""Green Pear"" sort=""13"" ismatch=""True"" />
  <xitem text=""942c8848-94ec-400c-a9c0-aaf797f95c01"" xitem=""[TemporalAffinityQFModel]"" preview=""Red Strawb"" sort=""14"" />
  <xitem text=""1b697ab7-ea51-49ed-acb9-80e83a07979b"" xitem=""[TemporalAffinityQFModel]"" preview=""Black Pant"" sort=""15"" />
  <xitem text=""0d87aa77-eaa9-459c-a292-1e6263256ad0"" xitem=""[TemporalAffinityQFModel]"" preview=""Yellow Lem"" sort=""16"" />
  <xitem text=""414aec47-77ab-4e43-a03d-5c4ace40b278"" xitem=""[TemporalAffinityQFModel]"" preview=""White Swan"" sort=""17"" />
  <xitem text=""578b842b-0566-48c1-b427-ffeb2ee26f77"" xitem=""[TemporalAffinityQFModel]"" preview=""Purple Plu"" sort=""18"" />
  <xitem text=""dece7374-94f3-40fd-a24b-72fba95bf621"" xitem=""[TemporalAffinityQFModel]"" preview=""Blue Whale"" sort=""19"" />
  <xitem text=""c3a75a5e-1ab8-498a-8585-c39885db2f3e"" xitem=""[TemporalAffinityQFModel]"" preview=""Elephant  "" sort=""20"" />
  <xitem text=""a2455baf-3168-4596-b4dc-417eae92a168"" xitem=""[TemporalAffinityQFModel]"" preview=""Pineapple "" sort=""21"" />
  <xitem text=""9f4187e5-6ad8-4e41-a755-209a883ec9a0"" xitem=""[TemporalAffinityQFModel]"" preview=""Shark     "" sort=""22"" />
  <xitem text=""c907c436-8986-4dd0-aee7-a3b836ded575"" xitem=""[TemporalAffinityQFModel]"" preview=""Owl       "" sort=""23"" />
  <xitem text=""ba4851f2-24fe-4297-b659-7e4877cfd06c"" xitem=""[TemporalAffinityQFModel]"" preview=""Giraffe   "" sort=""24"" />
  <xitem text=""6befeb7b-6090-4405-8a72-9056b8c9f852"" xitem=""[TemporalAffinityQFModel]"" preview=""Coconut   "" sort=""25"" />
  <xitem text=""0505679a-969e-405c-a164-9a2d22dec832"" xitem=""[TemporalAffinityQFModel]"" preview=""Kangaroo  "" sort=""26"" />
  <xitem text=""80f06e33-6a67-4025-86fb-829dd4cbf1ed"" xitem=""[TemporalAffinityQFModel]"" preview=""Dragonfrui"" sort=""27"" />
  <xitem text=""c0bd9f06-f8f9-4c43-b7a0-378fd2aa3511"" xitem=""[TemporalAffinityQFModel]"" preview=""Turtle    "" sort=""28"" />
  <xitem text=""0ee993f5-b4b1-433b-93cb-1fbf91d0b2a1"" xitem=""[TemporalAffinityQFModel]"" preview=""Mango     "" sort=""29"" />
  <xitem text=""50fef149-c3c3-4185-83f7-b9a329a82748"" xitem=""[TemporalAffinityQFModel]"" preview=""Should NOT"" sort=""30"" />
  <xitem text=""41a54da9-9c8e-471f-b3b0-b8de9fe51161"" xitem=""[TemporalAffinityQFModel]"" preview=""Appetizer "" sort=""31"" />
  <xitem text=""21a57e47-b417-474a-ad14-477dc54ede41"" xitem=""[TemporalAffinityQFModel]"" preview=""Errata    "" sort=""32"" ismatch=""True"" />
  <xitem text=""4c45f5ad-e91b-42d4-8db1-3c145adb32ce"" xitem=""[TemporalAffinityQFModel]"" preview=""Happy Camp"" sort=""33"" />
  <xitem text=""24f226b3-c944-4ef0-853d-49135360de2f"" xitem=""[TemporalAffinityQFModel]"" preview=""Great exam"" sort=""34"" />
  <xitem text=""25c06266-3c23-4e80-8c56-7e43f8069666"" xitem=""[TemporalAffinityQFModel]"" preview=""Applicatio"" sort=""35"" />
  <xitem text=""2807fdeb-0a6c-4110-ad9d-35196947374b"" xitem=""[TemporalAffinityQFModel]"" preview=""App Store "" sort=""36"" />
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
