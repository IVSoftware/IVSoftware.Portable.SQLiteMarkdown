using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_ChainOfCustody
{
    [TestMethod]
    public void Test_SerializeToken()
    {
        using var te = this.TestableEpoch();

        string actual, expected;

        var entryCOC = new ChainOfCustodyToken();

        actual = JsonConvert.SerializeObject(entryCOC, Formatting.Indented);
        actual.ToClipboardExpected();
        { }
        expected = @" 
{
  ""LocalTimestamp"": ""2000-01-01T09:00:00+07:00"",
  ""RemoteTimestamp"": ""0001-01-01T00:00:00+00:00"",
  ""ModifiedFlags"": 0
}"
        ;

        Assert.AreEqual(
    expected.NormalizeResult(),
    actual.NormalizeResult(),
    "Expecting json serialization to succeed."
);
    }
}
