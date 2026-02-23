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

            using var local = this.TestableEpoch();

            SelectableQFPrimeModel item;

            item = new();
            Assert.AreEqual(
                AffinityTestableEpoch.GuidReset.ToString(), 
                item.Id,
                "Expecting id initialized to first.");

            Assert.AreEqual(
                AffinityTestableEpoch.UtcReset,
                item.Created,
                "Expecting epoch initialized to first.");


            actual = JsonConvert.SerializeObject(item, Formatting.Indented);
            actual.ToClipboardExpected();
            { } // <- FIRST TIME ONLY: Adjust the message.
            actual.ToClipboardAssert("Expecting json serialization to match.");
            { }
            expected = @" 
{
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""CustomProperties"": {},
  ""ChainOfCustodyJSON"": ""[]"",
  ""Position"": 630822888000000000,
  ""Path"": """",
  ""UtcStart"": ""2000-01-01T09:00:00+07:00"",
  ""Duration"": null,
  ""Remaining"": null,
  ""AffinityMode"": null,
  ""UtcEnd"": null,
  ""IsDone"": false,
  ""IsPastDue"": null,
  ""Available"": null,
  ""AffinityParent"": """",
  ""AffinityChildMode"": null,
  ""AffinityTimeDomain"": null,
  ""Slots"": [],
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
}";

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
