using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_LoadCanon
{
    [TestMethod, DoNotParallelize]
    public void TestMethod_LoadCanon101()
    {
        // using IVSoftware.Portable.SQLiteMarkdown.Util;
        using var te = this.TestableEpoch();
        string actual, expected;

        var opc = new ObservableQueryFilterSource<SelectableQFModel>();
        opc.PopulateForDemo(5);

        actual = JsonConvert.SerializeObject(opc, Formatting.Indented);
        actual.ToClipboardExpected();
        { } // <- FIRST TIME ONLY: Adjust the message.
        actual.ToClipboardAssert("Expecting json serialization to match.");
        { }
        expected = @" 
[
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000000"",
    ""Description"": ""Item01"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000000"",
    ""QueryTerm"": ""item01"",
    ""FilterTerm"": ""item01"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item01\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000001"",
    ""Description"": ""Item02"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000001"",
    ""QueryTerm"": ""item02"",
    ""FilterTerm"": ""item02"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item02\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000002"",
    ""Description"": ""Item03"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000002"",
    ""QueryTerm"": ""item03"",
    ""FilterTerm"": ""item03"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item03\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000003"",
    ""Description"": ""Item04"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000003"",
    ""QueryTerm"": ""item04"",
    ""FilterTerm"": ""item04"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item04\""\r\n}""
  },
  {
    ""Id"": ""312d1c21-0000-0000-0000-000000000004"",
    ""Description"": ""Item05"",
    ""Keywords"": ""[]"",
    ""KeywordsDisplay"": """",
    ""Tags"": """",
    ""IsChecked"": false,
    ""Selection"": 0,
    ""IsEditing"": false,
    ""PrimaryKey"": ""312d1c21-0000-0000-0000-000000000004"",
    ""QueryTerm"": ""item05"",
    ""FilterTerm"": ""item05"",
    ""TagMatchTerm"": """",
    ""Properties"": ""{\r\n  \""Description\"": \""Item05\""\r\n}""
  }
]";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting json serialization to match."
        );
    }
}
