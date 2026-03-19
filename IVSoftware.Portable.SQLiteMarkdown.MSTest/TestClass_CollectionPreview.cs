using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_CollectionPreview
    {
        [TestMethod, DoNotParallelize]
        public void TestMethod_AddDynamicToFilteredCollection()
        {
            using var te = this.TestableEpoch();

            string actual, expected;

            Switcheroo.ObservableNetProjectionWithComposition<ItemCardModel> onp = new();
            var mdc = onp.Model.To<ModeledMarkdownContext<ItemCardModel>>();

            // F I L T E R    M O D E    ! ! ! !
            onp.QueryFilterConfig = QueryFilterConfig.Filter;

            #region M D C    B O O T S T R A P
            Assert.AreSame(
                onp,
                mdc.ObservableNetProjection,
                "Expecting opc is injected in the factory getter.");

            Assert.IsTrue(onp.IsFiltering, "ALWAYS TRUE in filter mode.");

            Type ct = mdc.ContractType;
            TableMapping mapping = ct.GetSQLiteMapping();
            Assert.AreEqual(
                $"{nameof(ItemCardModel)}",
                mapping.TableName,
                "Expecting correct table name mapping."
            );
            #endregion M D C    B O O T S T R A P

            // This will raise OnNetProjectionCollectionChanged with Projection authority
            onp.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });
            { }

            actual = JsonConvert.SerializeObject(onp, Formatting.Indented);
            actual.ToClipboardExpected();
            { }
            expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Brown Dog"",
    ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]"",
    ""KeywordsDisplay"": ""\""loyal\"",\""friend\"",\""furry\"""",
    ""Tags"": ""[canine] [color]"",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""FilterTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
    ""TagMatchTerm"": ""[canine] [color]"",
    ""Properties"": ""{\r\n  \""Description\"": \""Brown Dog\"",\r\n  \""Tags\"": \""[canine] [color]\"",\r\n  \""Keywords\"": \""[\\\""loyal\\\"",\\\""friend\\\"",\\\""furry\\\""]\""\r\n}""
  }
]";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting UI View has an item."
            );

            actual = onp.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model mmdc=""[MMDC]"" autocount=""1"" count=""1"" matches=""1"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[ItemCardModel]"" sort=""0"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            // IN PROGRESS
            actual = mdc.SerializeTopology();
            actual.ToClipboardExpected();
            { }
            expected = @" 
{
  ""ObservableNetProjection"": [
    {
      ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
      ""Description"": ""Brown Dog"",
      ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]"",
      ""KeywordsDisplay"": ""\""loyal\"",\""friend\"",\""furry\"""",
      ""Tags"": ""[canine] [color]"",
      ""IsChecked"": false,
      ""Selection"": ""None"",
      ""IsEditing"": false,
      ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
      ""QueryTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
      ""FilterTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
      ""TagMatchTerm"": ""[canine] [color]"",
      ""Properties"": ""{\r\n  \""Description\"": \""Brown Dog\"",\r\n  \""Tags\"": \""[canine] [color]\"",\r\n  \""Keywords\"": \""[\\\""loyal\\\"",\\\""friend\\\"",\\\""furry\\\""]\""\r\n}""
    }
  ],
  ""CanonicalSuperset"": [
    {
      ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
      ""Description"": ""Brown Dog"",
      ""Keywords"": ""[\""loyal\"",\""friend\"",\""furry\""]"",
      ""KeywordsDisplay"": ""\""loyal\"",\""friend\"",\""furry\"""",
      ""Tags"": ""[canine] [color]"",
      ""IsChecked"": false,
      ""Selection"": ""None"",
      ""IsEditing"": false,
      ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
      ""QueryTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
      ""FilterTerm"": ""brown~dog~loyal~friend~furry~[canine]~[color]"",
      ""TagMatchTerm"": ""[canine] [color]"",
      ""Properties"": ""{\r\n  \""Description\"": \""Brown Dog\"",\r\n  \""Tags\"": \""[canine] [color]\"",\r\n  \""Keywords\"": \""[\\\""loyal\\\"",\\\""friend\\\"",\\\""furry\\\""]\""\r\n}""
    }
  ],
  ""PredicateMatchSubset"": [],
  ""ProjectionTopology"": ""Composition"",
  ""ProjectionOption"": ""ObservableOnly"",
  ""ReplaceItemsEventingOptions"": ""StructuralReplaceEvent"",
  ""Count"": 0
}";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );


            actual = mdc.StateReport();
            actual.ToClipboardExpected();
            { }
            expected = @" 
[IME Len: 0, IsFiltering: True], [Net: 1, CC: 1, PMC: 1], [Filter: SearchEntryState.QueryCompleteWithResults, FilteringState.Armed]"
            ;
            Assert.AreEqual(expected.NormalizeResult(), actual.NormalizeResult(), "Expecting State Report to match.");
        }
    }
}

