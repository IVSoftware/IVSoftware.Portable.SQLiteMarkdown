using IVSoftware.Portable.SQLiteMarkdown.Common;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.V1
{
    [TestClass]
    public sealed class TestClass_V1
    {
        [TestMethod]
        public void Test_Capabilities()
        {
            MarkdownContext<SelectableQFModel> mdc = new();
            var cnx = mdc.MemoryDatabase;
        }
    }
}
