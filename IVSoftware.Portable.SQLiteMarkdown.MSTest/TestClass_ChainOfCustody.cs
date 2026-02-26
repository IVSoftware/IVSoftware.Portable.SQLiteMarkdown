using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_ChainOfCustody
{
    [TestMethod]
    public void Test_SerializeCOCToken()
    {
        using var te = this.TestableEpoch();

        string actual, expected;

        var cocToken = new ChainOfCustodyToken();

        actual = JsonConvert.SerializeObject(cocToken, Formatting.Indented);
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

    [TestMethod]
    public void Test_SerializeCOC()
    {
        using var te = this.TestableEpoch();
        string actual, expected;
        ChainOfCustody coc;

        coc = new ChainOfCustody();

        actual = JsonConvert.SerializeObject(coc, Formatting.Indented);
        actual.ToClipboardExpected();
        { }
        expected = @" 
[]"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting json serialization to succeed."
        );

        // Test empty loopback.
        Assert.IsNotNull(JsonConvert.DeserializeObject<ChainOfCustody>("[]"));
    }
}
