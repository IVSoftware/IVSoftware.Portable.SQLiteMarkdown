using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_Extensions
{
    [TestMethod]
    public void Test_GetAttributeValue()
    {
        XElement model = new(nameof(StdMarkdownElement.model));

        int @int = model.GetAttributeValue<int>(StdMarkdownAttribute.count);

    }
}
