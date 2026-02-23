using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_AffinityField
    {

        [TestMethod]
        public void Test_TestableEpoch()
        {
            string actual, expected;
            DateTimeOffset utcTest = AffinityTestableEpoch.UtcReset;

            using var local = this.TestableEpoch();

            SelectableQFAffinityModel item;

            item = new();
            Assert.AreEqual(
                AffinityTestableEpoch.GuidReset.ToString(), 
                item.Id,
                "Expecting id initialized to first.");

            Assert.AreEqual(
                utcTest,
                item.Created,
                "Expecting epoch initialized to first.");


            actual = JsonConvert.SerializeObject(item, Formatting.Indented);
            actual.ToClipboardExpected();
            { }

            expected = @" 
{
  ""Position"": 630822888000000000,
  ""Path"": """",
  ""UtcStart"": null,
  ""Duration"": ""00:00:00"",
  ""Remaining"": ""00:00:00"",
  ""AffinityMode"": null,
  ""AffinityParent"": """",
  ""AffinityChildMode"": null,
  ""Slots"": [],
  ""AffinityTimeDomain"": null,
  ""UtcEnd"": null,
  ""IsDone"": null,
  ""IsDonePendingConfirmation"": null,
  ""IsPastDue"": null,
  ""Available"": null,
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""CustomProperties"": {},
  ""ChainOfCustodyJSON"": ""[]"",
  ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
  ""Description"": ""New Item"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": """",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
  ""QueryTerm"": ""new~item"",
  ""FilterTerm"": ""new~item"",
  ""TagMatchTerm"": """",
  ""Properties"": ""{}""
}"
            ;

            expected = @" 
{
  ""Position"": 630822888000000000,
  ""Path"": """",
  ""UtcStart"": null,
  ""Duration"": ""00:00:00"",
  ""Remaining"": ""00:00:00"",
  ""AffinityMode"": null,
  ""UtcEnd"": null,
  ""IsDone"": false,
  ""IsPastDue"": null,
  ""Available"": null,
  ""AffinityParent"": """",
  ""AffinityChildMode"": null,
  ""AffinityTimeDomain"": null,
  ""Slots"": [],
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""CustomProperties"": {},
  ""ChainOfCustodyJSON"": ""[]"",
  ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
  ""Description"": ""New Item"",
  ""Keywords"": ""[]"",
  ""KeywordsDisplay"": """",
  ""Tags"": """",
  ""IsChecked"": false,
  ""Selection"": 0,
  ""IsEditing"": false,
  ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
  ""QueryTerm"": ""new~item"",
  ""FilterTerm"": ""new~item"",
  ""TagMatchTerm"": """",
  ""Properties"": ""{}""
}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting json serialization to match."
            );
        }


        [TestMethod]
        public void Test_AffinityFieldModel101()
        {
            string actual, expected;
        }
    }
}
