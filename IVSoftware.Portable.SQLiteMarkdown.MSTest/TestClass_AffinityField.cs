using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_AffinityField
    {

        [TestMethod]
        public void Test_AffinityQFModelBootstrap()
        {
            string actual, expected;
            DateTimeOffset utcTest = AffinityTestableEpoch.UtcReset;

            using var local = this.TestableEpoch();

            AffinityQFModel item;

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

            expected = @" 
{
  ""Position"": 630822888000000000,
  ""Path"": ""312d1c21-0000-0000-0000-000000000000"",
  ""IsRoot"": true,
  ""UtcStart"": null,
  ""Duration"": ""00:00:00"",
  ""Remaining"": ""00:00:00"",
  ""AffinityMode"": null,
  ""AffinityParent"": null,
  ""AffinityChildMode"": null,
  ""Slots"": [],
  ""AffinityTimeDomain"": null,
  ""UtcEnd"": null,
  ""IsDone"": null,
  ""IsDonePendingConfirmation"": null,
  ""IsPastDue"": null,
  ""Available"": null,
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""ChainOfCustody"": ""[]"",
  ""CustomProperties"": ""{}"",
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
            item.UpdateUtc(utcTest);


            actual = JsonConvert.SerializeObject(item, Formatting.Indented);
            actual.ToClipboardExpected();
            { }

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting json serialization to match."
            );
        }
    }
}
