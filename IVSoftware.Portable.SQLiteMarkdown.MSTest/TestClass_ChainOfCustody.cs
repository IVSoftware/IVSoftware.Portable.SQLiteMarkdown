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
    public async Task Test_SerializeCOC()
    {
        using var te = this.TestableEpoch();

        string actual, expected, localId = "D8BCD4F9-67C4-426A-B93A-B2885BFFC4CE";
        ChainOfCustody coc, cocLoopback;

        coc = new ChainOfCustody();

        actual = JsonConvert.SerializeObject(coc, Formatting.Indented);
        expected = @" 
{
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""Coc"": {}
}"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting json serialization to show EMPTY."
        );

        // Test empty loopback.
        cocLoopback = JsonConvert.DeserializeObject<ChainOfCustody>(actual)!;

        actual = JsonConvert.SerializeObject(cocLoopback, Formatting.Indented);

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting LIMIT IS UNCHANGED (loopback)"
        );

        // Now check out an edit token.
        await coc.CommitLocalEdit(localId);

        actual = JsonConvert.SerializeObject(coc, Formatting.Indented);
        actual.ToClipboardExpected();
        { }
        expected = @" 
{
  ""Created"": ""2000-01-01T09:00:00+07:00"",
  ""Coc"": {
    ""D8BCD4F9-67C4-426A-B93A-B2885BFFC4CE"": {
      ""LocalTimestamp"": ""2000-01-01T09:02:00+07:00"",
      ""RemoteTimestamp"": ""0001-01-01T00:00:00+00:00"",
      ""ModifiedFlags"": 0
    }
  }
}"
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting json serialization to succeed."
        );
    }
}
