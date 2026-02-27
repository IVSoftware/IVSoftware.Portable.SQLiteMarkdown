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

            TemporalAffinityQFModel item;

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
  ""FullPath"": ""312d1c21-0000-0000-0000-000000000000"",
  ""ParentPath"": """",
  ""ParentId"": """",
  ""Duration"": ""00:00:00"",
  ""Remaining"": ""00:00:00"",
  ""TemporalAffinity"": null,
  ""TemporalChildAffinity"": null,
  ""TemporalAffinityCurrentTimeDomain"": null,
  ""Slots"": [],
  ""Priority"": 630822888000000000,
  ""PriorityOverride"": null,
  ""UtcStart"": null,
  ""UtcEnd"": null,
  ""AvailableTimeSpan"": null,
  ""IsRoot"": true,
  ""IsDone"": null,
  ""OutOfTime"": false,
  ""IsPastDue"": null,
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""ChainOfCustody"": ""{\r\n  \""Created\"": \""2000-01-01T09:00:00+07:00\"",\r\n  \""Coc\"": {}\r\n}"",
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
            // This is going to leave a mark...
            // item.UpdateAffinityUtcNow(utcTest);
            actual = JsonConvert.SerializeObject(item, Formatting.Indented);

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NO CHANGE."
            );
        }
    }
}
