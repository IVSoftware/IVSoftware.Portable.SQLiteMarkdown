using IVSoftware.Portable.Collections.Common;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_260325_AffinityField
    {

        [TestMethod, DoNotParallelize]
        public void Test_AffinityQFModelBootstrap()
        {
            string actual, expected, loopback;
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

            actual = JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
            actual.ToClipboardExpected();
            { }

            // Look for padded preview in Model: "New Item  " !!!
            expected = @" 
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
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""ChainOfCustody"": ""{\r\n  \""Created\"": \""2000-01-01T09:00:00+07:00\"",\r\n  \""Coc\"": {}\r\n}"",
  ""Model"": ""<model preview=\""New Item  \"" />"",
  ""FullPath"": ""312d1c21-0000-0000-0000-000000000000"",
  ""ParentPath"": """",
  ""ParentId"": """",
  ""Priority"": 630822888000000000,
  ""PriorityOverride"": null,
  ""IsRoot"": true,
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

            // Test idempotence by serializing again.
            actual = JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting json serialization to match."
            );
            loopback = @" 
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
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""ChainOfCustody"": ""{\r\n  \""Created\"": \""2000-01-01T09:00:00+07:00\"",\r\n  \""Coc\"": {}\r\n}"",
  ""Model"": ""<model preview=\""New Item  \"" />"",
  ""FullPath"": ""312d1c21-0000-0000-0000-000000000000"",
  ""ParentPath"": """",
  ""ParentId"": """",
  ""Priority"": 630822888000000000,
  ""PriorityOverride"": null,
  ""IsRoot"": true,
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
                expected, 
                loopback,
                "Expecting REGENERATED serialization matches original expect."
            );
        }
    }
}
